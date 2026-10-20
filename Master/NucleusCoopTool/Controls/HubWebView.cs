using Ionic.Zip;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nucleus.Gaming.Http_Client.Http_Client;

namespace Nucleus.Coop.Forms
{
    public partial class HubWebView : UserControl
    {
        private readonly string darkReaderDirectory = Path.Combine(Application.StartupPath, $"webview\\darkreader");
        private readonly string cacheDirectory= Path.Combine(Application.StartupPath, $"webview\\cache");
        private string downloadPath = Path.Combine(Application.StartupPath, "handlers\\handler.nc");
        private string scriptFolder = GameManager.Instance.GetJsScriptsPath();
        private readonly string hubUri = "https://hub.splitscreen.me/?fromWebview=true";
        private readonly string theme = Globals.ThemeFolder;

        private CoreWebView2DownloadOperation downloadOperation;
        private CoreWebView2Settings webViewSettings;
        private ZipFile zip;

        private bool zipExtractFinished;
        private bool downloadCompleted;
        private bool hasFreshCache;
        private bool inUserBrowser;

        private int entriesDone = 0;
        private int numEntries = 0;

        private EventHandler Modal_Yes_Button_Event;
        private EventHandler Modal_No_Button_Event;

        private List<JObject> installedHandlers = new List<JObject>();
        public bool Downloading { get; private set; }

        private Stopwatch timeoutWatch;

        public HubWebView()
        {
            OnHubConnectionChange += OnHubConnection_Lost;
            InitializeComponent();

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BackColor = Color.FromArgb(255, 31, 34, 35);

            modal.BackColor = BackColor;

            modalControlsContainer.BackColor = Color.FromArgb(255, 24, 26, 27);
            modal_yes.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            modal_yes.Cursor = Theme_Settings.Hand_Cursor;
            modal_no.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            modal_no.Cursor = Theme_Settings.Hand_Cursor;

            modal_text.BackColor = Color.FromArgb(255, 24, 26, 27);

            home.BackgroundImage = ImageCache.GetImage(theme + "home.png");
            home.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            home.BackColor = BackColor;
            home.Cursor = Theme_Settings.Hand_Cursor;

            back.BackgroundImage = ImageCache.GetImage(theme + "arrow_left.png");
            back.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            back.BackColor = BackColor;
            back.Cursor = Theme_Settings.Hand_Cursor;

            next.BackgroundImage = ImageCache.GetImage(theme + "arrow_right.png");
            next.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            next.BackColor = BackColor;
            next.Cursor = Theme_Settings.Hand_Cursor;

            closeBtn.BackgroundImage = ImageCache.GetImage(theme + "title_close.png");
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 0, 98, 190);
            closeBtn.BackColor = BackColor;
            closeBtn.Cursor = Theme_Settings.Hand_Cursor;

            webView.DefaultBackgroundColor = BackColor;

            button_Panel.BackColor = BackColor;
           
            ShowThrobber();

            string debugUri = Path.Combine(Application.StartupPath, $"webview\\debugUri.txt");

            if (File.Exists(debugUri))
            {
                StreamReader local = new StreamReader(debugUri);

                string expectedUri = "http://localhost:3000/";
                string descResult = local.ReadToEnd();

                if (expectedUri == descResult)
                {
                    hubUri = expectedUri;
                }

                local.Dispose();
            }

            Load += OnLoad;
            Disposed += DisposeContent;
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

            foreach (Control c in Controls)
            {
                c.Click += Generic_Functions.ClickAnyControl;
                if (c.HasChildren)
                {
                    foreach (Control child in c.Controls)
                    {
                        child.Click += Generic_Functions.ClickAnyControl;
                    }
                }
            }
         
            BuildHandlersDatas();
        }

        private PictureBox throbber;

        private void ShowThrobber()
        {
            if (throbber == null)
            {
                throbber = new PictureBox
                {
                    Size = new Size(50, 50),
                    BackColor = Color.FromArgb(255, 26, 29, 30),
                    Image = ImageCache.GetImage(Globals.ThemeFolder + "loading.gif"),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                throbber.LocationChanged += ThrobberLocationChanged;              
                Controls.Add(throbber);

                throbber.BringToFront();
            } 
        }

        protected void ThrobberLocationChanged(object sender,EventArgs e)
        {
            PictureBox _throb = (PictureBox)sender;
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(new Rectangle(0, 0, _throb.Width , _throb.Height));
                _throb.Region = new Region(gp);
            }
        }

