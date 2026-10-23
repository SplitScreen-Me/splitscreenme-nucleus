using Ionic.Zip;
using Newtonsoft.Json;
using Nucleus.Coop.Controls;
using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using System.Windows.Forms.Integration;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Threading.Tasks;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Http_Client;
using static Nucleus.Gaming.Http_Client.Http_Client;
using static Nucleus.Gaming.Forms.SearchProgress;
using System.Windows.Documents;
using Nucleus.Gaming.Windows;


namespace Nucleus.Coop.Forms
{
    //Must check if the game executable as changed between update/edit/etc to cover more edge cases
    //Maybe add a file supervisor for the handler content files changes too?would that be to demanding??
    //Maybe add the broken files as item too(red)(Almost done maybe check if the file (zip or js has changed (it's content "integrity" as changed )))
    //Add fileinfo blacklist (List<FileInfo>) when accessing a file fail (corrupted,bad zip format, handler with errors , blocked by av)
    public class HandlersManager 
    {
        // private float scale;
        //private float scale
        //{
        //    get
        //    {
        //        if (_scale < 1.0f)
        //        {
        //            return 1.0f;
        //        }
        //        else
        //        {
        //            return _scale;
        //        }
        //    }
        //    set;
        //}
        private float scale = UI_Interface.Scale;
        public static HandlersManager Instance;

        public class SmoothListView : ListView
        {
            public SmoothListView()
            {
                // reduces flicker on hover and scroll
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
                this.UpdateStyles();
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);

                // Enable ListView double buffering (hidden property)
                const int LVM_FIRST = 0x1000;
                const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
                const int LVS_EX_DOUBLEBUFFER = 0x00010000;

                SendMessage(this.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_DOUBLEBUFFER, LVS_EX_DOUBLEBUFFER);
            }

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        }

        public class GameListViewItem : ListViewItem
        {
            private GameHandlerInfo info = new GameHandlerInfo();
            public GameHandlerInfo Info
            {
                get => info;
                set
                {
                    info = value;
                }
            }

            public ItemFileType FileType { get; set; }
            public GameListViewItem() { }
        }

        public class CompressedHandlerData
        {
            public string JSContent;
            public string PackageContent;
            public GenericGameInfo GenericGameInfo;
        }

        public class GameHandlerInfo
        {
            public string Id;
            public int Version = -1;
            public string FileFullPath;
            public string ZipFileFullPath;
            public string JsFileName;
            public string ZipFileName;
            public string JsContent;
            public string GameName;
            public string GameGuid;
            public string ExePath;
            public string[] ExecutableContext;
            public string[] ExeNames;
            public bool IsInstalled;
            public bool UpdateAvailable;
            public bool IsDuplicate;
            public UserGameInfo UserGameInfo;
            public HandlerInfo HandlerInfo;
            public string PackageContent;
            public bool CanBeInstalled = true;
            public string BinariesFolder;
            //public string JsFileDate;
            //public string FileChecksum;
            public string JSErrorLineIndex;
            public string JSErrorLineText;
            public DateTime LastWriteTimeUtc;
        }


        private class JSContentView : ElementHost
        {
            public TextEditor TextEditor;
            public SearchPanel SearchPanel;

