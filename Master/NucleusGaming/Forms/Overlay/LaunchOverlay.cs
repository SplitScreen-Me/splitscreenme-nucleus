using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Nucleus.Gaming.Forms.Overlay
{
    /// <summary>
    /// Per-player launch cards over the game screens: "Starting…" on the player's region when its instance starts,
    /// "Ready" when the setup finishes, then they fade out. They replace the on-screen display texts
    /// "Starting {game} instance for {nickname} as Player #{n}" and "Setup Finished", which still reach the app
    /// (<see cref="WPF_OSD.MessageShown"/>). Callable from any thread, never waits: the cards live on their own STA thread.
    /// </summary>
    internal static class LaunchOverlay
    {
        private const int StartingNoticeMs = 1200;
        private const int FinishedNoticeMs = 1600;

        private static readonly object sync = new object();
        private static OverlaySession current;

        /// <summary>
        /// A player's instance is starting: shows its card. Shows <paramref name="notice"/> on the on-screen display
        /// instead when the card can't be shown.
        /// </summary>
        public static void PlayerStarting(PlayerInfo player, string gameName, string notice)
        {
            bool shown = false;

            try
            {
                if (player != null && player.MonitorBounds.Width > 0 && player.MonitorBounds.Height > 0)
                {
                    PlayerCardModel model = PlayerCardModel.From(player, gameName);
                    Session(true).Starting(model);
                    shown = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Launch overlay: can't show the player card ({ex.Message})");
            }

            if (shown)
            {
                WPF_OSD.Mirror(StartingNoticeMs, notice);
            }
            else
            {
                Globals.MainOSD?.Show(StartingNoticeMs, notice);
            }
        }

        /// <summary>
        /// The setup finished: every card turns "Ready" and fades out. False when no card is shown,
        /// the caller then shows its own text.
        /// </summary>
        public static bool SetupFinished(string notice)
        {
            OverlaySession session = Session(false);

            if (session == null || !session.HasCards)
            {
                return false;
            }

            session.Finished();
            WPF_OSD.Mirror(FinishedNoticeMs, notice);
            return true;
        }

        private static OverlaySession Session(bool create)
        {
            lock (sync)
            {
                GenericGameHandler handler = GenericGameHandler.Instance;

                if (current != null && !current.IsClosing && current.Owner == handler)
                {
                    return current;
                }

                if (!create)
                {
                    return null;
                }

                current?.CloseAll(0);//a card left from another session
                current = new OverlaySession(handler);
                return current;
            }
        }

        internal static void Forget(OverlaySession session)
        {
            lock (sync)
            {
                if (current == session)
                {
                    current = null;
                }
            }
        }
    }

    /// <summary>The cards of one launch, on one STA thread that ends with the last card.</summary>
    internal sealed class OverlaySession
    {
        private const int ReadyHoldMs = 2000;
        private const int ExitMs = 380;
        private static readonly TimeSpan MaxLifetime = TimeSpan.FromMinutes(20);

        private readonly object sync = new object();
        private readonly List<Action> pending = new List<Action>();
        private readonly Dictionary<int, PlayerCardWindow> cards = new Dictionary<int, PlayerCardWindow>();//overlay thread only
        private readonly IntPtr dpiContext;
        private readonly DateTime started = DateTime.UtcNow;
        private Dispatcher dispatcher;
        private DispatcherTimer watch;
        private int queued;
        private volatile bool closing;
        private bool shutDown;

        /// <summary>Session the cards belong to: they close when it ends.</summary>
        public GenericGameHandler Owner { get; }

        public bool IsClosing => closing;

        public bool HasCards => Volatile.Read(ref queued) > 0;

        public OverlaySession(GenericGameHandler owner)
        {
            Owner = owner;
            //Same DPI awareness as the launch thread: the regions are in its coordinates.
            dpiContext = OverlayNative.CurrentDpiContext();

            Thread thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "Nucleus launch overlay",
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void Starting(PlayerCardModel model)
        {
            Interlocked.Increment(ref queued);
            Post(() => ShowStarting(model));
        }

        public void Finished()
        {
            closing = true;
            Post(ShowReady);
        }

        public void CloseAll(int fadeMs)
        {
            closing = true;
            Post(() => FadeAll(fadeMs));
        }

        private void Run()
        {
            OverlayNative.UseDpiContext(dpiContext);

            Dispatcher own = Dispatcher.CurrentDispatcher;
            own.UnhandledException += (s, e) =>
            {
                LogManager.Log($"Launch overlay: {e.Exception.Message}");
                e.Handled = true;
            };

            watch = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, (s, e) => Watch(), own);
            watch.Start();

            lock (sync)
            {
                dispatcher = own;

                foreach (Action action in pending)
                {
                    own.BeginInvoke(action);
                }

                pending.Clear();
            }

            try
            {
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                LogManager.Log($"Launch overlay: {ex.Message}");
            }
        }

        private void Post(Action action)
        {
            Action safe = () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    LogManager.Log($"Launch overlay: {ex.Message}");
                }
            };

            lock (sync)
            {
                if (dispatcher == null)
                {
                    pending.Add(safe);
                    return;
                }
            }

            dispatcher.BeginInvoke(safe);
        }

        private void ShowStarting(PlayerCardModel model)
        {
            if (shutDown)
            {
                return;
            }

            if (cards.TryGetValue(model.Number, out PlayerCardWindow shown))
            {
                shown.Card.SetStarting();
                return;
            }

            PlayerCardWindow window = new PlayerCardWindow(model);
            cards[model.Number] = window;

            window.Closed += (s, e) =>
            {
                if (cards.TryGetValue(window.Number, out PlayerCardWindow same) && same == window)
                {
                    cards.Remove(window.Number);
                }

                if (cards.Count == 0 && closing)
                {
                    Shutdown();
                }
            };

            window.ShowOverlay();
        }

        private void ShowReady()
        {
            if (cards.Count == 0)
            {
                Shutdown();
                return;
            }

            int index = 0;

            foreach (PlayerCardWindow window in cards.Values.OrderBy(c => c.Number).ToList())
            {
                int delay = Math.Min(index++ * 70, 560);//a short wave across the screens

                window.Raise();//above game windows made topmost at the end of the setup
                After(delay, () => window.Card.SetReady(true));
                After(delay + ReadyHoldMs, () => window.Card.Exit(ExitMs, window.Dismiss));
            }
        }

        private void FadeAll(int fadeMs)
        {
            if (cards.Count == 0)
            {
                Shutdown();
                return;
            }

            foreach (PlayerCardWindow window in cards.Values.ToList())
            {
                if (fadeMs <= 0)
                {
                    window.Dismiss();
                }
                else
                {
                    window.Card.Exit(fadeMs, window.Dismiss);
                }
            }
        }

        /// <summary>The session ended or another one started (or something went wrong): no card stays behind.</summary>
        private void Watch()
        {
            bool ended = Owner != null && (Owner.hasEnded || GenericGameHandler.Instance != Owner);

            if (ended || DateTime.UtcNow - started > MaxLifetime)
            {
                watch.Stop();
                closing = true;
                FadeAll(160);
            }
        }

        private void After(int ms, Action action)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher) { Interval = TimeSpan.FromMilliseconds(Math.Max(1, ms)) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                if (!shutDown)
                {
                    action();
                }
            };
            timer.Start();
        }

        private void Shutdown()
        {
            if (shutDown)
            {
                return;
            }

            shutDown = true;
            closing = true;
            watch?.Stop();
            LaunchOverlay.Forget(this);
            dispatcher?.BeginInvokeShutdown(DispatcherPriority.Background);
        }
    }
}