        private void ThrobberDispose()
        {           
            if (throbber != null)
            {
                Controls.Remove(throbber);
                throbber?.Dispose();
                throbber = null;
            }
        }

        private void HubWebView_Resize(object sender, EventArgs e)
        {
            if(throbber != null)
            throbber.Location = RectangleUtil.Center(throbber.ClientRectangle, ClientRectangle).Location;
        }

        private void BuildHandlersDatas()
        {
            installedHandlers.Clear();

            foreach (UserGameInfo userGameInfo in GameManager.Instance.User.Games)
            {
                if(userGameInfo.HandlerInfo.GenericGameInfo == null)
                {
                    continue;
                }

                string id = userGameInfo.HandlerInfo.GenericGameInfo?.HandlerId;

                int version = userGameInfo.HandlerInfo.GenericGameInfo.Hub.Handler.Version;

                if (!string.IsNullOrEmpty(id) && version != -1)
                {
                    installedHandlers.Add(new JObject { new JProperty("id", id), new JProperty("version", version) });
                }
            }
        }

        private async void OnLoad(object sender, EventArgs e)
        {         
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {               
                CoreWebView2Environment environment;            
                CoreWebView2EnvironmentOptions environmentOptions = new CoreWebView2EnvironmentOptions();
                environmentOptions.AreBrowserExtensionsEnabled = true;
               
                webView.CreationProperties = new CoreWebView2CreationProperties();
                webView.CreationProperties.UserDataFolder = cacheDirectory;

                if (Directory.Exists(webView.CreationProperties.UserDataFolder))
                {
                    environment = await CoreWebView2Environment.CreateAsync(null, webView.CreationProperties.UserDataFolder, environmentOptions);
                    await webView.EnsureCoreWebView2Async(environment);

                    webView.Source = new Uri(hubUri);
                    hasFreshCache = false;
                    return;
                }

                hasFreshCache = true;

                environment = await CoreWebView2Environment.CreateAsync(null, webView.CreationProperties.UserDataFolder, environmentOptions);
                await webView.EnsureCoreWebView2Async(environment);
            }
            catch { Console.WriteLine("Webview init failed!"); }
        }