            public JSContentView()
            {

                TextEditor textEditor = new TextEditor();
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
                textEditor.FontSize = (double)((double)14 / (double)UI_Interface.Scale);

                //var ff = System.Windows.Media.Fonts.SystemFontFamilies;
                //;
                //textEditor.FontFamily = ff.ElementAt(7);
                textEditor.ShowLineNumbers = true;
                //Don't forget to cache the System.Windows.Media.SolidColorBrush
                textEditor.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                textEditor.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                //textEditor.Text = "test";
                textEditor.Width = Width;
                textEditor.Height = Height;
                //textEditor.
                textEditor.Visibility = System.Windows.Visibility.Visible;
                // HighlightingDefinitionTypeConverter conv = new HighlightingDefinitionTypeConverter();



                //HighlightingRuleSet rules = new HighlightingRuleSet();
                //HighlightingRule rule = new HighlightingRule();

                //rule.Regex = new Regex("ProtoInput");
                //var _color = new HighlightingColor();
                //_color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.LightYellow);
                //rule.Color = _color;

                //rules.Rules.Add(rule);
                //textEditor.SyntaxHighlighting = new CustomHighlightingDefinition("CSharpModified", textEditor.SyntaxHighlighting.MainRuleSet, textEditor.SyntaxHighlighting.NamedHighlightingColors);
                //Dictionary<string, SimpleHighlightingBrush> propertyValues = new Dictionary<string, SimpleHighlightingBrush> { { "ProtoInput", new SimpleHighlightingBrush(System.Windows.Media.Colors.LightYellow) } };

                //conv.CreateInstance(propertyValues);


                var highlighting = textEditor.SyntaxHighlighting;

                //highlighting.GetNamedColor("ProtoInput").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.GreenYellow);
                highlighting.GetNamedColor("StringInterpolation").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.LightYellow);
                highlighting.GetNamedColor("Punctuation").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.Violet);
                highlighting.GetNamedColor("NumberLiteral").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.Wheat);
                highlighting.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.LightGreen);
                highlighting.GetNamedColor("MethodCall").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.DarkGoldenrod);
                highlighting.GetNamedColor("GetSetAddRemove").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("Visibility").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ParameterModifiers").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("Modifiers").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("String").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.LightYellow);
                highlighting.GetNamedColor("Char").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.LightPink);
                highlighting.GetNamedColor("Preprocessor").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("TrueFalse").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("Keywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ValueTypeKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("SemanticKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("NamespaceKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ReferenceTypeKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ThisOrBaseReference").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("NullOrValueKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("GotoKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ContextKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("ExceptionKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("CheckedKeyword").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("UnsafeKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("OperatorKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);
                highlighting.GetNamedColor("SemanticKeywords").Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.SkyBlue);

                foreach (var color in highlighting.NamedHighlightingColors)
                {
                    color.FontWeight = null;
                }

                textEditor.SyntaxHighlighting = null;
                textEditor.SyntaxHighlighting = highlighting;

                SearchPanel = SearchPanel.Install(textEditor.TextArea);


                Child = textEditor;
                TextEditor = textEditor;

                //Context menu
                textEditor.PreviewMouseRightButtonDown += (s, e) =>
                {
                    var pos = textEditor.GetPositionFromPoint(e.GetPosition(textEditor));
                    if (pos.HasValue)
                        textEditor.TextArea.Caret.Position = pos.Value;
                };

                var menu = new System.Windows.Controls.ContextMenu();
                menu.Background = System.Windows.SystemColors.ControlBrush;
                menu.Foreground = System.Windows.SystemColors.ControlTextBrush;

                var copy = new System.Windows.Controls.MenuItem { 
                   Header = "Copy",
                };
                
                copy.Click += (s, e) => textEditor.Copy();

                var paste = new System.Windows.Controls.MenuItem { Header = "Paste" };
                paste.Click += (s, e) => textEditor.Paste();
                
                var cut = new System.Windows.Controls.MenuItem{ Header = "Cut" };
                cut.Click += (s, e) => textEditor.Cut();

                menu.Opened += (s, e) =>
                {
                    copy.IsEnabled = textEditor.SelectionLength > 0;
                    cut.IsEnabled = textEditor.SelectionLength > 0;
                    paste.IsEnabled = Clipboard.ContainsText();
                };
                var selectAll = new System.Windows.Controls.MenuItem { Header = "Select All" };
                selectAll.Click += (s, e) => textEditor.SelectAll();

                menu.Items.Add(new System.Windows.Controls.Separator());

                menu.Items.Add(selectAll);
                menu.Items.Add(copy);
                menu.Items.Add(paste);
                menu.Items.Add(cut);

                textEditor.ContextMenu = menu;
                //
            }
        }


        private enum ItemActions
        {
            Default,
            Delete_File,
            Update_Handler,
            Install_Handler,
            Uninstall_Handler,
            Refresh_All
        }

        public enum ItemFileType
        {
            Invalid,
            JsFile,
            Compressed
        }

        public enum ItemSortingFlag
        {
            Any,
            Installed,
            NotInstalled,
            Archive
        }

        private ItemSortingFlag currentSortingFlag;

        private int _installedHandlersCount;
        private int installedHandlersCount
        {
            get => _installedHandlersCount;
            set
            {
                _installedHandlersCount = value;
                installedLabelCount.Text = "Installed handlers: " + value.ToString();
                allFoundCount.Text = "Found:" + (value + notInstalledHandlersCount);
            }
        }

        private int _notInstalledHandlersCount;
        private int notInstalledHandlersCount
        {
            get => _notInstalledHandlersCount;
            set
            {
                _notInstalledHandlersCount = value;
                notInstalledLabelCount.Text = "Not installed handlers: " + value.ToString();
                allFoundCount.Text = "Found:" + (installedHandlersCount + value);
            }
        }

        private int _handlersArchivesCount;
        private int handlersArchivesCount
        {
            get => _handlersArchivesCount;
            set
            {
                _handlersArchivesCount = value;
                archivesLabelCount.Text = "Archives found: " + value.ToString();
            }
        }

        private string prevSearchText = "";
        private string handlersDirectory = Globals.NucleusInstallRoot + "\\handlers";
        private string tempReadZipDirectory = Globals.NucleusInstallRoot + "\\handlers\\temp";

        private const string searchFieldHint = "Search in all handlers";

        private bool firstClick = false;//searchfield

        private List<GameListViewItem> gameslistViewJsItemsList = new List<GameListViewItem>();
        private List<GameListViewItem> gameslistViewZipItemsList = new List<GameListViewItem>();

        private Dictionary<string, DateTime> blackListedZipFile = new Dictionary<string, DateTime>();//full file path,dateTime
        private Dictionary<string, DateTime> blackListedJSFile = new Dictionary<string, DateTime>();//full file path,dateTime

        public Form Form => form;
        private Form form;
        private GameListViewItem selectedItem;
        private Label allFoundCount;
        private Label installedLabelCount;
        private Label notInstalledLabelCount;
        private Label archivesLabelCount;
        private Label dateToolTipLabel;
        private SmoothListView gameslistView;
        private TextBox searchField;
        private JSContentView jsContentView;
        private Form packagejsContentWindow;
        private Label packagejsContentView;
        private PictureBox throbber;
        private Panel handlerInfoPanel;
        private Panel sortOptionsPanel;
        private Label updateButton;
        private Label installButton;
        private Label deleteJsFileButton;
        private Label openJsFileButton;
        private Label openHandlersFolderButton;
        private Label openJsPackageButton;
        private Label refreshButton;
        private Label sortOptionsButton;
        private Label saveChangeButton;

        private AutoCompleteStringCollection autoCompleteSource = new AutoCompleteStringCollection();

        private Regex jsFileNameRegex = new Regex("[\\/:*?\"<>|]");

        private Pen borderPen;
        private SolidBrush darkBrush;
        private System.Windows.Media.SolidColorBrush textSelectionBrush;
        private System.Windows.Media.SolidColorBrush searchPanelMarkerBrush;
        private Color darkColor = Color.FromArgb(255, 21, 24, 25);

        private System.Windows.Forms.Timer filesSuprVisorTimer;
        private bool extracting = DownloadPrompt.Instance != null;
        private bool refreshingAll = false;
        private Point CursorPos;

        public void Initialize()
        {
            Instance = this;
            if (!Directory.Exists(handlersDirectory))
            {
                Directory.CreateDirectory(handlersDirectory);
            }

            if (Directory.Exists(tempReadZipDirectory))
            {
                Directory.Delete(tempReadZipDirectory, true);
            }

            darkBrush = new SolidBrush(darkColor);
            borderPen = new Pen(Color.DarkGray);
            currentSortingFlag = ItemSortingFlag.Any;
            textSelectionBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 61, 73));
            searchPanelMarkerBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(67, 115, 249));
            CreateToolForm();
            Http_Client.OnHubConnectionChange += OnHubStateChange;
        }

        private void LocationChanged(object sender, EventArgs e)
        {
            //UI_Interface.MainForm.Location = new Point(form.Location.X,form.Location.Y);
        }

        private void CreateToolForm()
        {
            LoadAutoCompleteSource();
           
            //Cache all the autocompletion, save it to file, update if there new entries to add from readme or user handlers(not sure if it's 100% necessary)
            //Make something to create handlers from scratch (with the possiblility to load per engine preset)
            //Add a way to look at the raw readme.txt (in a new window, text box or open user default text editor)
            form = new Form
            {
                Text = "Handlers Manager",
                Size = new Size(1300, 850),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable,
                MaximizeBox = true,
                ShowIcon = false,
                BackColor = Color.FromArgb(31, 34, 35),
                Cursor = Theme_Settings.Default_Cursor,
            };

            form.AutoScaleMode = AutoScaleMode.None;
            form.SizeChanged += Form_SizeChanged;
            form.MouseEnter += Form_MouseEnter;
            form.MouseMove += HideSortOptionsPanel;
            form.MouseMove += HideDateTooltip;
            form.FormClosing += OnFormClosing;
            form.FormClosed += OnFormClosed;
            form.Paint += Form_Paint;
            form.LocationChanged += LocationChanged;
            form.AllowDrop = true;
            form.Location = UI_Interface.MainForm.Location;
            form.DragEnter += UI_Functions.ExtractHandlerDragEnter;
            form.DragOver += UI_Functions.ShowDragAndDropModal;
            form.DragLeave += UI_Functions.DisposeDragAndDropModal;

            form.DragDrop += UI_Functions.ExtractHandlerDragDrop;

            {
                saveChangeButton = new Label();
                saveChangeButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                saveChangeButton.Size = new Size(35, 20);
                saveChangeButton.Text = "Save changes";
                saveChangeButton.Location = new Point(form.Width - (saveChangeButton.Width + 5), 5);
                saveChangeButton.ForeColor = Color.RoyalBlue;
                saveChangeButton.AutoSize = true;
                saveChangeButton.Visible = false;
                saveChangeButton.Font = new Font("Arial", 10f/scale, FontStyle.Bold);
                saveChangeButton.BorderStyle = BorderStyle.FixedSingle;
                saveChangeButton.TextAlign = ContentAlignment.MiddleCenter;
                saveChangeButton.MouseDown += SavechangeButton_MouseDown;
                saveChangeButton.BackColor = darkColor;
                saveChangeButton.MouseEnter += Buttons_MouseEnter;
                saveChangeButton.MouseLeave += Buttons_MouseLeave;
                saveChangeButton.Cursor = Theme_Settings.Hand_Cursor;
                form.Controls.Add(saveChangeButton);
            }


            searchField = new TextBox();

            searchField.Location = new Point(20, 10);
            searchField.BackColor = Color.FromArgb(255, 31, 34, 35);
            searchField.ForeColor = Color.Gray;
            searchField.Text = searchFieldHint;
            searchField.Font = new Font("Aharoni", 10f, FontStyle.Italic);
            searchField.BorderStyle = BorderStyle.FixedSingle;
            searchField.Width = 300;

            searchField.AutoCompleteMode = AutoCompleteMode.Suggest;
            searchField.AutoCompleteSource = AutoCompleteSource.CustomSource;
            searchField.AutoCompleteCustomSource = autoCompleteSource;

            searchField.GotFocus += SearchFieldGotFocus;
            searchField.MouseDown += SearchFieldMouseDown;
            searchField.TextChanged += SearchFieldTextChanged;

            allFoundCount = new Label();
            allFoundCount.Size = new Size(10, 10);
            allFoundCount.AutoSize = true;
            allFoundCount.Location = new Point(20, searchField.Bottom + 10);
            allFoundCount.Font = new Font("Arial", 9 / scale, FontStyle.Regular);
            allFoundCount.ForeColor = Color.White;
            allFoundCount.Text = "Found: ..";

            installedLabelCount = new Label();
            installedLabelCount.Size = new Size(10, 10);
            installedLabelCount.AutoSize = true;
            installedLabelCount.Location = new Point(20, allFoundCount.Bottom + 8);
            installedLabelCount.Font = new Font("Arial", 9 / scale, FontStyle.Regular);
            installedLabelCount.ForeColor = Color.LightSeaGreen;
            installedLabelCount.Text = "Installed handlers: ";

            notInstalledLabelCount = new Label();
            notInstalledLabelCount.Size = new Size(10, 10);
            notInstalledLabelCount.AutoSize = true;
            notInstalledLabelCount.Location = new Point(20, installedLabelCount.Bottom + 5);
            notInstalledLabelCount.Font = new Font("Arial", 9 / scale, FontStyle.Regular);
            notInstalledLabelCount.ForeColor = Color.Gray;
            notInstalledLabelCount.Text = "Not installed handlers: ";

            archivesLabelCount = new Label();
            archivesLabelCount.Size = new Size(10, 10);
            archivesLabelCount.AutoSize = true;
            archivesLabelCount.Location = new Point(20, notInstalledLabelCount.Bottom + 5);
            archivesLabelCount.Font = new Font("Arial", 9 / scale, FontStyle.Regular);
            archivesLabelCount.ForeColor = Color.Orange;
            archivesLabelCount.Text = "Archives found: ";

            gameslistView = new SmoothListView();
            gameslistView.Location = new Point(20, archivesLabelCount.Bottom + 15);
            gameslistView.BackColor = darkColor;
            gameslistView.ForeColor = Color.White;
            gameslistView.BorderStyle = BorderStyle.None;
            gameslistView.Size = new Size(300, 685);
            gameslistView.AutoScrollOffset = new Point(280, 510);
            gameslistView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            gameslistView.Scrollable = true;
            gameslistView.Activation = ItemActivation.OneClick;
            gameslistView.Font = new Font("Arial", 10f, FontStyle.Bold);
            gameslistView.Alignment = ListViewAlignment.Left;
            gameslistView.MultiSelect = false;
            gameslistView.View = View.List;
            gameslistView.Click += ItemActivate;
            //gameslistView.SmallImageList = installCheckImages;
            gameslistView.MouseMove += ListView_MouseMove;
            //gameslistView.MouseLeave += ListView_MouseLeave;
            gameslistView.Cursor = Theme_Settings.Hand_Cursor;
            gameslistView.OwnerDraw = true;
            gameslistView.DrawItem += GameslistView_Paint;
            gameslistView.Visible = false;


            dateToolTipLabel = new Label();
            dateToolTipLabel.AutoSize = true;
            dateToolTipLabel.TextAlign = ContentAlignment.MiddleCenter;
            dateToolTipLabel.ForeColor = Color.GreenYellow;
            dateToolTipLabel.BackColor = darkColor;
            dateToolTipLabel.Font = new Font("Arial", 9f / scale, FontStyle.Bold);
            dateToolTipLabel.Visible = false;
            gameslistView.Controls.Add(dateToolTipLabel);

            updateButton = new Label();
            updateButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            updateButton.Text = "Update Handler";
            updateButton.Location = new Point(gameslistView.Right + 10, searchField.Top);
            updateButton.ForeColor = Color.White;
            updateButton.AutoSize = true;
            updateButton.Visible = false;
            updateButton.Font = new Font("Arial", 10f / scale, FontStyle.Bold);
            updateButton.BorderStyle = BorderStyle.FixedSingle;
            updateButton.TextAlign = ContentAlignment.MiddleCenter;
            updateButton.MouseDown += UpdateButton_MouseDown;
            updateButton.BackColor = darkColor;
            updateButton.MouseEnter += Buttons_MouseEnter;
            updateButton.MouseLeave += Buttons_MouseLeave;
            updateButton.Cursor = Theme_Settings.Hand_Cursor;

            installButton = new Label();
            installButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            installButton.Text = "Install Handler";
            installButton.Location = new Point(updateButton.Right + 5, searchField.Top);
            installButton.ForeColor = Color.White;
            installButton.AutoSize = true;
            installButton.Visible = false;
            installButton.Font = new Font("Arial", 10f / scale, FontStyle.Bold);
            installButton.BorderStyle = BorderStyle.FixedSingle;
            installButton.TextAlign = ContentAlignment.MiddleCenter;
            installButton.MouseDown += InstallButton_MouseDown;
            installButton.Cursor = Theme_Settings.Hand_Cursor;
            installButton.BackColor = darkColor;
            installButton.MouseEnter += Buttons_MouseEnter;
            installButton.MouseLeave += Buttons_MouseLeave;

            deleteJsFileButton = new Label();
            deleteJsFileButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            deleteJsFileButton.Text = "Delete handler files ⚠";
            deleteJsFileButton.Location = new Point(installButton.Right + 5, searchField.Top);
            deleteJsFileButton.ForeColor = Color.White;
            deleteJsFileButton.AutoSize = true;
            deleteJsFileButton.Visible = false;
            deleteJsFileButton.Font = new Font("Arial", 10f / scale, FontStyle.Bold);
            deleteJsFileButton.BorderStyle = BorderStyle.FixedSingle;
            deleteJsFileButton.TextAlign = ContentAlignment.MiddleCenter;
            deleteJsFileButton.MouseDown += DeleteJsFileButton_MouseDown;
            deleteJsFileButton.Cursor = Theme_Settings.Hand_Cursor;
            deleteJsFileButton.BackColor = darkColor;
            deleteJsFileButton.MouseEnter += Buttons_MouseEnter;
            deleteJsFileButton.MouseLeave += Buttons_MouseLeave;

            openJsFileButton = new Label();
            openJsFileButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            openJsFileButton.Text = "Open handler file ✑";
            openJsFileButton.Location = new Point(deleteJsFileButton.Right + 5, searchField.Top);
            openJsFileButton.ForeColor = Color.White;
            openJsFileButton.AutoSize = true;
            openJsFileButton.Font = new Font("Arial", 10f / scale, FontStyle.Bold);
            openJsFileButton.BorderStyle = BorderStyle.FixedSingle;
            openJsFileButton.TextAlign = ContentAlignment.MiddleCenter;
            openJsFileButton.MouseDown += OpenJsFileButton_MouseDown;
            openJsFileButton.Visible = false;
            openJsFileButton.Cursor = Theme_Settings.Hand_Cursor;
            openJsFileButton.BackColor = darkColor;
            openJsFileButton.MouseEnter += Buttons_MouseEnter;
            openJsFileButton.MouseLeave += Buttons_MouseLeave;

            openJsPackageButton = new Label();
            openJsPackageButton.Anchor = AnchorStyles.Left | AnchorStyles.Top /*| AnchorStyles.Bottom*//* | AnchorStyles.Right*/;
            openJsPackageButton.Text = "Open package folder ☒";
            openJsPackageButton.Location = new Point(openJsFileButton.Right + 5, searchField.Top);
            openJsPackageButton.ForeColor = Color.White;
            openJsPackageButton.AutoSize = true;
            openJsPackageButton.Font = new Font("Arial", 10f / scale, FontStyle.Bold);
            openJsPackageButton.BorderStyle = BorderStyle.FixedSingle;
            openJsPackageButton.TextAlign = ContentAlignment.MiddleCenter;
            openJsPackageButton.MouseDown += OpenJsPackageButton_MouseDown;
            openJsPackageButton.Visible = false;
            openJsPackageButton.Cursor = Theme_Settings.Hand_Cursor;
            openJsPackageButton.BackColor = darkColor;
            openJsPackageButton.MouseEnter += Buttons_MouseEnter;
            openJsPackageButton.MouseLeave += Buttons_MouseLeave;

            handlerInfoPanel = new Panel();
            handlerInfoPanel.Location = new Point(gameslistView.Right + 15, updateButton.Location.Y + updateButton.Height + 10);
            handlerInfoPanel.BackColor = darkColor;
            handlerInfoPanel.BorderStyle = BorderStyle.None;
            handlerInfoPanel.ForeColor = Color.White;
            handlerInfoPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            handlerInfoPanel.AutoSize = true;
            handlerInfoPanel.Visible = false;

            packagejsContentView = new Label();
            packagejsContentView.Font = new Font("Cascadia Code", 10 / scale, FontStyle.Regular);
            packagejsContentView.BorderStyle = BorderStyle.None;
            packagejsContentView.Anchor = AnchorStyles.Left | AnchorStyles.Top ;
            packagejsContentView.BackColor = darkColor;
            packagejsContentView.ForeColor = Color.White;
            packagejsContentView.AutoSize = true;
            packagejsContentView.TextAlign = ContentAlignment.MiddleLeft;
            
            packagejsContentWindow = new Form();
            packagejsContentWindow.Size = new Size(800,200);
            packagejsContentWindow.TransparencyKey = Color.Yellow;
            packagejsContentWindow.BackColor = Color.Yellow;
            packagejsContentWindow.StartPosition = FormStartPosition.Manual;
            packagejsContentWindow.AutoSize = true;
            packagejsContentWindow.FormBorderStyle = FormBorderStyle.None;
            packagejsContentWindow.Controls.Add(packagejsContentView);
           
            jsContentView = new JSContentView();
            jsContentView.Font = new Font("Cascadia Code", 10 / scale, FontStyle.Regular);
            jsContentView.Location = new Point(handlerInfoPanel.Right + 10, 10);
            jsContentView.Size = new Size((form.Width - (handlerInfoPanel.Left)) - 30, 550);
            jsContentView.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            jsContentView.BackColor = darkColor;
            jsContentView.ForeColor = Color.White;
            jsContentView.Visible = false;
            jsContentView.TextEditor.TextArea.TextEntered += TextArea_TextEntered;
            jsContentView.TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            jsContentView.TextEditor.Document.Changed += Document_TextChanged;
            jsContentView.TextEditor.PreviewKeyDown += CtrlS_Shortcut;

            refreshButton = new Label();
            refreshButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            refreshButton.Text = "Refresh";
            refreshButton.Location = new Point(openJsFileButton.Right + 5, searchField.Top);
            refreshButton.ForeColor = Color.White;
            refreshButton.AutoSize = true;
            refreshButton.Font = new Font("Arial", 9f / scale, FontStyle.Bold);
            refreshButton.BorderStyle = BorderStyle.FixedSingle;
            refreshButton.TextAlign = ContentAlignment.MiddleCenter;
            refreshButton.Visible = true;
            refreshButton.Cursor = Theme_Settings.Hand_Cursor;
            refreshButton.BackColor = darkColor;
            refreshButton.MouseEnter += Buttons_MouseEnter;
            refreshButton.MouseLeave += Buttons_MouseLeave;
            refreshButton.MouseDown += RefreshButton_MouseDown;

            openHandlersFolderButton = new Label();
            openHandlersFolderButton.Anchor = AnchorStyles.Left | AnchorStyles.Top ;
            openHandlersFolderButton.Text = "Open handlers folder";
            openHandlersFolderButton.ForeColor = Color.White;
            openHandlersFolderButton.AutoSize = true;
            openHandlersFolderButton.Font = new Font("Arial", 9f / scale, FontStyle.Bold);
            openHandlersFolderButton.BorderStyle = BorderStyle.FixedSingle;
            openHandlersFolderButton.TextAlign = ContentAlignment.MiddleCenter;
            openHandlersFolderButton.MouseDown += OpenHandlersFolder_MouseDown;
            openHandlersFolderButton.Visible = false;
            openHandlersFolderButton.Cursor = Theme_Settings.Hand_Cursor;
            openHandlersFolderButton.BackColor = darkColor;
            openHandlersFolderButton.MouseEnter += Buttons_MouseEnter;
            openHandlersFolderButton.MouseLeave += Buttons_MouseLeave;

            sortOptionsButton = new Label();
            sortOptionsButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            sortOptionsButton.Text = "View";
            sortOptionsButton.ForeColor = Color.White;
            sortOptionsButton.AutoSize = true;
            sortOptionsButton.Font = new Font("Arial", 9f / scale, FontStyle.Bold);
            sortOptionsButton.BorderStyle = BorderStyle.FixedSingle;
            sortOptionsButton.TextAlign = ContentAlignment.MiddleCenter;
            sortOptionsButton.Visible = true;
            sortOptionsButton.Cursor = Theme_Settings.Hand_Cursor;
            sortOptionsButton.BackColor = darkColor;
            sortOptionsButton.MouseEnter += Buttons_MouseEnter;
            sortOptionsButton.MouseLeave += Buttons_MouseLeave;
            sortOptionsButton.MouseDown += SortOptionsButton_MouseDown;//Open sorting panel

            sortOptionsPanel = new Panel();
            sortOptionsPanel.BackColor = darkColor;
            sortOptionsPanel.Visible = false;
            sortOptionsPanel.AutoSize = true;

            RadioButton options1 = new RadioButton();
            options1.Text = "All";
            options1.ForeColor = Color.White;
            options1.BackColor = darkColor;
            options1.Location = new Point(0, 0);
            options1.Tag = ItemSortingFlag.Any;
            options1.Checked = true;
            options1.CheckedChanged += ShowItemsByType;
            options1.Font = new Font("Arial", 8.75f / scale, FontStyle.Bold);

            sortOptionsPanel.Controls.Add(options1);

            RadioButton options2 = new RadioButton();
            options2.Text = "Installed";
            options2.ForeColor = Color.White;
            options2.BackColor = darkColor;
            options2.Location = new Point(0, options1.Bottom);
            options2.Tag = ItemSortingFlag.Installed;
            options2.CheckedChanged += ShowItemsByType;
            options2.Font = new Font("Arial", 8.75f / scale, FontStyle.Bold);
            sortOptionsPanel.Controls.Add(options2);

            RadioButton options3 = new RadioButton();
            options3.Text = "Not installed";
            options3.ForeColor = Color.White;
            options3.BackColor = darkColor;
            options3.Location = new Point(0, options2.Bottom);
            options3.Tag = ItemSortingFlag.NotInstalled;
            options3.CheckedChanged += ShowItemsByType;
            options3.Font = new Font("Arial", 8.75f / scale, FontStyle.Bold);
            sortOptionsPanel.Controls.Add(options3);

            RadioButton options4 = new RadioButton();
            options4.Text = "Archives";
            options4.ForeColor = Color.White;
            options4.BackColor = darkColor;
            options4.Location = new Point(0, options3.Bottom);
            options4.Tag = ItemSortingFlag.Archive;
            options4.CheckedChanged += ShowItemsByType;
            options4.Font = new Font("Arial", 8.75f / scale, FontStyle.Bold);
            sortOptionsPanel.Controls.Add(options4);

            throbber = new PictureBox
            {
                Size = new Size(50, 50),
                Anchor = AnchorStyles.None,
                Location = new Point((form.Width / 2) - 25, (form.Height / 2) - 25),
                BackColor = form.BackColor,
                Image = ImageCache.GetImage(Globals.ThemeFolder + "loading.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };

            throbber.LocationChanged += ThrobberLocationChanged;
            throbber.VisibleChanged += ThrobberLocationChanged;

            form.Controls.Add(searchField);
            form.Controls.Add(allFoundCount);
            form.Controls.Add(installedLabelCount);
            form.Controls.Add(notInstalledLabelCount);
            form.Controls.Add(archivesLabelCount);
            form.Controls.Add(gameslistView);
            form.Controls.Add(updateButton);
            form.Controls.Add(installButton);
            form.Controls.Add(deleteJsFileButton);
            form.Controls.Add(openJsFileButton);
            form.Controls.Add(openJsPackageButton);
            form.Controls.Add(handlerInfoPanel);
            form.Controls.Add(jsContentView);
            form.Controls.Add(refreshButton);
            form.Controls.Add(sortOptionsButton);
            form.Controls.Add(sortOptionsPanel);
            form.Controls.Add(openHandlersFolderButton);
            form.Controls.Add(throbber);
            form.SizeChanged += OnFormSizeChanged;
            gameslistView.Focus();


            foreach (Control ctrl in form.Controls)
            {
                if (ctrl != sortOptionsPanel && !(ctrl is RadioButton) && ctrl != sortOptionsButton)
                {
                    ctrl.MouseMove += HideSortOptionsPanel;
                }

                ctrl.MouseMove += HideDateTooltip;
                ctrl.DragEnter += UI_Functions.ExtractHandlerDragEnter;
                ctrl.DragDrop += UI_Functions.ExtractHandlerDragDrop;
            }

            UpdateHandlersInfo();

            filesSuprVisorTimer = new System.Windows.Forms.Timer();
            filesSuprVisorTimer.Interval = (2000); //millisecond                   
            filesSuprVisorTimer.Tick += FileSupervisorTimer_Tick;
            filesSuprVisorTimer.Start();

            //form.ShowDialog();
             form.Show();
             //scale = (float)User32Util.GetDpiForWindow(form.Handle) / (float)100;
             UpdateFonts();
        }

        private void UpdateFonts()
        {
            
            //foreach (Control con in form.Controls)
            //{
            //    if (!(con is TextBox) /*&& !(con is ListView)*/)
            //    {
            //        con.Font = new Font(con.Font.FontFamily, con.Font.Size / scale, con.Font.Style);
            //    }

            //    if (con is Button)
            //    {
            //        con.Location = new Point((int)(con.Left * scale), (int)(con.Top * scale));
            //    }

            //    //if (con is ListView)
            //    //{
            //    //    con.Size = new Size(con.Width, (int)(con.Height / scale));
            //    //}
                
               
            //}

            //if(UI_Interface.scale > 1)
            //{
            //    RefreshButton_MouseDown(null, null);
            //}
        }

        private TextEditor textEditor;

        //Ajouter save et profile path dans la handler info panel, peut etre ajouter d'autre trucs??
        private void UpdateHandlersInfo()
        {
            ShowThrobber();

            refreshingAll = true;
            currentSortingFlag = ItemSortingFlag.Any;
            RadioButton anyflagRadioButton = sortOptionsPanel.Controls[0] as RadioButton;
            anyflagRadioButton.Checked = true;


            blackListedJSFile.Clear();
            blackListedZipFile.Clear();

            gameslistView.BeginUpdate();

            installedHandlersCount = 0;
            notInstalledHandlersCount = 0;
            handlersArchivesCount = 0;

            gameslistViewZipItemsList.Clear();
            gameslistViewJsItemsList.Clear();

            gameslistView.Items?.Clear();

            string jsfolder = Core_Interface.GameManager.GetJsScriptsPath();

            DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);

            //Compressed handlers
            List<FileInfo> zippedFiles = jsFolder.EnumerateFiles().Where(f =>
                                         f.Extension.Equals(".nc", StringComparison.OrdinalIgnoreCase) ||
                                         f.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (FileInfo zipInfo in zippedFiles)
            {
                CompressedHandlerData zipGenInfo = GetCompressedHandlerDatas(zipInfo.FullName);

                if (zipGenInfo != null && zipGenInfo.GenericGameInfo != null)
                {
                    HandlerInfo handlerInfo = new HandlerInfo(zipInfo, zipGenInfo.GenericGameInfo);

                    SetGameListViewZipItemData(handlerInfo, zipInfo.FullName, zipGenInfo.JSContent, zipGenInfo.PackageContent);
                }
            }

            foreach (HandlerInfo info in GameManager.Instance.HandlerInfos)
            {
                //else it's added later by the filesupervisor
                if (info.IsValid)
                {
                    SetGameListViewItemData(info);
                }
            }

            gameslistView.EndUpdate();

            ReSelectSelectedItem();

            refreshButton.Location = new Point(gameslistView.Right - refreshButton.Width, searchField.Bottom + 10);
            openHandlersFolderButton.Location = new Point(refreshButton.Right - openHandlersFolderButton.Width, refreshButton.Bottom + 5);
            openHandlersFolderButton.Visible = true;
            sortOptionsButton.Location = new Point(refreshButton.Right - sortOptionsButton.Width, openHandlersFolderButton.Bottom + 5);
            sortOptionsPanel.Location = new Point(sortOptionsButton.Right - sortOptionsPanel.Width, sortOptionsButton.Top);
            form.BringToFront();

            gameslistView.Sorting = SortOrder.Ascending;

            jsContentView.Size = new Size((form.Width - (handlerInfoPanel.Left)) - 30, jsContentView.Height);
            jsContentView.Location = new Point(handlerInfoPanel.Left, handlerInfoPanel.Bottom + 10);
            gameslistView.Visible = true;
            refreshingAll = false;
            HideThrobber();
        }

        private void ItemActivate(object sender, EventArgs e)
        {
            SmoothListView listView;
            GameListViewItem item;

            if (sender.GetType() == typeof(SmoothListView))//regular selection
            {
                listView = (SmoothListView)sender;

                if (listView.SelectedItems.Count == 0)
                {
                    return;
                }

                item = (GameListViewItem)listView.SelectedItems[0];
            }
            else
            {
                item = (GameListViewItem)sender;//update selection
            }

            saveChangeButton.Enabled = false;

            selectedItem = item;

            bool isJsHandler = item.FileType == ItemFileType.JsFile;

            if(!isJsHandler)
            {
                saveChangeButton.Visible = false;
            }
            
            jsContentView.Visible = false;
            jsContentView.TextEditor.IsReadOnly = item.FileType == ItemFileType.Compressed;
            jsContentView.SearchPanel.Close();
            jsContentView.SearchPanel.SearchPattern = "";

            jsContentView.BringToFront();
            jsContentView.Text = item.Info.JsContent;

            if (searchField.Text != "" && searchField.Text != searchFieldHint)
            {
                HighlightWordEditor(jsContentView.TextEditor, searchField.Text, true);
            }

            jsContentView.TextEditor.Text = item.Info.JsContent;
            deleteJsFileButton.Visible = true;
            handlerInfoPanel.Visible = item.FileType != ItemFileType.Invalid;
            openJsFileButton.Visible = true;
            jsContentView.TextEditor.TextArea.SelectionBrush = textSelectionBrush;
            jsContentView.SearchPanel.MatchCase = false;
            jsContentView.SearchPanel.Open();

            if (item.FileType is ItemFileType.Invalid)
            {
                if (item.Info.JSErrorLineText != null)
                {
                    HighlightWordEditor(jsContentView.TextEditor, item.Info.JSErrorLineText, false);
                    jsContentView.TextEditor.TextArea.SelectionBrush = textSelectionBrush;

                }

                openJsPackageButton.Visible = false;
                installButton.Visible = false;

                jsContentView.Visible = true;
                UpdateMainControlsLocation();
                return;
            }

            PopulateHandlerInfo(item.Info);

            if (item.Info.PackageContent != null)
            {
                packagejsContentView.Text = item.Info.PackageContent;
            }         
          
            if (isJsHandler)
            {                          
                updateButton.Visible = item.Info.IsInstalled && !string.IsNullOrEmpty(item.Info.Id) && item.Info.Version != -1 && HubConnected;

                if (updateButton.Visible)
                {
                    if (item.Info.UpdateAvailable && item.Info.IsInstalled)
                    {
                        updateButton.Text = "Update handler 🠋";
                        updateButton.ForeColor = Color.White;
                        updateButton.Cursor = Theme_Settings.Hand_Cursor;
                    }
                    else if (!item.Info.UpdateAvailable && item.Info.IsInstalled)
                    {
                        updateButton.Text = "Handler is up-to-date ✔";
                        updateButton.ForeColor = Color.White;
                        updateButton.Cursor = Theme_Settings.Default_Cursor;
                    }
                }

                deleteJsFileButton.Text = "Delete handler files ⚠";

                if (item.Info.IsDuplicate)
                {
                    installButton.Text = "Handler duplicate";
                    installButton.ForeColor = Color.IndianRed;
                    installButton.Visible = true;
                    item.Info.CanBeInstalled = false;
                }
                else if (item.Info.IsInstalled)
                {
                    installButton.Text = "Uninstall handler ✘";
                    installButton.ForeColor = Color.White;
                    installButton.Visible = true;
                }
                else if (!item.Info.IsInstalled)
                {
                    installButton.Text = "Install handler ✚";
                    installButton.ForeColor = Color.White;
                    installButton.Visible = true;
                }
            }
            else
            {
                updateButton.Visible = false;
                openJsFileButton.Visible = false;
                installButton.Text = "Extract && install ⮻";
                installButton.Visible = true;
                deleteJsFileButton.Text = "Delete handler archive ⚠";
            }
                   
            openJsPackageButton.Visible = isJsHandler && Directory.Exists(item.Info.FileFullPath.Replace(".js", ""));
            jsContentView.Visible = true;

            UpdateMainControlsLocation();
            gameslistView.EnsureVisible(item.Index);
        }

        private void PopulateHandlerInfo(GameHandlerInfo handlerInfo)
        {
            foreach (Control ctrl in handlerInfoPanel.Controls)
            {
                ctrl.Dispose();
            }

            handlerInfoPanel.Controls.Clear();

            int xOffset = 0;
            int yOffset = 3;

            Font bigFont = new Font("Microsoft Sans Serif", 10f/scale, FontStyle.Bold);//must be cached
            Font smallFont = new Font("Verdana", 9f / scale, FontStyle.Regular);//must be cached

            //Game name
            MultiColorLabel gameName = new MultiColorLabel(handlerInfoPanel, new Point(0, 0));
            gameName.SetText(new (string, Color, Font)[] { ($"Game name:", Color.LightGray, bigFont), (handlerInfo.GameName, Color.LightBlue, smallFont) });
            //Installed?
            MultiColorLabel installed = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
            installed.SetText(new (string, Color, Font)[] { ($"Installed:", Color.LightGray, bigFont), (handlerInfo.IsInstalled.ToString(), handlerInfo.IsInstalled ? Color.LightGreen : Color.IndianRed, smallFont) });
            //Handler version
            MultiColorLabel version = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
            version.SetText(new (string, Color, Font)[] { ($"Version:", Color.LightGray, bigFont), (handlerInfo.Version == -1 ? "Unknow" : handlerInfo.HandlerInfo.GenericGameInfo.Hub.Handler.Version.ToString(), handlerInfo.Version == -1 ? Color.IndianRed : Color.LightGreen, smallFont) });
            //Js file name
            MultiColorLabel fileName = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
            fileName.SetText(new (string, Color, Font)[] { ($"Handler file name:", Color.LightGray, bigFont), (handlerInfo.JsFileName, Color.LightBlue, smallFont) });
            //Js file date
            if (handlerInfo.LastWriteTimeUtc != null)
            {
                MultiColorLabel fileDate = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
                fileDate.SetText(new (string, Color, Font)[] { ($"Handler file creation:", Color.LightGray, bigFont), (handlerInfo.LastWriteTimeUtc.ToString(), Color.LightBlue, smallFont) });
            }
            //Exe path
            MultiColorLabel exePath = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
            exePath.SetText(new (string, Color, Font)[] { (handlerInfo.IsInstalled ? "Executable path:" : $"Executables path: ", Color.Yellow, bigFont), (handlerInfo.ExePath, Color.LightBlue, smallFont) });

            //Selectable exes
            MultiColorLabel validExes = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));

            if (handlerInfo.HandlerInfo.GenericGameInfo.ExecutableNames != null)
            {
                string[] exeNamesValues = handlerInfo.HandlerInfo.GenericGameInfo.ExecutableNames;

                (string, Color, Font)[] fields = new (string, Color, Font)[exeNamesValues.Length + 1];

                for (int i = 0; i < fields.Length; i++)
                {

                    if (i == 0)
                    {
                        fields[i] = ($"Valid executables:", Color.Yellow, bigFont);
                    }
                    else
                    {
                        if (i - 1 < exeNamesValues.Length - 1)
                        {
                            fields[i] = (exeNamesValues[i - 1] + ",", Color.LightBlue, smallFont);
                        }
                        else
                        {
                            fields[i] = (exeNamesValues[i - 1], Color.LightBlue, smallFont);
                        }
                    }
                }

                validExes.SetText(fields);
            }
            else
            {
                validExes.SetText(new (string, Color, Font)[] { ($"Valid executable:", Color.Yellow, bigFont), (handlerInfo.HandlerInfo.GenericGameInfo.ExecutableName, Color.LightBlue, smallFont) });
            }

            //Exe context requierements
            if (handlerInfo.HandlerInfo.GenericGameInfo.ExecutableContext != null)
            {
                MultiColorLabel exeContext = new MultiColorLabel(handlerInfoPanel, new Point(xOffset, handlerInfoPanel.Controls[handlerInfoPanel.Controls.Count - 1].Bottom + yOffset));
                string[] exeContextValues = handlerInfo.HandlerInfo.GenericGameInfo.ExecutableContext;

                (string, Color, Font)[] fields = new (string, Color, Font)[exeContextValues.Length + 1];

                for (int i = 0; i < fields.Length; i++)
                {
                    if (i == 0)
                    {
                        fields[i] = ($"Required at Game Root:", Color.Yellow, bigFont);
                    }
                    else
                    {
                        string fileOrDir = "(Folder) ";

                        if (exeContextValues[i - 1].Contains("."))
                        {
                            fileOrDir = "(File) ";
                        }

                        if (i - 1 < exeContextValues.Length - 1)
                        {
                            fields[i] = (fileOrDir + exeContextValues[i - 1] + ",", Color.LightBlue, smallFont);
                        }
                        else
                        {
                            fields[i] = (fileOrDir + exeContextValues[i - 1], Color.LightBlue, smallFont);
                        }
                    }
                }

                exeContext.SetText(fields);
            }
        }

        private GameListViewItem SetGameListViewItemData(HandlerInfo handlerInfo)
        {
            GameHandlerInfo ghi = new GameHandlerInfo();

            UserGameInfo addedGame = Core_Interface.GameManager.User.Games.Find(g => g?.HandlerInfo?.FileFullPath == handlerInfo.FileFullPath);

            GameListViewItem item = new GameListViewItem();

            item.FileType = ItemFileType.JsFile;
            item.Name = handlerInfo.FileName;            
            item.Text = "❖ " + handlerInfo.GameName;
            item.BackColor = darkColor;
            item.ToolTipText = handlerInfo.LastWriteTimeUtc.ToString();
            
            ghi.HandlerInfo = handlerInfo;
            ghi.GameName = handlerInfo.GameName;
            ghi.GameGuid = handlerInfo.GUID;
            ghi.FileFullPath = handlersDirectory + "\\" + handlerInfo.FileName;
            ghi.LastWriteTimeUtc = handlerInfo.LastWriteTimeUtc;
            ghi.JsFileName = handlerInfo.FileName;
            ghi.BinariesFolder = handlerInfo.GenericGameInfo.BinariesFolder != null ? handlerInfo.GenericGameInfo.BinariesFolder : "";
            ghi.JsContent = File.ReadAllText(handlerInfo.FileFullPath);

            string packagePath = Path.Combine(handlersDirectory, jsFileNameRegex.Replace(handlerInfo.GameName, ""));

            if (Directory.Exists(packagePath))
            {
                ghi.PackageContent = ListPackageContent(packagePath);
            }

            if (addedGame != null)
            {
                ghi.UserGameInfo = addedGame;
                ghi.UserGameInfo.NotifyUpdateAvailable += RefreshHandlerUpdateState;
                ghi.ExePath = addedGame.ExePath;
                bool canCheckForUpdate = addedGame.HandlerInfo.GenericGameInfo.Hub.Handler.Version != -1 && !string.IsNullOrEmpty(addedGame.HandlerInfo.GenericGameInfo.Hub.Handler.Id);

                if (canCheckForUpdate)
                {
                    ghi.Version = addedGame.HandlerInfo.GenericGameInfo.Hub.Handler.Version;
                    ghi.Id = addedGame.HandlerInfo.GenericGameInfo?.Hub.Handler?.Id;
                }

                if (addedGame.UpdateAvailable)
                {
                    ghi.UpdateAvailable = addedGame.UpdateAvailable;
                }

                ghi.IsInstalled = true;
                installedHandlersCount++;
            }
            else
            {
                bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == handlerInfo.GUID && g.GameName == handlerInfo.GameName).Count() == 1;
                bool alreadyInstalled = GameManager.Instance.User.Games.Any(glvi => glvi.GameGuid == handlerInfo.GUID && glvi.FileFullPath != handlerInfo.FileFullPath);

                if (handlerInfo.GameName == "Half-Life")
                {
                    ;
                }

                if (onlyOneJsForGame)//Only one handler so the file name doesn't matter
                {
                    ghi.IsDuplicate = false;
                    ghi.CanBeInstalled = true;
                }
                else if (alreadyInstalled && gameslistViewJsItemsList.Any(it => it.Text == item.Text))
                {
                    ghi.IsDuplicate = true;
                    ghi.CanBeInstalled = false;
                    var index = GameManager.Instance.HandlerInfos.IndexOf(handlerInfo);
                    item.Text = item.Text + "*";
                }
                else if (alreadyInstalled)
                {
                    ghi.IsDuplicate = true;
                    ghi.CanBeInstalled = false;
                    var index = GameManager.Instance.HandlerInfos.IndexOf(handlerInfo);
                    item.Text = item.Text + "*";
                }
                else
                {
                    ghi.IsDuplicate = false;
                    ghi.CanBeInstalled = true;
                }

                ghi.ExePath = ghi.BinariesFolder != null && ghi.BinariesFolder != "" ? @"Game Folder Root\" + ghi.BinariesFolder : "Game Folder Root";

                if (handlerInfo.GenericGameInfo.Hub.Handler.Version != -1 && !string.IsNullOrEmpty(handlerInfo.GenericGameInfo.Hub.Handler.Id))
                {
                    ghi.Version = handlerInfo.GenericGameInfo.Hub.Handler.Version;
                    ghi.Id = handlerInfo.GenericGameInfo.Hub.Handler.Id;
                }

                ghi.IsInstalled = false;
                notInstalledHandlersCount++;
            }

            //int indexOfLastItem = gameslistView.Items.Count - 1;

            //if(indexOfLastItem < 0 )
            //{
            //    indexOfLastItem = 0;
            //}

            //item.Font = new Font(gameslistView.Font.FontFamily, 10f / scale);
            //item.Position = new Point(item.Position.X, gameslistView.Items[indexOfLastItem].Bounds.Bottom);
            item.Info = ghi;
            gameslistView.Items.Add(item);
            gameslistViewJsItemsList.Add(item);

            ParseJsContentForAutoCompletion(ghi.JsContent);
            gameslistView.Sorting = SortOrder.Ascending;

         

            return item;
        }

        private GameListViewItem SetGameListViewZipItemData(HandlerInfo handlerInfo, string zipFilePath, string zipJsContent, string zipPackageContent)
        {
            GameHandlerInfo ghi = new GameHandlerInfo();

            GameListViewItem item = new GameListViewItem();

            DateTime lastWriteTime = new FileInfo(zipFilePath).LastWriteTimeUtc;

            item.Name = zipFilePath.TrimEnd();
            item.FileType = ItemFileType.Compressed;
            item.ToolTipText = lastWriteTime.ToString();
            item.Text = "▶ " + handlerInfo.GameName + " (archive)";
            item.BackColor = darkColor;

            ghi.HandlerInfo = handlerInfo;
            ghi.GameName = handlerInfo.GameName;
            ghi.GameGuid = handlerInfo.GUID;
            ghi.LastWriteTimeUtc = lastWriteTime;
            ghi.JsFileName = zipFilePath.Split('\\').Last();
            ghi.ZipFileFullPath = zipFilePath;       
            ghi.PackageContent = Path.Combine(handlersDirectory, jsFileNameRegex.Replace(handlerInfo.GameName, ""));
            ghi.BinariesFolder = handlerInfo.GenericGameInfo.BinariesFolder != null ? handlerInfo.GenericGameInfo.BinariesFolder : "";
            ghi.ExePath = ghi.BinariesFolder != null && ghi.BinariesFolder != "" ? @"Game Folder Root\" + ghi.BinariesFolder : "Game Folder Root";  
            ghi.JsContent = zipJsContent;
            ghi.IsInstalled = false;

            if (handlerInfo.GenericGameInfo.Hub.Handler.Version != -1 && !string.IsNullOrEmpty(handlerInfo.GenericGameInfo.Hub.Handler.Id))
            {
                ghi.Version = handlerInfo.GenericGameInfo.Hub.Handler.Version;
                ghi.Id = handlerInfo.GenericGameInfo.Hub.Handler.Id;
            }

            item.Info = ghi;

            gameslistViewZipItemsList.Add(item);
            gameslistView.Items.Add(item);

            handlersArchivesCount++;

            ParseJsContentForAutoCompletion(ghi.JsContent);
            gameslistView.Sorting = SortOrder.Ascending;

            return item;
        }

        private void OnHubStateChange(HubResponse response)
        {
            //if(response is HubResponse.Success)
            //{
                RefreshHandlerUpdateState(response is HubResponse.Success);
            //}
            //else
            //{               
            //    RefreshHandlerUpdateState(false);
            //}
        }

        private void RefreshHandlerUpdateState(bool updateAvailable)
        {
            if (selectedItem != null)
            {
                UIThread.Post(() =>
                {
                    updateButton.Visible = selectedItem.Info.IsInstalled && !string.IsNullOrEmpty(selectedItem.Info.Id) && selectedItem.Info.Version != -1 && HubConnected;

                    if (updateButton.Visible)
                    {
                        if (updateAvailable)
                        {
                            selectedItem.Info.UpdateAvailable = updateAvailable;
                            updateButton.Text = "Update handler 🠋";
                            updateButton.ForeColor = Color.White;
                            updateButton.Cursor = Theme_Settings.Hand_Cursor;
                        }
                        else
                        {
                            selectedItem.Info.UpdateAvailable = updateAvailable;
                            updateButton.Text = "Handler is up-to-date ✔";
                            updateButton.ForeColor = Color.White;
                            updateButton.Cursor = Theme_Settings.Default_Cursor;
                        }
                    }

                    UpdateMainControlsLocation();
                });
            }
        }

        private void SetInvalidGameListViewItem(FileInfo fileInfo, Exception ex)
        {
            GameListViewItem item = gameslistViewJsItemsList.Find(it => it.Name == fileInfo.Name.Replace(".js", "") || it.Name == fileInfo.Name);

            if (item == null)
            {
                blackListedJSFile.Add(fileInfo.FullName, fileInfo.LastWriteTimeUtc);
                GameListViewItem newItem = MakeOrUpdateInvalidGameListViewItemInfo(new GameListViewItem(), fileInfo, ex);
                gameslistView.Items.Add(newItem);
                gameslistViewJsItemsList.Add(newItem);
            }
            else
            {
                MakeOrUpdateInvalidGameListViewItemInfo(item,fileInfo, ex);
            }

            ReSelectSelectedItem();
        }

        private GameListViewItem MakeOrUpdateInvalidGameListViewItemInfo(GameListViewItem item, FileInfo fileInfo,Exception ex)
        {          
            item.Info = new GameHandlerInfo();
            item.Info.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            item.Info.JSErrorLineIndex = GetJSErrorLineIndex(ex);
            item.Info.FileFullPath = fileInfo.FullName;
            item.Info.JsFileName = fileInfo.Name;
            item.ToolTipText = ex.Message;
            GetInvalideJSFileContent(item.Info);
            item.Text = "❖ " + fileInfo.Name.Replace(".js", "");
            item.FileType = ItemFileType.Invalid;
            item.Name = item.Text;

            bool blackListResult = blackListedJSFile.ContainsKey(fileInfo.FullName);

            if(!blackListResult)
            {
                blackListedJSFile.Add(fileInfo.FullName, fileInfo.LastAccessTimeUtc);
            }

            if (selectedItem != null && selectedItem.Text == item.Text)
            {
                updateButton.Visible = false;
                UpdateMainControlsLocation();
            }

            return item;
        }

        private string GetJSErrorLineIndex(Exception ex)
        {
            string[] lineSplit = ex.Message.Split(':');
            string splited = lineSplit?[0];
            string[] getNum = splited.Split(' ');

            bool lineIsNum = int.TryParse(getNum?.Last(), out int numLine);

            string details = ex.Message;

            if (lineIsNum)
            {
                int num = int.Parse(getNum?[1]);
                return details = $"{num / 2}";
            }

            return details;
        }

        private void GetInvalideJSFileContent(GameHandlerInfo info)
        {
            StringBuilder sb = new StringBuilder();

            int errorLine = -1;

            int lineIndex;
            bool isNumber = int.TryParse(info.JSErrorLineIndex, out lineIndex);

            if (isNumber)
            {
                errorLine = lineIndex;
            }

            int findErrorLine = 0;

            foreach (string line in File.ReadAllLines(info.FileFullPath))
            {
                sb.AppendLine(line /*+ "\n"*/);
                if (isNumber)
                {
                    if ((errorLine - 1) == findErrorLine)
                    {
                        info.JSErrorLineText = line;
                    }

                    findErrorLine++;
                }
                else
                {
                    if (info.JSErrorLineText == null)
                    {
                        if (line.Contains(info.JSErrorLineIndex.Split(' ').First()))
                        {
                            info.JSErrorLineText = line;
                        }
                    }
                }
            }

            info.JsContent = sb.ToString();
        }

        private CompressedHandlerData GetCompressedHandlerDatas(string zipFilePath)
        {
            if (!File.Exists(zipFilePath))
            {
                return null;
            }

            try
            {
                ZipFile.CheckZip(zipFilePath);
            }
            catch (Exception)
            {
                if (!blackListedZipFile.Keys.Contains(zipFilePath))
                {
                    blackListedZipFile.Add(zipFilePath, new FileInfo(zipFilePath).LastWriteTimeUtc);
                }

                return null;
            }

            CompressedHandlerData cData = new CompressedHandlerData();

            ZipFile zip = new ZipFile(zipFilePath);
            //zip.Password = password;//A voir

            if (!Directory.Exists(tempReadZipDirectory))
            {
                Directory.CreateDirectory(tempReadZipDirectory);
            }

            string tempFilePath = null;
            GenericGameInfo zippedGenericGameInfo = null;

            if (!Directory.Exists(tempReadZipDirectory))
            {
                Directory.CreateDirectory(tempReadZipDirectory);
            }

            int dirCount = 0;
            string packageDirName = null;

            foreach (ZipEntry e in zip)
            {
                if (e.IsDirectory)
                {
                    int count = 0;
                    foreach (char c in e.FileName)
                    {
                        if (c == '/')
                        {
                            count++;
                        }
                    }

                    if (dirCount == 0)
                    {
                        packageDirName = e.FileName.Replace("/", "");
                        dirCount++;
                    }

                    e.Extract(tempReadZipDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    if (e.FileName.EndsWith(".js"))
                    {
                        e.Extract(tempReadZipDirectory);
                        tempFilePath = Path.Combine(tempReadZipDirectory, e.FileName);

                        try
                        {
                            FileInfo zippedJSFileInfo = new FileInfo(tempFilePath);
                            zippedGenericGameInfo = new GenericGameInfo(zippedJSFileInfo, tempFilePath.Replace(".js", ""), new FileStream(tempFilePath, FileMode.Open, FileAccess.Read), false);
                        }
                        catch
                        {
                            zip?.Dispose();

                            if (Directory.Exists(tempReadZipDirectory))
                            {
                                Directory.Delete(tempReadZipDirectory, true);
                            }

                            return null;
                        }

                        if (zippedGenericGameInfo != null)
                        {
                            //zippedGenericGameInfos.Add(zippedGenericGameInfo);
                            cData.GenericGameInfo = zippedGenericGameInfo;

                            string sb = string.Empty;

                            foreach (string line in File.ReadAllLines(tempFilePath))
                            {
                                sb += line;
                                sb += "\n";
                            }

                            cData.JSContent = sb;
                        }

                        continue;
                    }

                    e.Extract(tempReadZipDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            if (packageDirName != null)
            {
                cData.PackageContent = ListPackageContent(Path.Combine(tempReadZipDirectory, packageDirName));
            }

            zip?.Dispose();

            if (Directory.Exists(tempReadZipDirectory))
            {
                Directory.Delete(tempReadZipDirectory, true);
            }

            return cData;
        }

        private void ShowItemsByType(object sender, EventArgs e)
        {
            if (refreshingAll)
            {
                return;
            }

            RadioButton optionRadio = (RadioButton)sender;

            if (!optionRadio.Checked)
            {
                return;
            }

            ItemSortingFlag sortingFlag = (ItemSortingFlag)optionRadio.Tag;
            currentSortingFlag = sortingFlag;

            form.Invoke((MethodInvoker)delegate
            {
                selectedItem = null;
                RefreshControlVisibility();
                SearchFieldTextChanged(searchField, null);
                HideThrobber();

                form.Invalidate();
            });
        }

        private void GetItemsBySortingFlag(object sortingFlag)
        {
            gameslistView.BeginUpdate();
            gameslistView.Items.Clear();

            foreach (GameListViewItem item in GetAllItems())
            {
                if (sortingFlag is string)
                {
                    string searchText = (string)sortingFlag;

                    if (!item.Info.JsContent.ToLower().Contains(searchText.ToLower()))
                    {
                        continue;
                    }
                }

                switch (currentSortingFlag)
                {
                    case ItemSortingFlag.Any:
                        {
                            gameslistView.Items.Add(item);
                            break;
                        }
                    case ItemSortingFlag.Installed:
                        {
                            if (item.Info.IsInstalled)
                                gameslistView.Items.Add(item);
                            break;
                        }
                    case ItemSortingFlag.NotInstalled:
                        {
                            if (!item.Info.IsInstalled && item.FileType != ItemFileType.Compressed)
                                gameslistView.Items.Add(item);
                            break;
                        }
                    case ItemSortingFlag.Archive:
                        {
                            if (item.FileType is ItemFileType.Compressed)
                                gameslistView.Items.Add(item);
                            break;
                        }
                }
            }

            gameslistView.EndUpdate();
            gameslistView.Sorting = SortOrder.Ascending;
        }

        //The "FileSupervisor" must only be used to managed file changes made out of the HandlerManager and only manage items not the files
        //ex: changes from an external text editor (file edition), a new file is added/removed (.js, or compressed)
        private void FileSupervisorTimer_Tick(object Object, EventArgs eventArgs)
        {
            if (!form.IsHandleCreated)
            {
                return;
            }

            if (DownloadPrompt.Instance != null || editMode)//So it don't mess with the regular deletion steps during extration(DownloadPrompt)
            {
                return;
            }

            string jsfolder = Core_Interface.GameManager.GetJsScriptsPath();

            if (!Directory.Exists(jsfolder))
            {
                form.Invoke((MethodInvoker)delegate
                {
                    blackListedJSFile.Clear();
                    blackListedZipFile.Clear();
                    selectedItem = null;
                    installedHandlersCount = 0;
                    notInstalledHandlersCount = 0;
                    handlersArchivesCount = 0;

                    gameslistViewZipItemsList.Clear();
                    gameslistViewJsItemsList.Clear();
                    gameslistView.Items.Clear();
                    gameslistView.Refresh();
                    RefreshControlVisibility();
                    form.Invalidate();
                    Directory.CreateDirectory(handlersDirectory);
                });
                return;
            }

            DirectoryInfo jsFolder = new DirectoryInfo(jsfolder);

            List<FileInfo> zippedFiles = jsFolder.EnumerateFiles().Where(f =>
                                         f.Extension.Equals(".nc", StringComparison.OrdinalIgnoreCase) ||
                                         f.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)).ToList();

            FileInfo[] fileInfos = jsFolder.GetFiles("*.js");


            int currentjsFileCount = fileInfos.Count() - 1;//-1 for masterhandler.js


            int currentzipFileCount = zippedFiles.Count();

            form.Invoke((MethodInvoker)delegate
            {
                gameslistView.BeginUpdate();
                //check for all items file update
                #region Per item file checksum comparison
                {
                    List<GameListViewItem> allItems = GetAllItems();

                    for (int i = 0; i < allItems.Count; i++)
                    {
                        GameListViewItem item = allItems[i];

                        if (item.FileType is ItemFileType.Compressed)
                        {
                            FileInfo zipFileInfo = new FileInfo(item.Info.ZipFileFullPath);

                            if (item.Info.LastWriteTimeUtc != zipFileInfo.LastWriteTimeUtc)//file edited or replaced?
                            {
                                form.SuspendLayout();

                                CompressedHandlerData zipGenInfo = GetCompressedHandlerDatas(item.Info.ZipFileFullPath);

                                if (zipGenInfo != null && zipGenInfo.GenericGameInfo != null)//we've got all the required datas ,the zip is valid and its js file too.
                                {
                                    HandlerInfo handlerInfo = new HandlerInfo(zipFileInfo, zipGenInfo.GenericGameInfo);

                                    SetGameListViewZipItemData(handlerInfo,item.Info.ZipFileFullPath, zipGenInfo.JSContent, zipGenInfo.PackageContent);

                                    if (blackListedZipFile.Keys.Contains(item.Info.ZipFileFullPath))
                                    {
                                        blackListedZipFile.Remove(item.Info.ZipFileFullPath);
                                    }
                                }
                                else
                                {
                                    RemoveItem(item, true);

                                    if (!blackListedZipFile.Keys.Contains(item.Info.ZipFileFullPath))
                                    {
                                        blackListedZipFile.Add(item.Info.ZipFileFullPath, zipFileInfo.LastWriteTimeUtc);
                                    }
                                }
                            }
                        }
                        else if (item.FileType is ItemFileType.JsFile || item.FileType is ItemFileType.Invalid)
                        {       
                            //the file has been deleted no need to bother more
                            if (!File.Exists(item.Info.FileFullPath))
                            {
                                form.SuspendLayout();

                                //work here
                                if(item.Info.IsInstalled)
                                {
                                    List<GameListViewItem> duplicates = gameslistViewJsItemsList.Where(glvi => glvi != item && /*!glvi.Info.IsInstalled &&*/ glvi.Info.GameGuid == item.Info.GameGuid).ToList();
                                    
                                    foreach (GameListViewItem listItem in duplicates)
                                    {
                                        listItem.Info.IsDuplicate = false;
                                        listItem.Info.CanBeInstalled = true;
                                        
                                        listItem.Text = listItem.Text.Replace("*", "");
                                    }
                                }

                                RemoveItem(item, false);

                                if (selectedItem != null)
                                {
                                    if(selectedItem == item)
                                    {
                                        selectedItem = null;
                                        RefreshControlVisibility();
                                    }
                                }
                            
                                continue;
                            }

                            if (item.FileType is ItemFileType.JsFile)
                            {
                                //if a game is installed or uninstalled from the mainform

                                //unstalled
                                if ((GameManager.Instance.User.Games.Find(ug => ug.FileFullPath == item.Info.FileFullPath && item.Info.IsInstalled) == null) && item.Info.IsInstalled)
                                {
                                    form.SuspendLayout();
                                    updateButton.Visible = false;
                                    item.Info.IsInstalled = false;

                                    PopulateHandlerInfo(item.Info);

                                    installButton.Text = "Install handler ✚";

                                    //bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == item.Info.GameGuid).Count() == 1;

                                    //if (!onlyOneJsForGame)//now there will be no handler with proper js file name so we can add any handler
                                    //{
                                    //    foreach (GameListViewItem listItem in gameslistViewJsItemsList)
                                    //    {
                                    //        if (listItem.Info.GameGuid != item.Info.GameGuid || listItem == item)
                                    //        {
                                    //            continue;
                                    //        }

                                    //        listItem.Info.IsDuplicate = false;
                                    //        listItem.Info.CanBeInstalled = true;
                                    //        listItem.Text = listItem.Text.Replace("*", "");
                                    //    }
                                    //}
                                    //if (item.Info.IsInstalled)
                                    //{

                                    //Next session need to create a proper universal function for that and check if it could be handler by the file supervisor only
                                    //(aka clicking button only do the minimal actions)
                                        List<GameListViewItem> duplicates = gameslistViewJsItemsList.Where(glvi => glvi != item && /*!glvi.Info.IsInstalled &&*/ glvi.Info.GameGuid == item.Info.GameGuid).ToList();

                                        foreach (GameListViewItem listItem in duplicates)
                                        {
                                            listItem.Info.IsDuplicate = false;
                                            listItem.Info.CanBeInstalled = true;

                                            listItem.Text = listItem.Text.Replace("*", "");
                                        }
                                    //}

                                    if (currentSortingFlag == ItemSortingFlag.NotInstalled)
                                    {
                                        gameslistView.Items.Remove(item);
                                        selectedItem = null;
                                    }

                                    UpdateMainControlsLocation();
                                    continue;
                                }
                                //installed
                                else if ((GameManager.Instance.User.Games.Find(ug => ug.FileFullPath == item.Info.FileFullPath && !item.Info.IsInstalled) != null) && !item.Info.IsInstalled)
                                {
                                    form.SuspendLayout();
                                    updateButton.Visible = updateButton.Visible = !string.IsNullOrEmpty(item.Info.Id) && item.Info.Version != -1 && HubConnected;
                                    item.Info.IsInstalled = true;

                                    PopulateHandlerInfo(item.Info);

                                    installButton.Text = "Uninstall handler ✘";


                                    List<GameListViewItem> duplicates = gameslistViewJsItemsList.Where(glvi => glvi != item && /*!glvi.Info.IsInstalled &&*/ glvi.Info.GameGuid == item.Info.GameGuid).ToList();

                                    foreach (GameListViewItem listItem in duplicates)
                                    {

                                        listItem.Info.IsDuplicate = true;
                                        listItem.Info.CanBeInstalled = false;

                                        if (!listItem.Text.EndsWith("*"))
                                        {
                                            listItem.Text = listItem.Text + "*";
                                        }
                                    }

                                    if (currentSortingFlag == ItemSortingFlag.Installed)
                                    {
                                        gameslistView.Items.Remove(item);
                                        selectedItem = null;
                                    }

                                    UpdateMainControlsLocation();
                                    continue;
                                }
                            }
                            //the file exists but has been replaced or edited?
                            FileInfo fileInfo = new FileInfo(item.Info.FileFullPath);

                            if (item.Info.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
                            {
                                continue;
                            }

                            form.SuspendLayout();
                            HandlerInfo handlerInfo = null;

                            //GameListViewItem itemToReselect = item == selectedItem ? item : null;

                            try
                            {
                                var genericGameInfo = GameManager.Instance.CheckGenericGameInfoIntegrity(fileInfo);

                                if (genericGameInfo is Exception exception)
                                {
                                    if(!blackListedJSFile.ContainsKey(fileInfo.FullName))
                                    {
                                        SetInvalidGameListViewItem(fileInfo, exception);
                                    }
                                   
                                    continue;
                                }

                                handlerInfo = new HandlerInfo(fileInfo, (GenericGameInfo)genericGameInfo);
                            }
                            catch (Exception ex)
                            {
                                SetInvalidGameListViewItem(fileInfo, ex);
                                continue;
                            }

                            if (handlerInfo != null)
                            {
                                UpdadateValidItemAfterEdit(item, fileInfo, handlerInfo);
                                RefreshUIAfterFileEdition(item, true);

                                string currentGameGUID = UI_Interface.CurrentGameListControl?.UserGameInfo?.GameGuid;

                                if (!string.IsNullOrEmpty(currentGameGUID))
                                {
                                    if (currentGameGUID == item?.Info?.GameGuid)
                                    {
                                        //make this a more secured there are to many possiblilities of error with all these inline checks
                                        var selectedControl = Core_Interface.GetGameControl(currentGameGUID);
                                        //!! not done yet!!Must check if the Curent_UserGameInfo is equal to the item USerGameInfo
                                        if (selectedControl != null)
                                        {
                                            Core_Interface.ReselectGameControl(selectedControl.Name);
                                        }
                                    }
                                }
                            }

                            //if (itemToReselect != null)
                            //{
                            //    selectedItem = itemToReselect;
                            //}
                        }
                    }
                }

                #endregion

                #region Compressed handlers
                if (blackListedZipFile.Count > 0)
                {
                    foreach (FileInfo zipInfo in zippedFiles)
                    {
                        if (blackListedZipFile.Keys.Contains(zipInfo.FullName))
                        {
                            var date = blackListedZipFile[zipInfo.FullName];

                            if (blackListedZipFile[zipInfo.FullName] != zipInfo.LastWriteTimeUtc)
                            {
                                blackListedZipFile.Remove(zipInfo.FullName);
                            }
                        }
                    }
                }

                int previousZipFileCount = gameslistViewZipItemsList.Count() + blackListedZipFile.Count();

                if (currentzipFileCount > previousZipFileCount)//new compressed handler found
                {
                    form.SuspendLayout();

                    bool foundZip = false;

                    foreach (FileInfo zipInfo in zippedFiles)
                    {
                        if (gameslistViewZipItemsList.Any(zi => zi.Name == zipInfo.FullName))
                        {
                            continue;
                        }

                        CompressedHandlerData zipGenInfo = GetCompressedHandlerDatas(zipInfo.FullName);

                        if (zipGenInfo != null && zipGenInfo.GenericGameInfo != null)
                        {
                            HandlerInfo handlerInfo = new HandlerInfo(zipInfo, zipGenInfo.GenericGameInfo);
                            SetGameListViewZipItemData(handlerInfo, zipInfo.FullName, zipGenInfo.JSContent, zipGenInfo.PackageContent);
                        }

                        gameslistView.Sorting = SortOrder.Ascending;
                        foundZip = true;
                    }

                    if (foundZip)
                    {
                        if (selectedItem != null)
                        {
                            ReSelectSelectedItem();
                        }
                        else
                        {
                            RefreshControlVisibility();
                            form.Invalidate();
                        }

                        //return;//temp?? to test let it continue and see
                    }
                }

                #endregion

                #region JS handler

                int previousJsFileCount = gameslistViewJsItemsList.Count() + blackListedJSFile.Count();

                if (currentjsFileCount > previousJsFileCount)//Added a js file in handlers folder
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (fileInfo.Name == "MasterHandler.js" || gameslistViewJsItemsList.Any(gli => gli.Name == fileInfo.Name) || blackListedJSFile.Keys.Contains(fileInfo.Name))
                        {
                            continue;
                        }

                        HandlerInfo handlerInfo = null;

                        try
                        {
                            var genericGameInfo = GameManager.Instance.CheckGenericGameInfoIntegrity(fileInfo);

                            if (genericGameInfo is Exception exception)
                            {
                                SetInvalidGameListViewItem(fileInfo, exception);
                                continue;
                            }

                            handlerInfo = new HandlerInfo(fileInfo, (GenericGameInfo)genericGameInfo);
                        }
                        catch (Exception ex)
                        {
                            SetInvalidGameListViewItem(fileInfo, ex);
                            continue;
                        }

                        //GameListViewItem item = 
                        SetGameListViewItemData(handlerInfo);
                        //List<GameListViewItem> duplicates = gameslistViewJsItemsList.Where(glvi => glvi != item && /*!glvi.Info.IsInstalled &&*/ glvi.Info.GameGuid == item.Info.GameGuid).ToList();
                        
                        //found any duplicate so make this item a duplicate too
                        //(it doesn't matter if others are installed or not here)
                        //if(duplicates .Count > 0)//Not sure if it's necessary since it's checked in "SetGameListViewItemData(handlerInfo);"
                        //                         //Will check that later as i've got it working that way
                        //{
                        //    if(!item.Text.EndsWith("*"))
                        //    {
                        //        item.Text = item.Text + "*";
                        //    }
                            
                        //}                     
                    }

                    //if (selectedItem != null)//reselect the item and scroll so it's visible
                    //{
                    //    //ReSelectSelectedItem();
                    //}
                    //else
                    if (selectedItem == null)
                    {
                        RefreshControlVisibility();
                        form.Invalidate();
                    }
                }

                if (blackListedJSFile.Count > 0)
                {
                    for (int i = 0; i < blackListedJSFile.Count; i++)
                    {
                        string filePath = blackListedJSFile.ElementAt(i).Key;
                        DateTime fileLastWriteTimeUtc = blackListedJSFile.ElementAt(i).Value;

                        FileInfo fileInfo = new FileInfo(filePath);
                        HandlerInfo handlerInfo = null;

                        if (fileLastWriteTimeUtc != fileInfo.LastWriteTimeUtc)
                        {
                            GameListViewItem item = gameslistViewJsItemsList.Find(it => it.Text == "❖ " + fileInfo.Name.Replace(".js", ""));

                            if(item == null)
                            {
                                continue;
                            }

                            try
                            {
                                var genericGameInfo = GameManager.Instance.CheckGenericGameInfoIntegrity(fileInfo);

                                if (genericGameInfo is Exception exception)
                                {
                                    if(!blackListedJSFile.ContainsKey(fileInfo.FullName))
                                    {
                                        SetInvalidGameListViewItem(fileInfo, exception);
                                    }
                                   
                                    continue;
                                }

                                handlerInfo = new HandlerInfo(fileInfo, (GenericGameInfo)genericGameInfo);
                            }
                            catch (Exception ex)
                            {
                                SetInvalidGameListViewItem(fileInfo, ex);
                                continue;
                            }

                            blackListedJSFile.Remove(fileInfo.FullName);

                            UpdadateValidItemAfterEdit(item, fileInfo, handlerInfo);
                            RefreshUIAfterFileEdition(item,true);
                        }
                    }
                }

                form.ResumeLayout();
                gameslistView.EndUpdate();
                #endregion
            });
        }

        private async void ExtractHandler()
        {
            ShowThrobber();
            DownloadPrompt downloadPrompt = new DownloadPrompt(null, selectedItem.Info.ZipFileFullPath);
            downloadPrompt.ShowDialog();

            //the download prompt manage the zip deletion so we wait for it to finish
            while (File.Exists(selectedItem.Info.ZipFileFullPath) || SearchProgressForm.Instance != null)// ||Wait for user to select the exe
            {
                await Task.Delay(100);
            }

            string fileName = selectedItem.Info.GameName + ".js";
            HandlerInfo handlerInfo = GameManager.Instance.HandlerInfos.Where(aggi => aggi.FileName == fileName).FirstOrDefault();
            UserGameInfo newGameEntry = Core_Interface.GameManager.User.Games.Find(g => g.HandlerInfo.FileFullPath == selectedItem.Info.HandlerInfo.FileFullPath);

            //Remove the zip list item
            RemoveItem(selectedItem, false);

            //check if an item already exists for this exact .js file, if so delete it
            var existingToRemove = gameslistViewJsItemsList.Find(it => it.Info.JsFileName == fileName);

            if (existingToRemove != null)
            {
                RemoveItem(existingToRemove, false);
            }

            bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == selectedItem.Info.GameGuid).Count() == 1;

            if (!onlyOneJsForGame)//now there will be no handler with proper js file name so we can add any handler
            {
                foreach (GameListViewItem listItem in gameslistViewJsItemsList)
                {
                    if (listItem.Info.GameGuid != selectedItem.Info.GameGuid || listItem == selectedItem)
                    {
                        continue;
                    }

                    listItem.Info.IsDuplicate = true;
                    listItem.Info.CanBeInstalled = false;

                    if (!listItem.Text.EndsWith("*"))
                    {
                        listItem.Text = listItem.Text + "*";
                    }
                }
            }

            selectedItem = SetGameListViewItemData(handlerInfo);

            ReSelectSelectedItem();
            HideThrobber();
        }

        private void ReSelectSelectedItem()
        {
            if (selectedItem == null)
            {
                return;
            }

            GameListViewItem updateItem = null;

            if (selectedItem.FileType is ItemFileType.Invalid)
            {
                foreach (GameListViewItem item in gameslistView.Items)
                {
                    if (item.Info.JsFileName == selectedItem.Info.JsFileName)
                    {
                        updateItem = item;
                        break;
                    }
                }
            }
            else
            {
                foreach (GameListViewItem item in gameslistView.Items)
                {
                    if (item.FileType is ItemFileType.Compressed)
                    {
                        continue;
                    }

                    if (item.Info.FileFullPath == selectedItem.Info.FileFullPath)
                    {
                        updateItem = item;
                        break;
                    }
                }
            }

            if (updateItem != null)
            {
                updateItem.Checked = true;
                updateItem.Selected = true;
                gameslistView.FocusedItem = updateItem;
                selectedItem = updateItem;

                ItemActivate(updateItem, null);
            }
        }

        private void RemoveItem(GameListViewItem item,bool setSelectedItemNull)
        {
            if (item.FileType is ItemFileType.JsFile)
            {            
                if (item.Info.IsInstalled)
                {
                    installedHandlersCount--;
                }
                else
                {
                    notInstalledHandlersCount--;
                }

                if (blackListedJSFile.ContainsKey(item.Info.FileFullPath))
                {
                    blackListedJSFile.Remove(item.Info.FileFullPath);
                }

                gameslistViewJsItemsList.Remove(item);           
            }
            else
            {
                if (blackListedZipFile.ContainsKey(item.Info.ZipFileFullPath))
                {
                    blackListedJSFile.Remove(item.Info.ZipFileFullPath);
                }

                gameslistViewZipItemsList.Remove(item);
                handlersArchivesCount--;
            }

            gameslistView.Items.Remove(item);

            if (selectedItem == item)
            {
                if (setSelectedItemNull)
                {
                    selectedItem = null;
                }
                
                RefreshControlVisibility();
                form.Invalidate();
            }
            else
            {
                //ReSelectSelectedItem();
            }
        }

        #region CONTROLS RESIZING EVENTS
        private void OnDescPackageLabelSizeChanged(object sender, EventArgs e)
        {
            Label desc = sender as Label;
            packagejsContentView.Location = new Point(desc.Left, desc.Bottom + 8);
        }

        private void OnFormSizeChanged(object sender, EventArgs e)
        {
            throbber.Location = new Point((form.Width / 2) - 25, (form.Height / 2) - 25);

            int jsContentViewHeight = (form.Bottom - (handlerInfoPanel.Bottom + 10)) - (form.Bottom - gameslistView.Bottom);
            jsContentView.Size = new Size((form.Width - (handlerInfoPanel.Left)) - 30, jsContentViewHeight);
            jsContentView.Location = new Point(handlerInfoPanel.Left, handlerInfoPanel.Bottom + 10);
            saveChangeButton.Location = new Point(jsContentView.Right - saveChangeButton.Width, jsContentView.Top - (saveChangeButton.Height + 5));
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

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            form.Invalidate();
        }

        #endregion

        #region CONTROLS MOUSE EVENTS

        bool editMode = false;

        private void SavechangeButton_MouseDown(object sender, MouseEventArgs e)
        {
            //we don't add the file to blacklist here since the changes aren't saved to file in case of error.
            if (selectedItem == null)
            {
                return;//To update becaus the button should be invisible in this case
            }

            saveChangeButton.Enabled = false;
            editMode = true;

            GameListViewItem item = selectedItem;


            string tempFilePath = item.Info.FileFullPath.Replace(item.Info.JsFileName, "temp.js");
            string savePath = item.Info.FileFullPath;
            string content = jsContentView.TextEditor.Text;

            using (FileStream stream = new FileStream(tempFilePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(content);
                    stream.Flush();
                }
            }

            FileInfo fileInfo = null;
            FileInfo tempFileInfo = new FileInfo(tempFilePath);
            HandlerInfo handlerInfo = null;
            
            //We do not create a new item because we just need to update the item info not reset/refresh everything
            try
            {
                var genericGameInfo = GameManager.Instance.CheckGenericGameInfoIntegrity(tempFileInfo);

                if (genericGameInfo is Exception exception)
                {
                    item.FileType = ItemFileType.Invalid;
                    string errorIndex = GetJSErrorLineIndex(exception);
                    item.Info.JSErrorLineIndex = errorIndex;
                    GetInvalideJSFileContent(item.Info);
                    HighlightWordEditor(jsContentView.TextEditor, item.Info.JSErrorLineIndex, false);
                    item.ToolTipText = item.Info.JSErrorLineText;
                    string errorText = errorIndex != null ? $"Error at line: {errorIndex} " : "";

                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Warning}|Save Fail @* *@{OSDHighlightColors.Regular}|{errorText} => {item.Info.JSErrorLineText}@*");
                    
                    if(File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }

                    editMode = false;
                    return;
                }

                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(content);
                        stream.Flush();
                    }
                }

                fileInfo = new FileInfo(savePath);

                handlerInfo = new HandlerInfo(fileInfo, (GenericGameInfo)genericGameInfo);
            }
            catch (Exception ex)
            {      
                item.FileType = ItemFileType.Invalid;
                string errorIndex = GetJSErrorLineIndex(ex);
                item.Info.JSErrorLineIndex = errorIndex;
                GetInvalideJSFileContent(item.Info);
                HighlightWordEditor(jsContentView.TextEditor, item.Info.JSErrorLineIndex, false);
                item.ToolTipText = item.Info.JSErrorLineText;
                string errorText = errorIndex != null ? $"Error at line: {errorIndex} " : "";

                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Warning}|Save Fail @* *@{OSDHighlightColors.Regular}|{errorText} => {item.Info.JSErrorLineText}@*");
                RefreshUIAfterFileEdition(item,false);
                editMode = false;
                return;
            }

            if(blackListedJSFile.ContainsKey(fileInfo.FullName))
            {
                blackListedJSFile.Remove(fileInfo.FullName);
            }

            UpdadateValidItemAfterEdit(item, fileInfo, handlerInfo);
            RefreshUIAfterFileEdition(item, false);
            editMode = false;
            Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Confirm}|Saved@");
        }

        private void UpdadateValidItemAfterEdit(GameListViewItem item, FileInfo fileInfo, HandlerInfo handlerInfo)
        {
            if(item == null)
            {
                //not something that should happen
                return;
            }

            UserGameInfo addedGame = Core_Interface.GameManager.User.Games.Find(g => g.HandlerInfo?.FileName == handlerInfo?.FileName);

            item.Info.HandlerInfo = handlerInfo;
            item.Info.GameGuid = handlerInfo.GUID;
            item.Info.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
            item.Info.JsContent = File.ReadAllText(fileInfo.FullName);
            item.Info.BinariesFolder = handlerInfo.GenericGameInfo.BinariesFolder != null ? handlerInfo.GenericGameInfo.BinariesFolder : "";
            
            string packagePath = Path.Combine(handlersDirectory, jsFileNameRegex.Replace(handlerInfo.GameName, ""));

            if (Directory.Exists(packagePath))
            {
                item.Info.PackageContent = ListPackageContent(packagePath);
            }

            if (addedGame != null)
            {
                item.Info.IsInstalled = true;
                item.Info.UserGameInfo = addedGame;
                item.Info.UpdateAvailable = addedGame.HandlerInfo.GenericGameInfo.Hub.UpdateAvailable;

                string currentGameGUID = UI_Interface.CurrentGameListControl?.UserGameInfo?.GameGuid;

                if (!string.IsNullOrEmpty(currentGameGUID))
                {
                    if (currentGameGUID == item?.Info?.GameGuid)
                    {
                        var selectedControl = Core_Interface.GetGameControl(currentGameGUID);
                        if (selectedControl != null)
                        {
                            Core_Interface.ReselectGameControl(selectedControl.Name);
                        }
                    }
                }
            }
            else
            {
                item.Info.IsInstalled = false;
            }

            //so update is check once usergameinfo.handlerinfo id s updated (if it's)
            item.Info.Version = handlerInfo.GenericGameInfo.Hub.Handler.Version;
           
            item.ToolTipText = item.Info.LastWriteTimeUtc.ToString();
            item.Text = "❖ " + handlerInfo.GameName;
            item.BackColor = darkColor;
            item.FileType = ItemFileType.JsFile;
        }

        private void RefreshUIAfterFileEdition(GameListViewItem item, bool isExternalChange)
        {
            if (selectedItem != null && selectedItem.Info.FileFullPath == item.Info.FileFullPath)
            {
                saveChangeButton.Enabled = false;
                saveChangeButton.Visible = true;
                jsContentView.SearchPanel.SearchPattern = "";

                if (searchField.Text != "" && searchField.Text != searchFieldHint)
                {
                    HighlightWordEditor(jsContentView.TextEditor, searchField.Text, true);
                }

                if (isExternalChange)
                {
                    jsContentView.TextEditor.Text = item.Info.JsContent;
                }

                deleteJsFileButton.Visible = true;
                handlerInfoPanel.Visible = true;
                openJsFileButton.Visible = true;
                jsContentView.TextEditor.TextArea.SelectionBrush = textSelectionBrush;
                jsContentView.SearchPanel.MatchCase = false;
                jsContentView.SearchPanel.Open();

                PopulateHandlerInfo(item.Info);

                if (item.Info.PackageContent != null)
                {
                    packagejsContentView.Text = item.Info.PackageContent;
                }

                updateButton.Visible = item.Info.IsInstalled && !string.IsNullOrEmpty(item.Info.Id) && item.Info.Version != -1 && HubConnected;

                if (updateButton.Visible)
                {
                    if (item.Info.UpdateAvailable)
                    {
                        updateButton.Text = "Update handler 🠋";
                        updateButton.ForeColor = Color.White;
                        updateButton.Cursor = Theme_Settings.Hand_Cursor;
                    }
                    else if (!item.Info.UpdateAvailable)
                    {
                        updateButton.Text = "Handler is up-to-date ✔";
                        updateButton.ForeColor = Color.White;
                        updateButton.Cursor = Theme_Settings.Default_Cursor;
                    }
                }

                deleteJsFileButton.Text = "Delete handler files ⚠";

                if (item.Info.IsDuplicate)
                {
                    installButton.Text = "Handler duplicate";
                    installButton.ForeColor = Color.IndianRed;
                    installButton.Visible = true;
                    item.Info.CanBeInstalled = false;
                }
                else if (item.Info.IsInstalled)
                {
                    installButton.Text = "Uninstall handler ✘";
                    installButton.ForeColor = Color.White;
                    installButton.Visible = true;
                }
                else if (!item.Info.IsInstalled)
                {
                    installButton.Text = "Install handler ✚";
                    installButton.ForeColor = Color.White;
                    installButton.Visible = true;
                }                
                else
                {
                    installButton.Visible = false;
                }

                openJsPackageButton.Visible = Directory.Exists(item.Info.FileFullPath.Replace(".js", ""));
                jsContentView.Visible = true;

                UpdateMainControlsLocation();
            }
        }

        private void RefreshButton_MouseDown(object sender, MouseEventArgs e)
        {
            selectedItem = null;
            RefreshControlVisibility();
            form.Invalidate();
            UpdateHandlersInfo();
        }

        private void SearchFieldMouseDown(object sebder, EventArgs e)
        {
            firstClick = true;
        }

        private void ListView_MouseMove(object sender, MouseEventArgs e)
        {
            CursorPos = e.Location;
            gameslistView.Cursor = Theme_Settings.Hand_Cursor;
        }

        private void Form_MouseEnter(object sender, EventArgs e)
        {
            form.Cursor = Theme_Settings.Default_Cursor;
        }

        private void Buttons_MouseEnter(object sender, EventArgs e)
        {
            Control ctrl = (Control)sender;
            ctrl.ForeColor = Color.LightSeaGreen;
            ctrl.Invalidate();

            if(ctrl == openJsPackageButton)
            {
                packagejsContentWindow.Location = form.PointToScreen(new Point(ctrl.Right,ctrl.Bottom));
                packagejsContentWindow.BringToFront();
                packagejsContentWindow.Show() ;
            }
        }

        private void Buttons_MouseLeave(object sender, EventArgs e)
        {
            Control ctrl = (Control)sender;
            ctrl.ForeColor = ctrl != saveChangeButton ? Color.White : Color.RoyalBlue;
            ctrl.Invalidate();

            if (ctrl == openJsPackageButton)
            {
                packagejsContentWindow.Hide();
            }
        }

        private async void UpdateButton_MouseDown(object sender, MouseEventArgs e)
        {
            if(extracting)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (selectedItem != null)
                {
                    if (selectedItem.Info.UpdateAvailable)
                    {
                        ShowThrobber();
                        Handler handler = await HubCache.SearchByIdOnline(selectedItem.Info.Id);
                        string zipName = string.Format("handler-{0}-v{1}.nc", handler.Id, handler.CurrentVersion);
                        DownloadPrompt dlp = new DownloadPrompt(handler, null, true);
                        dlp.ShowDialog();

                        FileInfo fileInfo = new FileInfo(selectedItem.Info.FileFullPath);

                        if (blackListedJSFile.Keys.Contains(fileInfo.Name))
                        {
                            HideThrobber();
                            return;//need a messagebox because previous handler was fine??
                        }

                        HandlerInfo handlerInfo = null;
                     
                        try
                        {
                            string ext = Path.GetFileNameWithoutExtension(fileInfo.Name);
                            string pathBlock = Path.Combine(fileInfo.Directory.FullName, ext);
                            GenericGameInfo genericGameInfo = null;

                            using (Stream str = fileInfo.OpenRead())
                            {  
                                genericGameInfo = new GenericGameInfo(fileInfo, pathBlock, str, false);
                            }

                            if (genericGameInfo.GameName == null)
                            {
                                SetInvalidGameListViewItem(fileInfo, new Exception("\"Game.GameName\" can't be null"));
                                HideThrobber();
                                return;
                            }
                            else if (genericGameInfo.GUID == null)
                            {
                                SetInvalidGameListViewItem(fileInfo, new Exception("\"Game.GUID\" can't be null"));
                                HideThrobber();
                                return;
                            }

                            handlerInfo = new HandlerInfo(fileInfo, genericGameInfo);
                        }
                        catch (Exception ex)
                        {
                            SetInvalidGameListViewItem(fileInfo, ex);
                            HideThrobber();
                            return;
                        }

                        RemoveItem(selectedItem, false);

                        selectedItem = SetGameListViewItemData(handlerInfo);

                        RefreshControlVisibility();
                        ReSelectSelectedItem();
                        HideThrobber();
                    }
                }
            }
        }

        private async void InstallButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bool requiresUpdate;

                if (selectedItem.FileType == ItemFileType.JsFile)
                {
                    if (!selectedItem.Info.CanBeInstalled)
                    {
                        return;
                    }

                    if (selectedItem.Info.IsInstalled)//Uninstall
                    {
                        if(selectedItem.Info.UserGameInfo == null)
                        {
                            //add an error window if it happen again?
                            return;
                        }

                        requiresUpdate = RemoveGame.Remove(selectedItem.Info.UserGameInfo, true);

                        if (requiresUpdate)
                        {
                            updateButton.Visible = false;
                            selectedItem.Info.IsInstalled = false;

                            PopulateHandlerInfo(selectedItem.Info);

                            installButton.Text = "Install handler ✚";

                            bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == selectedItem.Info.GameGuid).Count() == 1;

                            if (!onlyOneJsForGame)//now there will be no handler with proper js file name so we can add any handler
                            {
                                foreach (GameListViewItem listItem in gameslistViewJsItemsList)
                                {
                                    if (listItem.Info.GameGuid != selectedItem.Info.GameGuid || listItem == selectedItem)
                                    {
                                        continue;
                                    }

                                    listItem.Info.IsDuplicate = false;
                                    listItem.Info.CanBeInstalled = true;
                                    listItem.Text = listItem.Text.Replace("*","");
                                }
                            }

                            if (currentSortingFlag == ItemSortingFlag.Installed)
                            {
                                gameslistView.Items.Remove(selectedItem);
                                selectedItem = null;
                            }
                        }
                    }
                    else//Install
                    {
                        StringBuilder sb = new StringBuilder();
                        var exeNames = selectedItem.Info.HandlerInfo.GenericGameInfo.ExecutableNames;

                        if (exeNames != null)
                        {
                            for (int i = 0; i < exeNames.Length; i++)
                            {
                                string exeName = exeNames[i];
                                sb.Append($"{exeName};");
                            }
               
                            requiresUpdate = await SearchGame.Search(sb.ToString(), selectedItem.Info.HandlerInfo);
                        }
                        else
                        {
                            requiresUpdate = await SearchGame.Search(selectedItem.Info.HandlerInfo.GenericGameInfo.ExecutableName, selectedItem.Info.HandlerInfo);
                        }

                        if (requiresUpdate)//if a executable has been selected by the user
                        {

                            UserGameInfo userGameInfo = GameManager.Instance.User.Games.Find(ug => ug.FileFullPath == selectedItem.Info.FileFullPath);

                            selectedItem.Info.UserGameInfo = userGameInfo;
                            selectedItem.Info.IsInstalled = true;
                            
                            PopulateHandlerInfo(selectedItem.Info);
                          
                            bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == selectedItem.Info.GameGuid).Count() == 1;

                            if (!onlyOneJsForGame)//now there will be no handler with proper js file name so we can add any handler
                            {
                                foreach (GameListViewItem listItem in gameslistViewJsItemsList)
                                {
                                    if (listItem.Info.GameGuid != selectedItem.Info.GameGuid || listItem == selectedItem)
                                    {
                                        continue;
                                    }

                                    listItem.Info.IsDuplicate = true;
                                    listItem.Info.CanBeInstalled = false;

                                    if (!listItem.Text.EndsWith("*"))
                                    {
                                        listItem.Text = listItem.Text + "*";
                                    }
                                }
                            }

                            if (currentSortingFlag == ItemSortingFlag.NotInstalled)
                            {
                                gameslistView.Items.Remove(selectedItem);
                                selectedItem = null;
                                UpdateMainControlsLocation();
                                return;
                            }

                            updateButton.Visible = !string.IsNullOrEmpty(selectedItem.Info.Id) && selectedItem.Info.Version != -1 && HubConnected;
                            installButton.Text = "Uninstall handler ✘";
                        }
                    }
                }
                else
                {
                    form.Invalidate();
                    DialogResult dialogResult = MessageBox.Show(
                        $"Do you want to extract this handler?", "Extract handler?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        ExtractHandler();
                    }
                }

                UpdateMainControlsLocation();
            }
        }


        private void UpdateMainControlsLocation()
        {
            installButton.Location = updateButton.Visible ? new Point(updateButton.Right + 5, searchField.Top) : new Point(updateButton.Left, searchField.Top);

            deleteJsFileButton.Location = new Point(installButton.Right + 5, searchField.Top);
            openJsFileButton.Location = new Point(deleteJsFileButton.Right + 5, searchField.Top);
            openJsPackageButton.Location = new Point(openJsFileButton.Right + 5, searchField.Top);

            if(handlerInfoPanel.Visible)
            {
                int jsContentViewHeight = (form.Bottom - (handlerInfoPanel.Bottom + 10)) - (form.Bottom - gameslistView.Bottom);
                jsContentView.Size = new Size((form.Width - (handlerInfoPanel.Left)) - 30, jsContentViewHeight);
                jsContentView.Location = new Point(handlerInfoPanel.Left, handlerInfoPanel.Bottom + 10);
            }
            else
            {
                int jsContentViewHeight = (form.Bottom - (installButton.Bottom + 10)) - (form.Bottom - gameslistView.Bottom);
                jsContentView.Size = new Size((form.Width - (handlerInfoPanel.Left)) - 30, jsContentViewHeight);
                jsContentView.Location = new Point(handlerInfoPanel.Left, installButton.Bottom + 10);
            }
           
            saveChangeButton.Location = new Point(jsContentView.Right - saveChangeButton.Width, jsContentView.Top - (saveChangeButton.Height + 5));

            form.Invalidate();
            gameslistView.Invalidate();
        }

        private void DeleteJsFileButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (selectedItem.FileType == ItemFileType.JsFile)
                {
                    bool hFileExist = File.Exists(selectedItem.Info.FileFullPath);
                    bool hFolderExist = Directory.Exists(selectedItem.Info.FileFullPath.Replace(".js", ""));

                    string dir = hFolderExist ? " and its package directory" : "";
                    DialogResult dialogResult = MessageBox.Show(
                    $"Delete {selectedItem.Info.GameName} handler file{dir}? ",
                    "Delete handler?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        if (hFileExist)
                        {
                            FileUtil.SendToRecycleBin(selectedItem.Info.FileFullPath);
                        }

                        if (hFolderExist)
                        {
                            FileUtil.SendToRecycleBin(selectedItem.Info.FileFullPath.Replace(".js", ""));
                        }


                        //let the file supervisor manage the item(s) info update

                        //RefreshControlVisibility();

                        ////bool onlyOneJsForGame = GameManager.Instance.HandlerInfos.Where(g => g.GUID == selectedItem.Info.GameGuid).Count() == 1;
                        //List<GameListViewItem> duplicates = gameslistViewJsItemsList.Where(glvi => glvi != selectedItem && glvi.Info.GameGuid == selectedItem.Info.GameGuid).ToList();

                        ////if (!onlyOneJsForGame)//now there will be no handler with proper js file name so we can add any handler
                        ////{
                        //foreach (GameListViewItem listItem in duplicates)
                        //{
                        //    if (listItem.Info.IsInstalled)
                        //    {
                        //        continue;
                        //    }
                        //    //if (listItem.Info.GameGuid != selectedItem.Info.GameGuid || listItem == selectedItem)
                        //    //{
                        //    //    continue;
                        //    //}

                        //    listItem.Info.IsDuplicate = false;
                        //    listItem.Info.CanBeInstalled = true;
                        //    listItem.Text = listItem.Text.Replace("*", "");
                        //}
                        ////}

                        //RemoveItem(selectedItem, true);
                        //GetItemsBySortingFlag(currentSortingFlag);
                    }
                }
                else if (selectedItem.FileType == ItemFileType.Compressed)
                {
                    bool hFileExist = File.Exists(selectedItem.Info.ZipFileFullPath);

                    if (hFileExist)
                    {
                        DialogResult dialogResult = MessageBox.Show(
                        $"Delete {selectedItem.Info.GameName} handler archive? ",
                        "Delete handler archive?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (dialogResult == DialogResult.Yes)
                        {
                            FileUtil.SendToRecycleBin(selectedItem.Info.ZipFileFullPath);
                            RefreshControlVisibility();
                            form.Invalidate();
                        }
                    }
                }
                else
                {
                    bool hFileExist = File.Exists(selectedItem.Info.FileFullPath);
                    bool hFolderExist = Directory.Exists(selectedItem.Info.FileFullPath.Replace(".js", ""));

                    string dir = hFolderExist ? " and its package directory" : "";
                    DialogResult dialogResult = MessageBox.Show(
                    $"Delete {selectedItem.Info.GameName} invalid handler file{dir}? ",
                    "Delete invalid handler?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        if (hFileExist)
                        {
                            FileUtil.SendToRecycleBin(selectedItem.Info.FileFullPath);
                        }

                        if (hFolderExist)
                        {
                            FileUtil.SendToRecycleBin(selectedItem.Info.FileFullPath.Replace(".js", ""));
                        }

                        var item = gameslistView.Items.Find("❖ " + selectedItem.Info.JsFileName.Replace(".js", ""), false).FirstOrDefault() as GameListViewItem;
                        if (item != null)
                        {
                            gameslistView.Items.Remove(item);
                            blackListedJSFile.Remove(selectedItem.Info.FileFullPath);
                        }

                        selectedItem = null;
                        RefreshControlVisibility();
                        form.Invalidate();
                    }
                }
            }
        }

        private void OpenJsFileButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (File.Exists(selectedItem.Info.FileFullPath))
            {
                try
                {
                    if (App_Misc.TextEditorPath != "Default")
                    {
                        Process.Start(App_Misc.TextEditorPath, "\"" + selectedItem.Info.FileFullPath + "\"");
                    }
                    else
                    {
                        Process.Start("notepad++.exe", "\"" + selectedItem.Info.FileFullPath + "\"");
                    }

                }
                catch (Exception)
                {
                    Process.Start("notepad.exe", selectedItem.Info.FileFullPath);
                }
            }
        }

        private void OpenJsPackageButton_MouseDown(object sender, MouseEventArgs e)
        {
            Process.Start(selectedItem.Info.FileFullPath.Replace(".js", ""));
        }

        private void OpenHandlersFolder_MouseDown(object sender, MouseEventArgs e)
        {
            Process.Start(handlersDirectory);
        }

        private void SortOptionsButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (sortOptionsPanel.Visible)
            {
                sortOptionsPanel.Visible = false;
            }
            else
            {
                sortOptionsPanel.BringToFront();
                sortOptionsPanel.Visible = true;
            }
        }

        #endregion

        private void SearchFieldGotFocus(object sender, EventArgs e)
        {
            if (!firstClick)
            {
                gameslistView.Focus();
                return;
            }

            if (searchField.Text == searchFieldHint)
                searchField.Text = "";
            searchField.Font = new Font("Aharoni", 10, FontStyle.Regular);
        }

        private List<GameListViewItem> GetAllItems()
        {
            List<GameListViewItem> allItems = new List<GameListViewItem>();
            allItems.AddRange(gameslistViewJsItemsList);
            allItems.AddRange(gameslistViewZipItemsList);
            return allItems;
        }

        #region CONTROLS VISIBIBLITY METHODS
        private void ShowThrobber()
        {
            throbber.Visible = true;
            throbber.BringToFront();
        }

        private void HideThrobber()
        {
            throbber.Visible = false;
        }

        private void RefreshControlVisibility()
        {
            saveChangeButton.Visible = false;
            openJsPackageButton.Visible = false;
            openJsFileButton.Visible = false;
            jsContentView.Visible = false;
            installButton.Visible = false;
            updateButton.Visible = false;
            packagejsContentView.Visible = false;
            deleteJsFileButton.Visible = false;
            handlerInfoPanel.Visible = false;
        }

        private void HideSortOptionsPanel(object sender, MouseEventArgs e)
        {
            sortOptionsPanel.Visible = false;
            CursorPos = e.Location;
        }

        private void HideDateTooltip(object sender, MouseEventArgs e)
        {
            Control con = (Control)sender;
            dateToolTipLabel.Visible = false;
            //only need the real cursor pos if the listView contains the cursor else just set a value out of the listView.
            CursorPos = con is SmoothListView ? e.Location : new Point(form.Width, form.Height);
        }

        #endregion

        private string ListPackageContent(string directoryToScan)
        {
            if (!Directory.Exists(directoryToScan))
            {
                return null;
            }

            List<string> files = Directory.EnumerateFileSystemEntries(directoryToScan, "*", SearchOption.AllDirectories).ToList();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < files.Count(); i++)
            {
                string dl = files[i];

                var fileName = dl.Split('\\').ToList();

                string dirName = directoryToScan.Split('\\').Last();
                int startIndex = fileName.IndexOf(dirName);

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

                if (info.Exists)//skip dirs
                    sb.AppendLine(clean);
            }

            return sb.ToString();
        }

        private void SearchFieldTextChanged(object sender, EventArgs e)
        {
            form.Invoke((MethodInvoker)delegate
            {
                TextBox sch = sender as TextBox;

                selectedItem = null;
                RefreshControlVisibility();

                if (!ProcessSearch(sch))
                {
                    GetItemsBySortingFlag(currentSortingFlag);
                    HideThrobber();
                    form.Invalidate();
                    return;
                }

                RefreshControlVisibility();
                form.Invalidate();

                GetItemsBySortingFlag(sch.Text);

                HideThrobber();
                form.Invalidate();
            });
        }

        private bool ProcessSearch(TextBox search)
        {
            if (search.Text == "") { return false; }
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

        #region PAINT EVENTS

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            int verticalSB = SystemInformation.VerticalScrollBarWidth;
            int horizontalSB = SystemInformation.HorizontalScrollBarHeight;

            e.Graphics.ResetClip();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(new Rectangle(gameslistView.Location.X - 5, gameslistView.Location.Y - 4, gameslistView.Width + 9, gameslistView.Height + 9), 8, 8, true, true, true, true))
            {
                e.Graphics.FillPath(darkBrush, backGp);
                e.Graphics.DrawPath(borderPen, backGp);
            }

            if (handlerInfoPanel.Visible)
            {
                using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(new Rectangle(handlerInfoPanel.Location.X - 3, handlerInfoPanel.Location.Y - 3, handlerInfoPanel.Width + 7, handlerInfoPanel.Height + 5), 8, 8, true, true, true, true))
                {
                    e.Graphics.FillPath(darkBrush, backGp);
                    e.Graphics.DrawPath(borderPen, backGp);
                }
            }

            if (jsContentView.Visible)
            {
                using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(new Rectangle(jsContentView.Location.X - 3, jsContentView.Location.Y - 3, jsContentView.Width + 7, jsContentView.Height + 6), 8, 8, true, true, true, true))
                {
                    e.Graphics.FillPath(darkBrush, backGp);
                    e.Graphics.DrawPath(borderPen, backGp);
                }
            }

            //if (packagejsContentView.Visible)
            //{
            //    using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(new Rectangle(packagejsContentView.Location.X - 3, packagejsContentView.Location.Y - 3, packagejsContentView.Width + 7, packagejsContentView.Height + 5), 8, 8, true, true, true, true))
            //    {
            //        e.Graphics.FillPath(darkBrush, backGp);
            //        e.Graphics.DrawPath(borderPen, backGp);
            //    }
            //}
        }

        private void GameslistView_Paint(object sender, DrawListViewItemEventArgs e)
        {
            GameListViewItem item = e.Item as GameListViewItem;

            e.DrawBackground();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            GameListViewItem customItem = gameslistViewJsItemsList.Find(it => it.Text == item.Text);

            Font resizeF = e.Item.Font;

            if (e.Item.Font.Size != e.Item.Font.Size / scale)
            {
                resizeF = new Font(e.Item.Font.FontFamily, e.Item.Font.Size / scale, e.Item.Font.Style);
            }
             
            if (customItem == null)//zip items
            {
                if (selectedItem != null && e.Item == selectedItem)
                {
                    if (item.FileType == ItemFileType.Invalid)
                    {
                        e.Graphics.FillRectangle(Brushes.Yellow, e.Item.Bounds);
                        TextRenderer.DrawText(e.Graphics, e.Item.Text + "", resizeF, e.Bounds, Color.Red, TextFormatFlags.Left);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(Brushes.Gray, e.Item.Bounds);
                        TextRenderer.DrawText(e.Graphics, e.Item.Text, resizeF, e.Bounds, Color.Orange, TextFormatFlags.Left);
                    }
                }
                else
                {
                    if (item.FileType == ItemFileType.Invalid)
                    {
                        e.Graphics.FillRectangle(darkBrush, e.Item.Bounds);
                        TextRenderer.DrawText(e.Graphics, e.Item.Text + "", resizeF, e.Bounds, Color.Red, TextFormatFlags.Left);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(darkBrush, e.Item.Bounds);
                        TextRenderer.DrawText(e.Graphics, e.Item.Text, resizeF, e.Bounds, Color.Orange, TextFormatFlags.Left);
                    }

                }

                return;
            }

            if (selectedItem != null && e.Item == selectedItem)
            {
                if (item.FileType == ItemFileType.Invalid)
                {
                    e.Graphics.FillRectangle(Brushes.Yellow, e.Item.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Item.Text + "", resizeF, e.Bounds, Color.Red, TextFormatFlags.Left);
                }
                else
                {
                    e.Graphics.FillRectangle(Brushes.DodgerBlue, e.Item.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Item.Text, resizeF, e.Bounds, Color.White, TextFormatFlags.Left);
                }
            }
            else
            {
                if (item.FileType == ItemFileType.Invalid)
                {
                    e.Graphics.FillRectangle(darkBrush, e.Item.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Item.Text + "", resizeF, e.Bounds, Color.Red, TextFormatFlags.Left);
                }
                else
                {
                    e.Graphics.FillRectangle(darkBrush, e.Item.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Item.Text, resizeF, e.Bounds, customItem.Info.IsInstalled  ? Color.LightSeaGreen : Color.Gray, TextFormatFlags.Left);
                }

            }

            if (e.Item.Bounds.Contains(CursorPos))
            {
                //e.Graphics.FillRectangle(Brushes.Aqua, new Rectangle(e.Item.Bounds.X + 1, e.Item.Bounds.Y + 1, e.Item.Bounds.Width - 2, e.Item.Bounds.Height - 2));
                dateToolTipLabel.Text = e.Item.ToolTipText;
                dateToolTipLabel.Location = new Point(CursorPos.X + 20, e.Bounds.Top);

                if (dateToolTipLabel.Right > gameslistView.Width)
                {
                    dateToolTipLabel.Location = new Point((CursorPos.X - dateToolTipLabel.Width) - 20, e.Bounds.Top);
                }

                dateToolTipLabel.Visible = true;
            }
        }

        #endregion

        #region HIGHLIGHT TEXT METHODS
        private void HighlightWordEditor(TextEditor editor, string word, bool searchAll)
        { //Don't forget to cache the System.Windows.Media.SolidColorBrush
            editor.TextArea.SelectionBrush = textSelectionBrush;

            var doc = editor.Document;

            if (searchAll)
            {
                jsContentView.SearchPanel.Close();
                //Don't forget to cache the System.Windows.Media.SolidColorBrush
                jsContentView.SearchPanel.MarkerBrush = searchPanelMarkerBrush;
                jsContentView.SearchPanel.SearchPattern = word;
                jsContentView.SearchPanel.MatchCase = true;
                jsContentView.SearchPanel.UseRegex = false;


                // Open the panel if you want visual feedback
                jsContentView.SearchPanel.Open();
                //jsContentView.SearchPanel.BringIntoView();
                return;
            }

            int lineIndex = -1;

            bool isNumber = int.TryParse(word, out lineIndex);
            if (isNumber)
            {
                var line = editor.Document.GetLineByNumber(lineIndex);

                editor.Select(line.Offset, line.Length);
                editor.TextArea.Caret.Offset = line.EndOffset;
                editor.TextArea.Focus();
                editor.ScrollToLine(lineIndex);
            }
            else
            {
                foreach (DocumentLine line in doc.Lines)
                {
                    int index = doc.GetText(line).IndexOf(word);

                    if (index >= 0)
                    {
                        int offset = line.Offset + index;
                        editor.Select(offset, word.Length);
                        break;
                    }
                }

                editor.ScrollToLine(editor.Document.GetLineByOffset(editor.SelectionStart).LineNumber);
            }
        }
        #endregion

        private CompletionWindow completionWindow;

        private List<string> keywords = new List<string>();

        private List<string> currentNameSpaceStringList;//must be "manually" reseted at some points (itemActive,update,etc)
        private List<string> previousNameSpaceStringList;

        private Dictionary<string, List<string>> nameSpaces = new Dictionary<string, List<string>>
        {
            {"Nucleus", new List<string>()},
            {"ProtoInput",  new List<string>()},
            {"Game",  new List<string>()},
            {"Context", new List<string>()},
            {"Hook", new List<string>()},
            {"System", new List<string>()},
            {"WM", new List<string>()},
            {"SearchType", new List<string>()},
            {"Folder", new List<string>()},
            {"Values", new List<string>()},
            {"Player", new List<string>()},
            {"RegType", new List<string>()}
        };

        private Dictionary<string, List<string>> values = new Dictionary<string, List<string>>();
        private List<string> currentValuesStringList;//must be "manually" reseted at some points (itemActive,update,etc)
        private List<string> previousValuesStringList;

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Show completion list only for letters
            if (char.IsLetter(e.Text[0]) || e.Text == "." /*|| e.Text == "="*/)
            {
                ShowCompletion();
            }
        }

        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            //When user types a character while completion is open
            if (completionWindow == null) return;

            //Only filter for letters/digits/underscore
            if (e.Text.Length > 0 && ((!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_') || e.Text[0] == '.'))
            {
                //Commit the current selection when non-word character typed
                completionWindow.CompletionList.RequestInsertion(e);
            }
        }

        private void ShowCompletion()
        {
            var editor = jsContentView.TextEditor;
            //Get the word fragment before the caret
            int offset = editor.CaretOffset;
            var document = editor.Document;

            int startOffset = offset;
            while (startOffset > 0)
            {
                char c = document.GetCharAt(startOffset - 1);
                if ((!char.IsLetterOrDigit(c) && (c != '_')) || (c == '.') /*|| (c == '=')*/)
                    break;
                startOffset--;
            }

            string prefix = document.GetText(startOffset, offset - startOffset);

            List<string> matches = new List<string>();
            List<string> getAutoComplete = currentNameSpaceStringList;
            
            //Filter keywords by prefix
            matches = getAutoComplete?
               .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
               .ToList();

            if (matches.Count == 0)
                return;

            completionWindow = new CompletionWindow(editor.TextArea);
            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

            foreach (var match in matches)
            {
                data.Add(new MyCompletionData(match));
            }

            completionWindow.StartOffset = startOffset; // replace only the prefix
            completionWindow.Show();
            completionWindow.Closed += (o, args) => completionWindow = null;
        }

        private void Document_TextChanged(object sender, DocumentChangeEventArgs e)
        {
            int caretOffset = e.Offset;

            if(caretOffset == 0)
            {
                return;
            }


            saveChangeButton.Visible = selectedItem.FileType == ItemFileType.JsFile;
            saveChangeButton.Enabled = true;
            TextEditor editor = jsContentView.TextEditor;

            //Get the document line containing the caret
            var line = editor.Document.GetLineByOffset(caretOffset);

            //Get the full text of that line
            string lineText = editor.Document.GetText(line);

            if (lineText == "")
            {
                currentNameSpaceStringList = keywords;
                return;
            }

            List<string> matchingNameSpace = null;

            foreach(KeyValuePair<string,List<string>> nameSpace in nameSpaces)
            {
                if(caretOffset - nameSpace.Key.Length < 0)
                {
                    continue;
                }

                string catchNameSpace = editor.Document.GetText(caretOffset - nameSpace.Key.Length, nameSpace.Key.Length);

                if (catchNameSpace == nameSpace.Key)
                {
                    matchingNameSpace = nameSpace.Value;                  
                    break;                  
                }
            }

            currentNameSpaceStringList = matchingNameSpace ?? keywords;
        }
    
        public class MyCompletionData : ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
        {
            public MyCompletionData(string text)
            {
                this.Text = text;
            }

            public System.Windows.Media.ImageSource Image => null;
            public string Text { get; private set; }
            public object Content => this.Text;
            public object Description => this.Text;
            public double Priority => 0;

            public void Complete(TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, this.Text);
            }
        }

        private readonly string[] JavaScriptKeywords = new[]
        {
             "break", "case", "const", "continue", "default",
             "do", "else", "finally", "for", "function", "if", "in",
             "let", "new", "return", "switch", "this", "var", "while","true","false","Count"
        };

        private void ParseJsContentForAutoCompletion(string stringToParse)
        {
            string[] lines = stringToParse.Split('\n');

            foreach (string line in lines)
            {
                var toLowerLine = line.ToLower();

                if(toLowerLine.Contains("hub") || /*toLowerLine.Contains("gamename") ||*/ toLowerLine.Contains("binaries"))
                {
                    continue;
                }

                //var getValue = line.Split('=');


                //if (getValue.Length >= 1)
                //{

                //    if(getValue.Last().EndsWith(";"))
                //        Console.WriteLine(getValue.Last());
                //    //foreach(var t in getValue)
                //    //{


                //    //}
                //    //;
                //}
                string result = Regex.Replace(line, "[^a-zA-Z0-9]", " ");
                List<string> words = result.Split(' ').ToList();

                for (int i = 0; i < words.Count; i++)
                {
                    string word = words[i];

                    

                    if (word == "" || word == " " || char.IsLower(word[0]))//there are no variable starting with lower case
                    {
                        words.Remove(word);
                        i--;
                        continue;
                    }

                    

                    if (!keywords.Contains(word))
                    {
                        keywords.Add(word);
                        autoCompleteSource.Add(word);
                    }

                    if (nameSpaces.Keys.Contains(word))
                    {
                        if (i < words.Count - 1 /*&& words.Count > 1*/)
                        {
                            if (!nameSpaces[word].Contains(word))
                            {
                                nameSpaces[word].Add(word);
                            }

                            if (words[i + 1] == "" || words[i + 1] == " " || char.IsLower(words[i + 1][0]))
                            {
                                continue;
                            }
    
                            if (!nameSpaces[word].Contains(words[i + 1]))
                            {
                                if(!keywords.Contains(words[i + 1]))
                                {
                                    keywords.Add(words[i + 1]);
                                }
                                
                                nameSpaces[word].Add(words[i + 1]);

                                if(!autoCompleteSource.Contains(words[i + 1]))
                                {
                                    autoCompleteSource.Add(words[i + 1]);
                                }
                               
                            }
                        }
                    }
                    //else
                    //{
                    //    if (words.Count > 1 && !nameSpaces.Any(ns => ns.Value.Contains(word)) && word.Length > 1)
                    //    {
                    //        nameSpaces.Add(word, new List<string>());

                    //        if (i < words.Count - 1)
                    //        {
                    //            if (!nameSpaces[word].Contains(word))
                    //            {
                    //                //nameSpaces[word].Add(word);
                    //            }

                    //            if (words[i + 1] == "" || words[i + 1] == " " || char.IsLower(words[i + 1][0]))
                    //            {
                    //                continue;
                    //            }

                    //            if (!nameSpaces[word].Contains(words[i + 1]))
                    //            {
                    //                nameSpaces[word].Add(words[i + 1]);

                    //                if (!keywords.Contains(words[i + 1]))
                    //                {
                    //                    keywords.Add(words[i + 1]);
                    //                }

                    //                if (!autoCompleteSource.Contains(words[i + 1]))
                    //                {
                    //                    autoCompleteSource.Add(words[i + 1]);
                    //                }

                    //            }
                    //        }
                    //    }
                    //}
                }
            }
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

            foreach(string line in protoLines)
            {
                ParseJsContentForAutoCompletion(line);
            }

            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (line.StartsWith("Known Issues:") || line == "" || line == " ")
                {
                    break;
                }

                if (!line.ToLower().StartsWith("game.") && !line.ToLower().StartsWith("context.") && !line.ToLower().StartsWith("player.") &&
                    !line.ToLower().StartsWith("system.") && !line.ToLower().StartsWith("%nuleus"))
                {
                    continue;
                }

                //string cleaned = line;

                //to complete nameSpaces
                ParseJsContentForAutoCompletion(line);
                autoCompleteSource.Add(line);

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

                if (cleaned == "" || cleaned == " " || char.IsLower(cleaned[0]))
                {
                    continue;
                }


                autoCompleteSource.Add(cleaned.TrimEnd());

                //autoCompleteSource.Add(line);
            }

            keywords.AddRange(protoLines);
            keywords.AddRange(JavaScriptKeywords);
            autoCompleteSource.AddRange(protoLines.ToArray());
            keywords.Sort();


            string fileName = handlersDirectory + "\\autoCompleteCache.txt";

            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    //string json = JsonConvert.SerializeObject(JMetaInfo, Formatting.Indented);
                    foreach(string keyword in keywords)
                    {
                        writer.WriteLine(keyword);
                    }

                    foreach(KeyValuePair<string,List<string>> words in nameSpaces)
                    {
                        writer.WriteLine(words.Key);

                        foreach (string word in words.Value)
                        {
                            writer.WriteLine(word);
                        }
                    }

                    stream.Flush();
                }
            }
        }

        private void OnFormClosed(object sender,EventArgs e)
        {
            Instance = null;
        }


        private void OnFormClosing(object sender, EventArgs e)
        {
            filesSuprVisorTimer?.Dispose();

            foreach (GameListViewItem item in gameslistViewJsItemsList)
            {
                if(item.Info.UserGameInfo == null)
                {
                    continue;
                }

                item.Info.UserGameInfo.NotifyUpdateAvailable -= RefreshHandlerUpdateState;
            }

            Http_Client.OnHubConnectionChange -= OnHubStateChange;
        }


        private void CtrlS_Shortcut(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                        e.Key == System.Windows.Input.Key.S)
            {
                e.Handled = true; 
                SavechangeButton_MouseDown(null,null);
            }
        }
    }
}
