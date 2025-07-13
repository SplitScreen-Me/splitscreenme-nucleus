using Ionic.Zip;
using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Nucleus.Coop.Forms
{
    public class HandlerScanner
    {              
        private string zipFile;
        private string prevSearchText = "";
        private const string searchFieldHint = "Search in all handlers";
        private string destPath = Globals.NucleusInstallRoot + "\\allhandlers";

        private List<string> zipsPath = new List<string>();
        private bool zipExtractFinished;
        private int numEntries;
        private int entriesDone = 0;

        private const char splitTag = '|';

        private Form form;
        private Label descLabel;
        private Label packageDescLabel;
        private Label progressLabel;
        private TextBox searchField;
        private ListView infoField;
        private RichTextBox contentView;
        private RichTextBox packageContentView;
        private ProgressBar progressBar;
        private PictureBox throbber;
        private AutoCompleteStringCollection autoCompleteSource = new AutoCompleteStringCollection();
        private Handler Handler;

        public void Initialize()
        {
            List<Handler> all = HubCache.GetAllHandlers();

            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            CreateUpdateForm();

            System.Threading.Tasks.Task.Run(() =>
            {
                List<string> downloaded = Directory.EnumerateFiles(destPath).Where
                (file => file.EndsWith(".js")).ToList();

                Dictionary<string, string> handlersVersion = new Dictionary<string, string>();

                for (int i = 0; i < downloaded.Count(); i++)
                {
                    string dl = downloaded[i];

                    var fileName = dl.Split('\\').Last();
                    string id = string.Empty;
                    string version = string.Empty;


                    foreach (string line in File.ReadAllLines(dl))
                    {
                        if (line.ToLower().StartsWith("hub.handler.version"))
                        {
                            int start = line.IndexOf("=");
                            int end = line.LastIndexOf(";");
                            version = line.Substring(start + 2, (end - start) - 2);
                        }

                        if (line.ToLower().StartsWith("hub.handler.id"))
                        {
                            int start = line.IndexOf("\"");
                            int end = line.LastIndexOf("\"");
                            id = line.Substring(start + 1, (end - start) - 1);
                        }
                    }

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version))
                    {
                        if(handlersVersion.Keys.Contains(id.ToLower()))
                        {
                            continue;
                        }

                        handlersVersion.Add(id.ToLower(), version.ToLower());
                    }
                }

                form.Invoke((MethodInvoker)delegate
                {
                    progressBar.Maximum = all.Count();
                });

                bool anyUpdate = false;

                for (int i = 0; i < all.Count; i++)
                {
                    Handler handler = all[i];

                    string zipFile = string.Format("handler-{0}-v{1}.nc", handler.Id, handler.CurrentVersion);
                    string zipFileVerplus1 = string.Format("handler-{0}-v{1}.nc", handler.Id, int.Parse(handler.CurrentVersion) + 1);
                    string zipFileVerminus1 = string.Format("handler-{0}-v{1}.nc", handler.Id, int.Parse(handler.CurrentVersion) - 1);


                    if (File.Exists(Path.Combine(destPath, zipFileVerplus1)))
                    {
                        File.Delete(Path.Combine(destPath, zipFileVerplus1));
                    }

                    if (File.Exists(Path.Combine(destPath, zipFileVerminus1)))
                    {
                        File.Delete(Path.Combine(destPath, zipFileVerminus1));
                    }

                    bool isValidZip = false;

                    if (File.Exists(Path.Combine(destPath, zipFile)))
                    {
                        try
                        {
                            isValidZip = ZipFile.CheckZip(Path.Combine(destPath, zipFile));
                        }
                        catch
                        {
                            File.Delete(Path.Combine(destPath, zipFile));
                            i--;
                            continue;
                        }
                    }

                    if (isValidZip)
                    {
                        anyUpdate = true;
                        zipsPath.Add(Path.Combine(destPath, zipFile));
                        continue;
                    }

                    form.Invoke((MethodInvoker)delegate
                    {
                        progressLabel.Text = (i + 1) + "/" + (all.Count - downloaded.Count());
                        progressLabel.Refresh();
                        descLabel.Text = "Checking: " + handler.GameName;
                        descLabel.Refresh();
                        progressBar.Value = i /** 100*/;
                    });

                    bool mustContinue = false;
                    bool hasUpdate = false;
                    bool noMatchingFile = true;///No js file to compare datas with cache so download

                    foreach (KeyValuePair<string, string> k in handlersVersion)
                    {
                        if (handler.Id.ToLower() == k.Key.ToLower())
                        {
                            if (handler.CurrentVersion.ToLower() == k.Value.ToLower())
                            {
                                form.Invoke((MethodInvoker)delegate
                                {
                                    descLabel.ForeColor = Color.GreenYellow;
                                    descLabel.Text = handler.GameName + " is up-to-date";
                                    descLabel.Refresh();
                                });

                                if (File.Exists(Path.Combine(destPath, zipFile)))
                                {
                                    File.Delete(Path.Combine(destPath, zipFile));
                                }

                                if (File.Exists(Path.Combine(destPath, zipFileVerplus1)))
                                {
                                    File.Delete(Path.Combine(destPath, zipFileVerplus1));
                                }

                                if (File.Exists(Path.Combine(destPath, zipFileVerminus1)))
                                {
                                    File.Delete(Path.Combine(destPath, zipFileVerminus1));
                                }

                                noMatchingFile = false;
                                mustContinue = true;
                                break;
                            }
                            else
                            {
                                noMatchingFile = false;
                                hasUpdate = true;
                                form.Invoke((MethodInvoker)delegate
                                {
                                    descLabel.ForeColor = Color.Orange;
                                    descLabel.Text = handler.GameName + "update available";
                                    descLabel.Refresh();
                                }); 
                            }
                        }
                    }

                    if (mustContinue)
                    {
                        continue;
                    }

                    if (hasUpdate || downloaded.Count() == 0 || noMatchingFile)
                    {
                        form.Invoke((MethodInvoker)delegate
                        {
                            descLabel.ForeColor = Color.White;
                            if (downloaded.Count() == 0)
                            {
                                form.Text = "Downloading Handlers...";
                                form.Refresh();
                            }
                            else
                            {
                                form.Text = "Updating Handlers...";
                                form.Refresh();
                            }

                            descLabel.Text = "Downloading handler: " + handler.GameName;
                            descLabel.Refresh();
                        });

                        HubCache.ThumbnailsCache(handler);

                        Download(handler, null);

                        anyUpdate = true;
                    }
                }

                if (anyUpdate)
                {
                    ExtractHandlers();//good
                    //CloseDownloadForm();
                }
                else
                {
                    CloseDownloadForm();
                }
            });
        }

        private Dictionary<string, string> cachedHandlers = new Dictionary<string, string>();

        private void CacheHandlersContent()
        {
            List<string> downloaded = Directory.EnumerateFiles(destPath).Where
                (file => file.EndsWith(".js")).ToList();
           
            for (int i = 0; i < downloaded.Count(); i++)
            {
                string dl = downloaded[i];

                var fileName = dl.Split('\\').Last();

                StringBuilder sb = new StringBuilder();

                foreach (string line in File.ReadAllLines(dl))
                {
                   sb.AppendLine(line);
                }

                cachedHandlers.Add(fileName.Replace(".js", "") , sb.ToString());
            }
        }

        private void CreateUpdateForm()
        {
            form = new Form
            {
                Text = "Checking Handlers...",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                ShowIcon = false,
                BackColor = Color.Black,
            };

            descLabel = new Label()
            {
                Size = new Size(10, 10),
                AutoSize = true,
                Location = new Point(20, 20),
                Font = new Font("Arial", 8, FontStyle.Regular),
                ForeColor = Color.White
            };

            form.Controls.Add(descLabel);

            progressLabel = new Label()
            {
                AutoSize = true,
                Location = new Point(20, 20),
                Font = new Font("Arial", 8, FontStyle.Regular),
                ForeColor = Color.White
            };

            form.Controls.Add(progressLabel);

            progressBar = new ProgressBar()
            {
                Location = new Point(20, 60),
                Size = new Size(360, 30),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            progressBar.BackColor = Color.DarkGray;
            progressBar.ForeColor = Color.Gray;
            progressLabel.Location = new Point(descLabel.Location.X, descLabel.Top - progressLabel.Height);

            form.FormClosed += OnDownloadFormClose;

            form.Controls.Add(progressBar);
            form.Show();
        }
  
        private void LoadAutoCompleteSource()
        {
            string sourceFile = Path.Combine(Globals.NucleusInstallRoot, "readme.txt");

            if (!File.Exists(sourceFile))
            {
                return;
            }

            List<string> protoLines = new List<string> {
                "Game.ProtoInput",
                "Game.ProtoInput.InjectStartup",
                "Game.ProtoInput.InjectRuntime_RemoteLoadMethod",
                "Game.ProtoInput.InjectRuntime_EasyHookMethod",
                "Game.ProtoInput.InjectRuntime_EasyHookStealthMethod",
                "Game.LockInputAtStart",
                "Game.LockInputSuspendsExplorer",
                "Game.LockInputToggleKey",
                "Game.ProtoInput.FreezeExternalInputWhenInputNotLocke",
                "Game.ProtoInput.RegisterRawInputHook",
                "Game.ProtoInput.GetRawInputDataHook",
                "Game.ProtoInput.MessageFilterHook",
                "Game.ProtoInput.GetCursorPosHook ",
                "Game.ProtoInput.SetCursorPosHook",
                "Game.ProtoInput.GetKeyStateHook",
                "Game.ProtoInput.GetAsyncKeyStateHook",
                "Game.ProtoInput.GetKeyboardStateHook",
                "Game.ProtoInput.CursorVisibilityHook",
                "Game.ProtoInput.DontShowCursorWhenImageUpdated",
                "Game.ProtoInput.ClipCursorHook",
                "Game.ProtoInput.ClipCursorHookCreatesFakeClip",
                "Game.ProtoInput.FocusHooks",
                "Game.ProtoInput.RenameHandles",
                "Game.ProtoInput.RenameNamedPipes",
                "Game.ProtoInput.XinputHook",
                "Game.ProtoInput.UseOpenXinput",
                "Game.ProtoInput.UseDinputRedirection",
                "Game.Hook.DInputEnabled",
                "Game.Hook.DInputForceDisable",
                "Game.Hook.XInputEnabled",
                "Game.Hook.XInputReroute",
                "Game.Hook.CustomDllEnabled",
                "Game.ProtoInput.DinputDeviceHook",
                "Game.ProtoInput.DinputHookAlsoHooksGetDeviceState",
                "Game.ProtoInput.SetWindowPosHook",
                "Game.ProtoInput.BlockRawInputHook",
                "Game.ProtoInput.BlockedMessages",
                "Game.ProtoInput.FindWindowHook",
                "Game.ProtoInput.CreateSingleHIDHook",
                "Game.ProtoInput.SetWindowStyleHook",
                "Game.ProtoInput.RawInputFilter",
                "Game.ProtoInput.MouseMoveFilter",
                "Game.ProtoInput.MouseActivateFilter",
                "Game.ProtoInput.WindowActivateFilter",
                "Game.ProtoInput.WindowActvateAppFilter",
                "Game.ProtoInput.MouseWheelFilter",
                "Game.ProtoInput.MouseButtonFilter",
                "Game.ProtoInput.KeyboardButtonFilter",
                "Game.ProtoInput.DrawFakeCursor",
                "Game.ProtoInput.AllowFakeCursorOutOfBounds",
                "Game.ProtoInput.ExtendFakeCursorBounds",
                "Game.ProtoInput.SendMouseWheelMessages",
                "Game.ProtoInput.SendMouseButtonMessages",
                "Game.ProtoInput.SendMouseMovementMessages",
                "Game.ProtoInput.SendKeyboardButtonMessages",
                "Game.ProtoInput.EnableFocusMessageLoop",
                "Game.ProtoInput.FocusLoopIntervalMilliseconds",
                "Game.ProtoInput.FocusLoop_WM_ACTIVATE",
                "Game.ProtoInput.FocusLoop_WM_SETFOCUS",
                "Game.ProtoInput.FocusLoop_WM_MOUSEACTIVATE",
                "Game.ProtoInput.FocusLoop_WM_ACTIVATEAPP",
                "Game.ProtoInput.FocusLoop_WM_NCACTIVATE",
                "ProtoInput.InstallHook",
                "ProtoInput.UninstallHook",
                "Game.ProtoInput.OnInputLocked",
                "Game.ProtoInput.OnInputUnlocked",
                "Game.LockInputSuspendsExplorer",
                "Game.ProtoInput.FreezeExternalInputWhenInputNotLocked",
                "ProtoInput.StartFocusMessageLoop",
                "ProtoInput.StopFocusMessageLoop",     
            };

            autoCompleteSource.AddRange(protoLines.ToArray());

            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (line == "" || line.StartsWith("#") || line.StartsWith("Nucleus Co-op") && ( !line.Contains("game.") && !line.Contains("context.") && !line.Contains("player.") && !line.Contains("system.") && !line.Contains("%NUCLEUS") && !line.Contains("var")))
                {
                    continue;
                }

                if (line.StartsWith("Known Issues:"))
                {
                    break;
                }

                string cleaned = line;

                if (cleaned.Contains('='))
                {
                    cleaned = cleaned.Split('=').First();
                }
           
                if (cleaned.Contains('('))
                {
                    cleaned = cleaned.Split('(').First();
                }       

                if (cleaned.Contains("/"))
                {
                    cleaned = cleaned.Split('/').First();
                }

                if (cleaned.Contains(";"))
                {
                    cleaned = cleaned.Split(';').First();
                }

                if (cleaned == "")
                {
                    continue;
                }
             
                if (cleaned != "")
                {
                    autoCompleteSource.Add(cleaned.TrimEnd());
                }        
            }            
        }

        private void CreateToolForm()
        {
            LoadAutoCompleteSource();

            form = new Form
            {
                Text = "Handlers Tool",
                Size = new Size(1280, 800),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                ShowIcon = false,
                BackColor = Color.FromArgb(50,50,50),
            };

            searchField = new TextBox();

            searchField.Location = new Point(20,10);
            searchField.BackColor = Color.FromArgb(255, 31, 34, 35);
            searchField.ForeColor = Color.Gray;
            searchField.Text = searchFieldHint;
            searchField.Font = new Font("Aharoni", 12, FontStyle.Italic);
            searchField.BorderStyle = BorderStyle.FixedSingle;
            searchField.Width = 300;

            searchField.AutoCompleteMode = AutoCompleteMode.Suggest;
            searchField.AutoCompleteSource = AutoCompleteSource.CustomSource;
            searchField.AutoCompleteCustomSource = autoCompleteSource;

            searchField.GotFocus += SearchFieldGotFocus;
            searchField.MouseDown += SearchFieldMouseDown;
            searchField.KeyUp += InputTemper;

            descLabel = new Label();

            descLabel.Size = new Size(10, 10);
            descLabel.AutoSize = true;
            descLabel.Location = new Point(20, searchField.Bottom + 10);
            descLabel.Font = new Font("Arial", 10, FontStyle.Regular);
            descLabel.ForeColor = Color.White;
            descLabel.Text = "Found: ..";

            infoField = new ListView();

            infoField.Location = new Point(20, descLabel.Bottom + 10);
            infoField.BackColor = Color.FromArgb(255, 31, 34, 35);
            infoField.ForeColor = Color.White;
            
            infoField.BorderStyle = BorderStyle.FixedSingle;
            infoField.Size = new Size(300, 670);
            infoField.AutoScrollOffset = new Point(280, 480);
            infoField.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            infoField.Scrollable = true;

            infoField.Font = new Font("Arial", 10, FontStyle.Regular);
            infoField.Alignment = ListViewAlignment.Left;
            infoField.MultiSelect = false;

            infoField.View = View.List;
            infoField.ItemActivate += ItemActivate;

            ImageList imageList = new ImageList
            {
                ImageSize = new Size(35, 35)
            };

            List<string> allImages = Directory.EnumerateFileSystemEntries(Globals.NucleusInstallRoot + @"\webview\cache\thumbnails\").ToList();

            List<Handler> all = HubCache.GetAllHandlers();
            int index = 0;
            foreach (string path in allImages)
            {
                try
                {
                    string imageName = path.Split('\\').Last().Replace(".jpeg", "").ToLower();
                    Handler tryGet = all.Where(h => h.Id.ToLower() == imageName).FirstOrDefault();
                    string imageKey = string.Empty;

                    if(tryGet != null)
                    {
                        imageKey = tryGet.Id;
                    }

                    if(imageKey != string.Empty)
                    imageList.Images.Add(imageKey, Image.FromFile(path));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load image: {path} - {ex.Message}");
                }

                index++;
            }

            infoField.SmallImageList = imageList;

            contentView = new RichTextBox();
            contentView.Font = new Font("Cascadia Code", 10, FontStyle.Regular);
            contentView.Location = new Point(infoField.Right + 10,10);
            contentView.BorderStyle = BorderStyle.FixedSingle;
            contentView.Size = new Size(920, 550);
            contentView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom| AnchorStyles.Right;
            contentView.Multiline = true;
            contentView.BackColor = Color.FromArgb(255, 31, 34, 35);
            contentView.ForeColor = Color.White;
            contentView.ReadOnly = true;

            packageDescLabel = new Label();
            packageDescLabel.Size = new Size(10, 10);
            packageDescLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            packageDescLabel.AutoSize = true;
            packageDescLabel.Location = new Point(contentView.Left, contentView.Bottom + 2);
            packageDescLabel.Font = new Font("Franklin Gothic", 11, FontStyle.Underline);
            packageDescLabel.ForeColor = Color.White;
            packageDescLabel.Text = "Handler package content";
            packageDescLabel.SizeChanged += OnDescPackageLabelSizeChanged;
            packageDescLabel.Visible = false;
            

            packageContentView = new RichTextBox();
            packageContentView.Font = new Font("Cascadia Code", 10, FontStyle.Regular);
            packageContentView.Location = new Point(contentView.Left, packageDescLabel.Bottom + 4);
            packageContentView.BorderStyle = BorderStyle.FixedSingle;
            packageContentView.Size = new Size(920, 150);
            packageContentView.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            packageContentView.Multiline = true;
            packageContentView.BackColor = Color.FromArgb(255, 31, 34, 35);
            packageContentView.ForeColor = Color.White;
            packageContentView.ReadOnly = true;
            packageContentView.Visible = false;
            packageContentView.AutoScrollOffset = new Point(0,0);

            throbber = new PictureBox
            {
                Size = new Size(50, 50),
                Anchor = AnchorStyles.None,
                Location = new Point((form.Width / 2) - 25, (form.Height / 2) - 25),
                BackColor = Color.FromArgb(255, 31, 34, 35),
                Image = ImageCache.GetImage(Globals.ThemeFolder + "loading.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };

            throbber.LocationChanged += ThrobberLocationChanged;

            form.Controls.Add(searchField);
            form.Controls.Add(descLabel);
            form.Controls.Add(infoField);
            form.Controls.Add(contentView);
            form.Controls.Add(packageDescLabel);
            form.Controls.Add(packageContentView);
            form.Controls.Add(throbber);
            form.Show();
            form.SizeChanged += OnFormSizeChanged;
            infoField.Focus();
        }

        private void OnDescPackageLabelSizeChanged(object sender, EventArgs e)
        {
            Label desc = sender as Label;
            packageContentView.Location = new Point(desc.Left, desc.Bottom + 8);
        }

        private void OnFormSizeChanged(object sender, EventArgs e)
        {
            throbber.Location = new Point((form.Width / 2) - 25, (form.Height / 2) - 25);
        }

        private void ShowThrobber()
        {
            throbber.Visible = true;
            throbber.BringToFront();
        }

        private void HideThrobber()
        {
            throbber.Visible = false;
        }

        private void ThrobberLocationChanged(object sender, EventArgs e)
        {
            PictureBox _throb = (PictureBox)sender;
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(new Rectangle(0, 0, _throb.Width, _throb.Height));
                _throb.Region = new Region(gp);
            }
        }

        private bool firstClick = false;

        private void SearchFieldMouseDown(object sebder ,EventArgs e)
        {
            firstClick = true;       
        }

        private void ItemActivate(object sender, EventArgs e)
        {
            contentView.SuspendLayout();
            contentView.Clear();

            var list = sender as ListView;
            if (list == null || list.SelectedItems.Count == 0)
                return;

            var items = list.SelectedItems[0];
            if (items == null || !(items.Tag is string rawTag))
                return;

            int splitIndex = rawTag.LastIndexOf(splitTag);
            if (splitIndex < 0)
                return;

            ListPackageContent(items.Text.Substring(2));

            string content = rawTag.Substring(0, splitIndex);
            string selectText = rawTag.Substring(splitIndex + 1);
            contentView.Text = content;

            HighlightJavaScript(contentView);
            HighlightWord(contentView,  selectText, Color.Gray);

            contentView.Select(0, 0);
            contentView.ResumeLayout();
        }

        private static readonly string[] JavaScriptKeywords = new[]
        {
             "break", "case", "const", "continue", "default",
             "do", "else", "finally", "for", "function", "if", "in",
             "let", "new", "return", "switch", "this", "var", "while","true","false","Count"
        };

        private static readonly string[] NucleusCommon = new[]
        {
            "Context", "Game", "ProtoInput", "Player","player", "Hook", "Hub", "System",      
        };

        private void HighlightJavaScript(RichTextBox box)
        {
            HighlightWords(box, JavaScriptKeywords, Color.DeepSkyBlue);
            HighlightWords(box, NucleusCommon, Color.PaleGreen);
            HighlightRegex(box, "\"(.*?)\"", Color.DarkOrange);//String
            HighlightRegex(box, @"//.*", Color.Green);//Comments
            HighlightRegex(box, @"/\*[\s\S]*?\*/", Color.Green); // Multi-line Comments
        }

        private void HighlightWord(RichTextBox box, string word, Color color)
        {
            bool scrolled = false;
            int startIndex = 0;
            while (startIndex < box.TextLength)
            {
                int foundIndex = box.Find(word, startIndex, RichTextBoxFinds.None);
                if (foundIndex == -1) break;

                box.Select(foundIndex, word.Length);
                box.SelectionBackColor = color;

                startIndex = foundIndex + word.Length;

                if (!scrolled)
                {
                    contentView.ScrollToCaret();
                    scrolled = true;
                }
            }
        }

        private void HighlightWords(RichTextBox box, string[] words, Color color)
        {
            foreach (string word in words)
            {
                int startIndex = 0;
                while (startIndex < box.TextLength)
                {
                    int foundIndex = box.Find(word, startIndex, RichTextBoxFinds.MatchCase | RichTextBoxFinds.WholeWord);
                    if (foundIndex == -1) break;

                    box.Select(foundIndex, word.Length);
                    box.SelectionColor = color;

                    startIndex = foundIndex + word.Length;
                }
            }
        }

        private void HighlightRegex(RichTextBox box, string pattern, Color color)
        {
            var matches = Regex.Matches(box.Text, pattern);
            foreach (Match match in matches)
            {
                box.Select(match.Index, match.Length);
                box.SelectionColor = color;
            }
        }

        private void SearchFieldGotFocus(object sender, EventArgs e)
        {
            if(!firstClick)
            {
                infoField.Focus();
                return;
            }

           if(searchField.Text == searchFieldHint)
           searchField.Text = "";
            searchField.Font = new Font("Aharoni", 12, FontStyle.Regular);
        }

        private void ListPackageContent(string directoryToScan)
        {
            string fullPath = Path.Combine(destPath, directoryToScan);
            packageDescLabel.Visible = false;
            packageContentView.Visible = false;

            packageContentView.Text = "";

            if (!Directory.Exists(fullPath))
            {
                return;
            }

            List<string> files = Directory.EnumerateFileSystemEntries(fullPath,"*",SearchOption.AllDirectories).ToList();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < files.Count(); i++)
            {
                string dl = files[i];

                var fileName = dl.Split('\\').ToList();

                int startIndex = fileName.IndexOf(directoryToScan);

                string clean = string.Empty;
                FileInfo info = new FileInfo(dl);

                for (int j = startIndex; j < fileName.Count; j++)
                {
                    string fileSize = string.Empty;
                    string details = string.Empty;

                    if (info.Exists)
                    {
                        fileSize = info.Length >= 1000000 ? " (~" + (info.Length / 1048576.0).ToString("F2") + " mo)" : " (~" + (info.Length / 1024.0).ToString("F2") + " ko)";
                        details = fileName[j] + "" + (info.Exists && info.Name.Equals(fileName[j]) ? fileSize : "");
                    }

                    clean += (j == startIndex ? @"" + details : @"\" + details);
                }

                if(info.Exists)//skip dirs
                sb.AppendLine(clean);
            }

            packageContentView.Text = sb.ToString();

            if(sb.Length > 0)
            {
                packageDescLabel.Visible = true;
                packageContentView.Visible = true;
            }
        }

        private bool ProcessSearch(TextBox search)
        {
            if (search.Text == "") {return false; }
            if (search.Text.StartsWith(" ")) { search.Text = ""; return false; }
            if (prevSearchText == searchFieldHint && search.Text == "") { return false; }
            if (prevSearchText == "" && search.Text == "") { return false; }
            if (search.Text == searchFieldHint && prevSearchText == "") { return false; }
            if (prevSearchText != "" && search.Text == searchFieldHint) { return false; }
            if (prevSearchText != "" && search.Text == searchFieldHint) { return false; }

            search.ForeColor = Color.White;
            prevSearchText = search.Text;

            return true;
        }

        private long temperValue = 0;

        private void InputTemper(object sender, EventArgs e)
        {
            TextBox sch = sender as TextBox;

            if(temperValue > 0 || sch.Text.Length < 3)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                temperValue = watch.ElapsedMilliseconds;
                return;
            }

            ShowThrobber();

            System.Threading.Tasks.Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                while (watch.ElapsedMilliseconds < 1500)
                {
                    temperValue = watch.ElapsedMilliseconds;
                    Thread.Sleep(100);
                }
       
                SearchFieldTextChanged(sender, e);              
                temperValue = 0;
            });
        }

        private void SearchFieldTextChanged(object sender, EventArgs e)
        {
            form.Invoke((MethodInvoker)delegate
            {
                TextBox sch = sender as TextBox;

                infoField.Items.Clear();

                if (!ProcessSearch(sch))
                {
                    descLabel.Text = "Found: .."  ;
                    return;
                }

                int matchCount = 0;

                if (cachedHandlers != null && cachedHandlers.Count > 0)
                {             
                    foreach (KeyValuePair<string, string> kv in cachedHandlers)
                    {
                        if (kv.Value.ToLower().Contains(sch.Text.ToLower()) /*|| kv.Value.Contains(sch.Text.Split('*').ToList().Any())*/)
                        {
                            matchCount++;
                            ListViewItem item = new ListViewItem();
   
                            var test = kv.Value.Split(';');
                            string imageKey = string.Empty;

                            foreach(string s in test)
                            {
                                if(s.Contains("Hub.Handler.Id"))
                                {
                                    int start = s.IndexOf("\"");
                                    int end = s.LastIndexOf("\"");
                                    imageKey = s.Substring(start + 1, (end - start) - 1);
                                    break;
                                }
                            }

                            if(imageKey != string.Empty)
                            {
                                item.ImageKey = imageKey;
                            }
                            
                            item.Text = "❖ " + kv.Key;
                            item.Tag = kv.Value + splitTag + sch.Text;
                            item.BackColor = Color.FromArgb(255, 31, 34, 35);
                            item.ForeColor = Color.Turquoise;

                            infoField.Items.Add(item);
                            infoField.SuspendLayout();
                        }
                    }

                    HideThrobber();
                    infoField.Visible = true;
                }

                descLabel.Text = "Found: " + matchCount;
            });
        }

        public void Download(Handler handler, string zipFileName)
        {
            try
            {
                Handler = handler;

                if (handler == null)
                {
                    return;
                }

                zipFile = string.Format("handler-{0}-v{1}.nc", Handler.Id, Handler.CurrentVersion);
                zipsPath.Add(zipFile);
                BeginDownload();
            }
            catch (Exception)
            {
            }
        }

        private void BeginDownload()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(
                    new System.Uri($@"https://hub.splitscreen.me/cdn/storage/packages/{Handler.CurrentPackage}/original/handler-{Handler.Id}-v{Handler.CurrentVersion}.nc"),
                    Path.Combine(destPath, zipFile)
                );
            }
        }

        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
            {
                entriesDone++;
                form.Invoke((MethodInvoker)delegate
                {
                    progressBar.Maximum = 100;
                    progressBar.Value = ((entriesDone / numEntries) * 100);
                    descLabel.Text = "Extracting handler " + ((entriesDone / numEntries) * 100) + "%";
                    descLabel.Refresh();
                });
            }

            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll || (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry && entriesDone == numEntries))
            {
                zipExtractFinished = true;
            }
        }

        private void ExtractHandlers()
        {
            int i = 1;

            foreach (string zipPath in zipsPath)
            {
                form.Invoke((MethodInvoker)delegate
                {
                    progressLabel.Text = i + "/" + zipsPath.Count;
                    progressLabel.Refresh();
                    descLabel.Text = "Extracting " + zipPath.Split('\\').Last();
                    descLabel.Refresh();
                });

                i++;
                ZipFile zip = new ZipFile(Path.Combine(destPath, zipPath));

                try
                {            
                    zip.ExtractProgress += ExtractProgress;
                    numEntries = zip.Entries.Count;

                    List<string> handlerFolders = new List<string>();

                    string scriptTempFolder = destPath + "\\temp";

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
                    string frmHandleTitle = pattern.Replace(zipPath, "");
                    string exeName = null;
                    int found = 0;

                    foreach (string line in File.ReadAllLines(Path.Combine(scriptTempFolder, "handler.js")))
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

                    if (Directory.Exists(Path.Combine(destPath, frmHandleTitle)))
                    {
                        Directory.Delete(Path.Combine(destPath, frmHandleTitle), true);
                    }

                    if (File.Exists(Path.Combine(destPath, frmHandleTitle + ".js")))
                    {
                        File.Delete(Path.Combine(destPath, frmHandleTitle + ".js"));
                    }

                    File.Move(Path.Combine(scriptTempFolder, "handler.js"), Path.Combine(destPath, frmHandleTitle + ".js"));

                    if (handlerFolders.Count > 0)
                    {
                        string gameFolder = Path.Combine(destPath, frmHandleTitle);

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

                    File.Delete(Path.Combine(destPath, zipPath));

                    numEntries = 0;
                    entriesDone = 0;
                }
                catch(Exception ex)
                {
                    zip?.Dispose();

                    numEntries = 0;
                    entriesDone = 0;

                    if (File.Exists(Path.Combine(destPath, zipPath)))
                    {
                        File.Delete(Path.Combine(destPath, zipPath));
                    }

                    form.Invoke((MethodInvoker)delegate
                    {
                        descLabel.ForeColor = Color.Red;
                        descLabel.Text = "File path is too long\n" + ex.Message;
                        descLabel.Refresh();
                    });

                    Thread.Sleep(2000);

                    form.Invoke((MethodInvoker)delegate
                    {
                        descLabel.ForeColor = Color.White;
                    });

                    continue;
                }

                numEntries = 0;
                entriesDone = 0;
            }

            if(Directory.Exists(Path.Combine(destPath,@"\Temp")))
            {
                Directory.Delete(Path.Combine(destPath, @"\Temp"));
            }

            CloseDownloadForm();
        }

        private void OnDownloadFormClose(object sender, EventArgs e)
        {
            CacheHandlersContent();
            CreateToolForm();
        }

        private void CloseDownloadForm()
        {
            form.Invoke((MethodInvoker)delegate
            {
                descLabel.Dispose();
                progressLabel.Dispose();
                progressBar.Dispose();
                form.Close();
            });
        }
    }
}