        private async void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            try
            {
                if (e.IsSuccess)
                {
                    webViewSettings = webView.CoreWebView2.Settings;
                    webViewSettings.IsWebMessageEnabled = true;
                    webViewSettings.IsGeneralAutofillEnabled = true;
#if RELEASE
            webViewSettings.AreDefaultContextMenusEnabled = false;
#endif
                    webViewSettings.IsScriptEnabled = true;

                    var get_Extensions = await webView.CoreWebView2.Profile.GetBrowserExtensionsAsync();

                    if (get_Extensions.All(ext => ext.Name != "Dark Reader"))
                    {
                        await webView.CoreWebView2.Profile.AddBrowserExtensionAsync(darkReaderDirectory);
                    }

                    CoreWebView2BrowserExtension DarkReader = get_Extensions.Where(ext => ext.Name == "Dark Reader").FirstOrDefault();

                    DarkReader?.EnableAsync(true);

                    if (webView.Source.AbsolutePath == "blank")
                    {
                        webView.Source = new Uri(hubUri);
                    }
               
                    webView.CoreWebView2.IsDefaultDownloadDialogOpenChanged += IsDefaultDownloadDialogOpenChanged;
                    webView.CoreWebView2.ContentLoading += ContentLoading;
                    webView.CoreWebView2.DOMContentLoaded += DOMContentLoaded;
                    webView.CoreWebView2.DownloadStarting += DownloadStarting;
                    webView.CoreWebView2.NewWindowRequested += NewWindowRequested;
                    webView.CoreWebView2.WebMessageReceived += WebMessageReceived;
                    webView.CoreWebView2.ProcessFailed += ProcessFailed;
                    webView.CoreWebView2.NavigationStarting += NavigationStarting;
                    webView.CoreWebView2.NavigationCompleted += NavigationCompleted;
                    webView.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
                    
                    BringToFront();
                }
            }
            catch { Console.WriteLine("Webview init failed!"); }
        }

        private void ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {

        }

        private void NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {        
            //if (e.IsSuccess)
            {
                timeoutWatch.Stop();
                inUserBrowser = false;
            }             
        }

        private void NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
             
            CoreWebView2 currentWindow = (CoreWebView2)sender;
             _ = TimeOutStopwatchAsync(15);
           
        }

        public async Task TimeOutStopwatchAsync(int seconds)
        {
            if(Downloading)
            {
                return;
            }

            timeoutWatch = Stopwatch.StartNew();

            while (timeoutWatch.Elapsed.TotalSeconds < seconds)
            {
                if(!timeoutWatch.IsRunning)
                {
                    return;
                }

                await Task.Delay(2000);
            }

            OnHubConnection_Lost(HubResponse.Error);
            timeoutWatch.Stop();
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            Console.WriteLine(e.WebMessageAsJson);
        }

        private void ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            this.Dispose();
        }

        private void NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            CoreWebView2 currentWindow = (CoreWebView2)sender;
            e.Handled = true;
            currentWindow.Navigate(e.Uri);
        }

        private async void SendDatas()
        {
            string json = JsonConvert.SerializeObject(installedHandlers, Formatting.Indented);
            await webView.ExecuteScriptAsync($"NucleusWebview.setLocalHandlerLibraryArray({json})");
            //string test = JsonConvert.SerializeObject(new string[] { "test message" }, Formatting.Indented);
            //await webView.ExecuteScriptAsync($"window.chrome.webview.postMessage({test});");
        }

        private void DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            if (hasFreshCache)
            {
                webView.CoreWebView2.Reload();
                hasFreshCache = false;
            }

            ThrobberDispose();
            webView.ZoomFactor = 0.8;
            SendDatas();
        }

        private void IsDefaultDownloadDialogOpenChanged(object sender, object e)
        {
            if (webView.CoreWebView2.IsDefaultDownloadDialogOpen) webView.CoreWebView2.CloseDefaultDownloadDialog();
        }

        private System.Windows.Forms.Timer downloadStateTimer;
        private int pending = 0;

        private void DownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            if (!webView.CoreWebView2.Source.StartsWith("https://hub.splitscreen.me/"))
            {
                e.DownloadOperation.Cancel();
                return;
            }

            if (downloadOperation == null)
            {
                Downloading = true;
                downloadStateTimer?.Dispose();
                downloadStateTimer = new System.Windows.Forms.Timer();
                downloadStateTimer.Interval = (150);
                downloadStateTimer.Tick += DownloadStateTimerTick;
                downloadStateTimer.Start();

                e.ResultFilePath = downloadPath;
                downloadOperation = e.DownloadOperation;
                downloadOperation.StateChanged += CheckDownloadState;

                modal_text.Text = "⠐ Downloading Handler Please Wait ⠂";
                modal_yes.Visible = false;
                modal_no.Visible = false;
                modal.Visible = true;
                modal.BringToFront();
            }
        }

        private void DownloadStateTimerTick(object Object, EventArgs EventArgs)
        {
            switch (pending)
            {
                case 0:
                    modal_text.Text = "⠒ Downloading Handler Please Wait ⠒";
                    pending++;
                    break;
                case 1:
                    modal_text.Text = "⠲ Downloading Handler Please Wait ⠖";
                    pending++;
                    break;
                case 2:
                    modal_text.Text = "⠴ Downloading Handler Please Wait ⠦";
                    pending++;
                    break;
                case 3:
                    modal_text.Text = "⠦ Downloading Handler Please Wait ⠴";
                    pending++;
                    break;
                case 4:
                    modal_text.Text = "⠶ Downloading Handler Please Wait ⠶";
                    pending++;
                    break;
                case 5:
                    modal_text.Text = "⠂ Downloading Handler Please Wait ⠐";
                    pending = 0;
                    break;
            }
        }

        private void CheckDownloadState(object sender, object e)
        {
            if (downloadOperation.State == CoreWebView2DownloadState.Completed && !downloadCompleted)
            {
                zipExtractFinished = false;
                entriesDone = 0;
                numEntries = 0;
                downloadCompleted = true;
                downloadOperation = null;
                downloadStateTimer.Dispose();
                pending = 0;
                ExtractHandler();
            }
            else if (downloadOperation.State == CoreWebView2DownloadState.Interrupted)
            {
                Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Warning}|The download has been interrupted.@*");
                downloadStateTimer?.Dispose();
                downloadOperation = null;
                HideModal();         
            }
        }

        private void ExtractHandler()
        {
            zip = new ZipFile(downloadPath);

            zip.ExtractProgress += ExtractProgress;
            numEntries = zip.Entries.Count;

            List<string> handlerFolders = new List<string>();

            string scriptTempFolder = scriptFolder + "\\temp";

            if (!Directory.Exists(scriptTempFolder))
            {
                Directory.CreateDirectory(scriptTempFolder);
            }

            foreach (ZipEntry ze in zip)
            {
                if (ze.IsDirectory)
                {
                    int count = 0;
                    foreach (char c in ze.FileName)
                    {
                        if (c == '/')
                        {
                            count++;
                        }
                    }

                    if (count == 1)
                    {
                        handlerFolders.Add(ze.FileName.TrimEnd('/'));
                    }

                    ze.Extract(scriptTempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    ze.Extract(scriptTempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            Regex pattern = new Regex("[\\/:*?\"<>|]");
            string frmHandleTitle = pattern.Replace(downloadPath, "");
            string exeName = null;
            int found = 0;

            foreach (string line in System.IO.File.ReadAllLines(Path.Combine(scriptTempFolder, "handler.js")))
            {
                if (line.ToLower().StartsWith("game.executablename"))
                {
                    int start = line.IndexOf("\"");
                    int end = line.LastIndexOf("\"");
                    exeName = line.Substring(start + 1, (end - start) - 1);
                    found++;
                }
                else if (line.ToLower().StartsWith("game.gamename"))
                {
                    int start = line.IndexOf("\"");
                    int end = line.LastIndexOf("\"");
                    frmHandleTitle = pattern.Replace(line.Substring(start + 1, (end - start) - 1), "");
                    found++;
                }

                if (found == 2)
                {
                    break;
                }
            }

            if (File.Exists(Path.Combine(scriptFolder, frmHandleTitle + ".js")))
            {
                modal_text.Text = "A handler with the name " + frmHandleTitle + ".js already exists.\nDo you wish to overwrite it?";

                modal_yes.Visible = true;
                Modal_Yes_Button_Event = (sender, e) => ModalAddHandler(zip, scriptTempFolder, frmHandleTitle, handlerFolders, exeName);
                modal_yes.Click += Modal_Yes_Button_Event;

                modal_no.Visible = true;
                Modal_No_Button_Event = (sender, e) => Abort(zip, scriptTempFolder, frmHandleTitle, handlerFolders, exeName);
                modal_no.Click += Modal_No_Button_Event;

                modal.Visible = true;
                modal.BringToFront();
                return;
            }

            ModalAddHandler(zip, scriptTempFolder, frmHandleTitle, handlerFolders, exeName);
        }

        private void Abort(ZipFile zip, string scriptTempFolder, string frmHandleTitle, List<string> handlerFolders, string exeName)
        {
            zip.Dispose();
            Directory.Delete(scriptTempFolder, true);
            File.Delete(downloadPath);
            HideModal();
        }

        private void AddGameToList(string frmHandleTitle, string exeName)
        {          
            HandlerInfo handlerInfo = Core_Interface.GameManager.HandlerInfos.Where(aggi => aggi.FileName == frmHandleTitle + ".js").FirstOrDefault();
            _ = SearchGame.Search(exeName, handlerInfo);
            HideModal();
            closeBtn.PerformClick();
        }

        private void ModalAddHandler(ZipFile zip, string scriptTempFolder, string frmHandleTitle, List<string> handlerFolders, string exeName)
        {
            modal_yes.Visible = true;
            modal_yes.Click -= Modal_Yes_Button_Event;

            modal_no.Visible = true;
            modal_no.Click -= Modal_No_Button_Event;

            if (Directory.Exists(Path.Combine(scriptFolder, frmHandleTitle)))
            {
                Directory.Delete(Path.Combine(scriptFolder, frmHandleTitle), true);
            }

            if (File.Exists(Path.Combine(scriptFolder, frmHandleTitle + ".js")))
            {
                File.Delete(Path.Combine(scriptFolder, frmHandleTitle + ".js"));
            }

            File.Move(Path.Combine(scriptTempFolder, "handler.js"), Path.Combine(scriptFolder, frmHandleTitle + ".js"));

            if (handlerFolders.Count > 0)
            {
                string gameFolder = Path.Combine(scriptFolder, frmHandleTitle);

                Directory.CreateDirectory(gameFolder);

                foreach (string hFolder in handlerFolders)
                {
                    string newFolder = Path.Combine(scriptTempFolder, hFolder);

                    foreach (string dir in Directory.GetDirectories(newFolder, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(Path.Combine(gameFolder, dir.Substring(newFolder.Length + 1)));
                    }

                    foreach (string file_name in Directory.GetFiles(newFolder, "*", SearchOption.AllDirectories))
                    {
                        File.Move(file_name, Path.Combine(gameFolder, file_name.Substring(newFolder.Length + 1)));
                    }

                    Directory.Delete(newFolder, true);
                }
            }

            Directory.Delete(scriptTempFolder, true);

            while (!zipExtractFinished)
            {
                Application.DoEvents();
            }

            zip.Dispose();

            File.Delete(downloadPath);

            bool update = GameManager.Instance.AddNewGenericGameInfo(frmHandleTitle, true);

            if (update)
            {
                BuildHandlersDatas();
                SendDatas();              
                Core_Interface.RefreshGamesList();
                HideModal();
                Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|{frmHandleTitle}@* *@{OSDHighlightColors.Regular}|handler has been updated successfully@*");
                return;
            }
          
            BuildHandlersDatas();
            SendDatas();

            Modal_Yes_Button_Event = (sender, e) => AddGameToList(frmHandleTitle, exeName);
            modal_yes.Click += Modal_Yes_Button_Event;
            Modal_No_Button_Event = (sender, e) => HideModal();
            modal_no.Click += Modal_No_Button_Event;

            modal_text.Text =
            "Downloading and extraction of " + frmHandleTitle +
            " handler is complete. Would you like to add this game to Nucleus now? " +
            "You will need to select the game executable to add it.";

            if (!modal.Visible)
            {
                modal.Visible = true;
                modal.BringToFront();
            }
        }

        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
            {
                entriesDone++;
            }

            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll || (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry && entriesDone == numEntries))
            {
                zipExtractFinished = true;
            }
        }

        private void HideModal()
        {
            modal_yes.Click -= Modal_Yes_Button_Event;
            modal_no.Click -= Modal_No_Button_Event;
            modal.Visible = false;
            downloadCompleted = false;
            modal.SendToBack();
            Downloading = false;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.GoBack();
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 != null)
                webView.CoreWebView2.GoForward();
        }

        private void Home_Click(object sender, EventArgs e)
        {
            if (webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(hubUri);
            }
        }

        public void CloseBtn_Click(object sender, EventArgs e)
        {
            downloadStateTimer?.Dispose();
            webView?.Dispose();
            Dispose();
        }

        private void DisposeContent(object sender, EventArgs e)
        {
            OnHubConnectionChange -= OnHubConnection_Lost;

            zip?.Dispose();

            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
    
            webView?.Dispose();
        }   

        private void OnHubConnection_Lost(HubResponse resp)
        {
            if(resp == HubResponse.Error)
            {
                Globals.MainOSD.Show(3000, $"*@{OSDHighlightColors.Info}|Hub connection timeout@*");
                OnHubConnectionChange -= OnHubConnection_Lost;
                Downloading = false;

                UIThread.Post(() =>
                {
                    CloseBtn_Click(null, null);
                });
            }
        }
    }
}
