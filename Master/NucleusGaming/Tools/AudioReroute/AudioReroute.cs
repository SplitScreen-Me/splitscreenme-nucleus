using Microsoft.Win32;
using Nucleus.Gaming.Platform.PCSpecs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Gaming.Tools.AudioReroute
{
    public static class AudioReroute
    {
        public static void SwitchProcessTo(string deviceId, ERole role, EDataFlow flow, uint processId)
        {
            var roles = new[]
           {
                ERole.eConsole,
                ERole.eCommunications,
                ERole.eMultimedia
            };

            if (role != ERole.ERole_enum_count)
            {
                roles = new[]
                {
                    role
                };
            }

            ComThread.Invoke((() =>
            {
                ExtendPolicyClient.SetDefaultEndPoint(deviceId, flow, roles, processId);
            }));
        }

        public enum ERole : uint
        {
            eConsole = 0,
            eMultimedia = (eConsole + 1),
            eCommunications = (eMultimedia + 1),
            ERole_enum_count = (eCommunications + 1)
        }

        public enum EDataFlow : uint
        {
            eRender = 0,
            eCapture = (eRender + 1),
            eAll = (eCapture + 1),
            EDataFlow_enum_count = (eAll + 1)
        }

        public enum HRESULT : uint
        {
            S_OK = 0x0,
            S_FALSE = 0x1,
            AUDCLNT_E_DEVICE_INVALIDATED = 0x88890004,
            AUDCLNT_S_NO_SINGLE_PROCESS = 0x889000d,
            ERROR_NOT_FOUND = 0x80070490,
        }

        internal class ExtendedPolicyClient
        {
            private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
            private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
            private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

            private IAudioPolicyConfigFactory _sharedPolicyConfig;
            private IAudioPolicyConfigFactory PolicyConfig
            {
                get
                {
                    if (_sharedPolicyConfig != null)
                    {
                        return _sharedPolicyConfig;
                    }

                    return _sharedPolicyConfig = AudioPolicyConfigFactory.Create();
                }
            }

            private IAudioPolicyConfigFactoryFor21H2 _sharedPolicyConfigFor21H2;
            private IAudioPolicyConfigFactoryFor21H2 PolicyConfigFor21H2
            {
                get
                {
                    if (_sharedPolicyConfigFor21H2 != null)
                    {
                        return _sharedPolicyConfigFor21H2;
                    }

                    return _sharedPolicyConfigFor21H2 = AudioPolicyConfigFactoryFor21H2.Create();
                }
            }

            private static string GenerateDeviceId(string deviceId, EDataFlow flow)
            {
                return $"{MMDEVAPI_TOKEN}{deviceId}{(flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
            }

            internal class ErrorConst
            {
                //FROM: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-
                public const ushort COM_ERROR_NOT_FOUND = 1168;
                public const int COM_ERROR_MASK = 0xFFFF;
            }

            public void SetDefaultEndPoint(string deviceId, EDataFlow flow, IEnumerable<ERole> roles, uint processId)
            {
                LogManager.Log($"ExtendedPolicyClient SetDefaultEndPoint {deviceId} [{flow}] {processId}");
                try
                {
                    var stringPtrDeviceId = IntPtr.Zero;

                    if (!string.IsNullOrWhiteSpace(deviceId))
                    {
                        var str = GenerateDeviceId(deviceId, flow);
                        ComBase.WindowsCreateString(str, (uint)str.Length, out stringPtrDeviceId);
                    }

                    if (MachineSpecs.Is21H2Update())
                    {
                        foreach (var eRole in roles)
                        {
                            PolicyConfigFor21H2.SetPersistedDefaultAudioEndpoint(processId, flow, eRole, stringPtrDeviceId);
                        }
                    }
                    else
                    {
                        foreach (var eRole in roles)
                        {
                            PolicyConfig.SetPersistedDefaultAudioEndpoint(processId, flow, eRole, stringPtrDeviceId);
                        }
                    }
                }
                catch (COMException e) when ((e.ErrorCode & ErrorConst.COM_ERROR_MASK) == ErrorConst.COM_ERROR_NOT_FOUND)
                {
                    //throw new DeviceNotFoundException($"Can't set default as {deviceId}", e, deviceId);
                    LogManager.Log($"(DeviceNotFound) Can't set default as {deviceId} - {e.Message}");
                }
                catch (Exception ex)
                {
                    LogManager.Log($"{ex}");
                }
            }

        }
        private static ExtendedPolicyClient _extendedPolicyClient;

        private static ExtendedPolicyClient ExtendPolicyClient
        {
            get
            {
                if (_extendedPolicyClient != null)
                {
                    return _extendedPolicyClient;
                }

                return _extendedPolicyClient = ComThread.Invoke(() => new ExtendedPolicyClient());
            }
        }


        internal sealed class ComTaskScheduler : TaskScheduler, IDisposable
        {
            /// <summary>The STA threads used by the scheduler.</summary>
            private readonly Thread _thread;

            /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
            private BlockingCollection<Task> _tasks;

            /// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
            public ComTaskScheduler()
            {
                // Initialize the tasks collection
                _tasks = new BlockingCollection<Task>();

                _thread = new Thread(() =>
                {
                    // Continually get the next task and try to execute it.
                    // This will continue until the scheduler is disposed and no more tasks remain.
                    foreach (var t in _tasks.GetConsumingEnumerable())
                    {
                        TryExecuteTask(t);
                    }

                    //lightweight pump of the thread
                    Thread.CurrentThread.Join(1);
                })
                { Name = "ComThread", IsBackground = true };

                _thread.SetApartmentState(ApartmentState.STA);

                // Start all of the threads
                _thread.Start();
            }

            public int ThreadId => _thread?.ManagedThreadId ?? -1;

            /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
            public override int MaximumConcurrencyLevel => 1;

            /// <summary>
            ///     Cleans up the scheduler by indicating that no more tasks will be queued.
            ///     This method blocks until all threads successfully shutdown.
            /// </summary>
            public void Dispose()
            {
                if (_tasks == null) return;

                // Indicate that no new tasks will be coming in
                _tasks.CompleteAdding();

                _thread.Join();

                // Cleanup
                _tasks.Dispose();
                _tasks = null;
            }

            /// <summary>Queues a Task to be executed by this scheduler.</summary>
            /// <param name="task">The task to be executed.</param>
            protected override void QueueTask(Task task)
            {
                // Push it into the blocking collection of tasks
                _tasks.Add(task);
            }

            /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
            /// <returns>An enumerable of all tasks currently scheduled.</returns>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                // Serialize the contents of the blocking collection of tasks for the debugger
                return _tasks.ToArray();
            }

            /// <summary>Determines whether a Task may be inlined.</summary>
            /// <param name="task">The task to be executed.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
            /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                //Never run inline, it HAS to be run on the COM thread
                return false;
            }
        }

        internal static class ComThread
        {
            private static bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != Scheduler.ThreadId;
            private static ComTaskScheduler Scheduler { get; } = new ComTaskScheduler();

            /// <summary>
            /// Asserts that the execution following this statement is running on the ComThreads
            /// <exception cref="InvalidThreadException">Thrown if the assertion fails</exception>
            /// </summary>
            public static void Assert()
            {
                if (InvokeRequired)
                    throw new Exception($"(InvalidThread)This operation must be run on the ComThread ThreadId: {Scheduler.ThreadId}");
            }

            public static void Invoke(Action action)
            {
                if (!InvokeRequired)
                {
                    action();
                    return;
                }

                BeginInvoke(action).Wait();
            }

            private static Task BeginInvoke(Action action)
            {
                return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, Scheduler);
            }

            public static T Invoke<T>(Func<T> func)
            {
                return !InvokeRequired ? func() : BeginInvoke(func).GetAwaiter().GetResult();
            }

            private static Task<T> BeginInvoke<T>(Func<T> func)
            {
                return Task<T>.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, Scheduler);
            }
        }


        [Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
        [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        public interface IAudioPolicyConfigFactory
        {
            int __incomplete__add_CtxVolumeChange();
            int __incomplete__remove_CtxVolumeChanged();
            int __incomplete__add_RingerVibrateStateChanged();
            int __incomplete__remove_RingerVibrateStateChange();
            int __incomplete__SetVolumeGroupGainForId();
            int __incomplete__GetVolumeGroupGainForId();
            int __incomplete__GetActiveVolumeGroupForEndpointId();
            int __incomplete__GetVolumeGroupsForEndpoint();
            int __incomplete__GetCurrentVolumeContext();
            int __incomplete__SetVolumeGroupMuteForId();
            int __incomplete__GetVolumeGroupMuteForId();
            int __incomplete__SetRingerVibrateState();
            int __incomplete__GetRingerVibrateState();
            int __incomplete__SetPreferredChatApplication();
            int __incomplete__ResetPreferredChatApplication();
            int __incomplete__GetPreferredChatApplication();
            int __incomplete__GetCurrentChatApplications();
            int __incomplete__add_ChatContextChanged();
            int __incomplete__remove_ChatContextChanged();
            [PreserveSig]
            HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
            [PreserveSig]
            HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
            [PreserveSig]
            HRESULT ClearAllPersistedApplicationDefaultEndpoints();
        }

        [Guid("ab3d4648-e242-459f-b02f-541c70306324")]
        [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        public interface IAudioPolicyConfigFactoryFor21H2
        {
            int __incomplete__add_CtxVolumeChange();
            int __incomplete__remove_CtxVolumeChanged();
            int __incomplete__add_RingerVibrateStateChanged();
            int __incomplete__remove_RingerVibrateStateChange();
            int __incomplete__SetVolumeGroupGainForId();
            int __incomplete__GetVolumeGroupGainForId();
            int __incomplete__GetActiveVolumeGroupForEndpointId();
            int __incomplete__GetVolumeGroupsForEndpoint();
            int __incomplete__GetCurrentVolumeContext();
            int __incomplete__SetVolumeGroupMuteForId();
            int __incomplete__GetVolumeGroupMuteForId();
            int __incomplete__SetRingerVibrateState();
            int __incomplete__GetRingerVibrateState();
            int __incomplete__SetPreferredChatApplication();
            int __incomplete__ResetPreferredChatApplication();
            int __incomplete__GetPreferredChatApplication();
            int __incomplete__GetCurrentChatApplications();
            int __incomplete__add_ChatContextChanged();
            int __incomplete__remove_ChatContextChanged();
            [PreserveSig]
            HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
            [PreserveSig]
            HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
            [PreserveSig]
            HRESULT ClearAllPersistedApplicationDefaultEndpoints();
        }

        internal sealed class ComBase
        {
            [DllImport("combase.dll", PreserveSig = false)]
            public static extern void RoGetActivationFactory(
                [MarshalAs(UnmanagedType.HString)] string activatableClassId,
                [In] ref Guid iid,
                [Out, MarshalAs(UnmanagedType.IInspectable)] out object factory);

            [DllImport("combase.dll", PreserveSig = false)]
            public static extern void WindowsCreateString(
                [MarshalAs(UnmanagedType.LPWStr)] string src,
                [In] uint length,
                [Out] out IntPtr hstring);
        }

        internal sealed class AudioPolicyConfigFactory
        {
            public static IAudioPolicyConfigFactory Create()
            {
                var iid = typeof(IAudioPolicyConfigFactory).GUID;
                ComBase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
                return (IAudioPolicyConfigFactory)factory;
            }
        }

        internal sealed class AudioPolicyConfigFactoryFor21H2
        {
            public static IAudioPolicyConfigFactoryFor21H2 Create()
            {
                var iid = typeof(IAudioPolicyConfigFactoryFor21H2).GUID;
                ComBase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
                return (IAudioPolicyConfigFactoryFor21H2)factory;
            }
        }

       
    }
}
