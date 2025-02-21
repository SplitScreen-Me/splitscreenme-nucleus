using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Gaming.UI
{
    public static class Theme_Settings
    {
        private static string ThemeFolder => Globals.ThemeFolder;
        private static IniFile ThemeConfigFile => Globals.ThemeConfigFile;

        public static Cursor Default_Cursor => GetCursorDefaut();
        private static Cursor default_Cursor;
        public static  PrivateFontCollection modernFont;

        public static float FontSize = float.Parse(Globals.ThemeConfigFile.IniReadValue("Font", "MainFontSize"));

        public static string CustomFont  => Globals.ThemeConfigFile.IniReadValue("Font", "FontFamily");
        //private static FontFamily customFont;
        //public static FontFamily CustomFont
        //{
        //    get
        //    {
        //        if (customFont == null)
        //        {
        //            modernFont = new PrivateFontCollection();

        //            var fontFile = Directory.GetFiles(ThemeFolder, "*.*", SearchOption.AllDirectories)
        //                 .Where(s => s.EndsWith(".otf") || s.EndsWith(".ttf")).ToList();

        //            if (fontFile.Count() == 0)
        //            {
        //                return null;
        //            }

        //            modernFont.AddFontFile(fontFile[0]);
        //            customFont = modernFont.Families[0];
        //            //label.Font = new Font(modernFont.Families[0], 40);`
        //        }

        //        return customFont;
        //    }

        //}

        private static Cursor GetCursorDefaut()
        {
            if(default_Cursor == null)
            {
                Bitmap bmp = new Bitmap(ThemeFolder + "cursor.ico");

                bmp = new Bitmap(bmp, new Size(Cursor.Current.Size.Width, Cursor.Current.Size.Height));
                default_Cursor = new Cursor(bmp.GetHicon());
                bmp.Dispose();
            }

            return default_Cursor;
        }

        public static Cursor Hand_Cursor => GetCursorHand();
        private static Cursor hand_Cursor;     

        private static Cursor GetCursorHand()
        {
            if (hand_Cursor == null)
            {
                Bitmap bmp = new Bitmap(ThemeFolder + "cursor_hand.ico");
                bmp = new Bitmap(bmp, new Size(Cursor.Current.Size.Width, Cursor.Current.Size.Height));
                hand_Cursor = new Cursor(bmp.GetHicon());
                bmp.Dispose();
            }

            return hand_Cursor;
        }

        private static Color selectedBackColor;
        public static Color SelectedBackColor
        {
            get
            {
                if (selectedBackColor.IsEmpty)
                {
                    selectedBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "Selection").Split(','));
                }

                return selectedBackColor;
            }
        }

        private static Color handlerNoteForeColor;
        public static Color HandlerNoteForeColor
        {
            get
            {
                if (handlerNoteForeColor.IsEmpty)
                {
                    handlerNoteForeColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "HandlerNoteFont").Split(','));
                }

                return handlerNoteForeColor;
            }
        }

        private static Color handlerNoteTitleForeColor;
        public static Color HandlerNoteTitleForeColor
        {
            get
            {
                if (handlerNoteTitleForeColor.IsEmpty)
                {
                    handlerNoteTitleForeColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "HandlerNoteTitleFont").Split(','));
                }

                return handlerNoteTitleForeColor;
            }
        }      

        private static Color mouseOverBackColor;
        public static Color MouseOverBackColor
        {
            get
            {
                if (mouseOverBackColor.IsEmpty)
                {
                    mouseOverBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "MouseOver").Split(','));
                }

                return mouseOverBackColor;
            }
        }

        private static Color windowPanelBackColor;
        public static Color WindowPanelBackColor
        {
            get
            {
                if (windowPanelBackColor.IsEmpty)
                {
                    windowPanelBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "MainButtonFrameBackground").Split(','));
                }

                return windowPanelBackColor;
            }
        }

        private static Color infoPanelBackColor;
        public static Color InfoPanelBackColor
        {
            get
            {
                if (infoPanelBackColor.IsEmpty)
                {
                    infoPanelBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "RightFrameBackground").Split(','));
                }

                return infoPanelBackColor;
            }
        }

        private static Color gameListBackColor;
        public static Color GameListBackColor
        {
            get
            {
                if (gameListBackColor.IsEmpty)
                {
                    gameListBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "GameListBackground").Split(','));
                }

                return gameListBackColor;
            }
        }

       

        private static Color backgroundGradientColor;
        public static Color BackgroundGradientColor
        {
            get
            {
                if (backgroundGradientColor.IsEmpty)
                {
                    backgroundGradientColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "BackgroundGradient").Split(','));
                }

                return backgroundGradientColor;
            }
        }

        private static Color defaultBorderGradientColor;
        public static Color DefaultBorderGradientColor
        {
            get
            {
                if (defaultBorderGradientColor.IsEmpty)
                {
                    defaultBorderGradientColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "WindowBorderGradient").Split(','));
                }

                return defaultBorderGradientColor;
            }
        }

        private static Color mainWindowBackColor;
        public static Color MainWindowBackColor
        {
            get
            {
                if (mainWindowBackColor.IsEmpty)
                {
                    mainWindowBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "WindowBorder").Split(','));
                }

                return mainWindowBackColor;
            }
        }

        

        private static Color controlsForeColor;
        public static Color ControlsForeColor
        {
            get
            {
                if (controlsForeColor.IsEmpty)
                {
                    controlsForeColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "Font").Split(','));
                }

                return controlsForeColor;
            }
        }

        private static Color handlerNoteBackColor;
        public static Color HandlerNoteBackColor
        {
            get
            {
                if (handlerNoteBackColor.IsEmpty)
                {
                    handlerNoteBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "HandlerNoteBackground").Split(','));
                }

                return handlerNoteBackColor;
            }
        }

        private static Color buttonsBackColor;
        public static Color ButtonsBackColor
        {
            get
            {
                if (buttonsBackColor.IsEmpty)
                {
                    buttonsBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "ButtonsBackground").Split(','));
                }

                return buttonsBackColor;
            }
        }

        private static Color buttonsBorderColor;
        public static Color ButtonsBorderColor
        {
            get
            {
                if (buttonsBorderColor.IsEmpty)
                {
                    buttonsBorderColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "ButtonsBorder").Split(','));
                }

                return buttonsBorderColor;
            }
        }

        private static Color gameOptionMenuBackColor;
        public static Color GameOptionMenuBackColor
        {
            get
            {
                if (gameOptionMenuBackColor.IsEmpty)
                {
                    gameOptionMenuBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "MenuStripBack").Split(','));
                }

                return gameOptionMenuBackColor;
            }      
        }

        private static Color gameOptionMenuForeColor;
        public static Color GameOptionMenuForeColor
        {
            get
            {
                if (gameOptionMenuForeColor.IsEmpty)
                {
                    gameOptionMenuForeColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "MenuStripFont").Split(','));
                }

                return gameOptionMenuForeColor;
            }
        }


        private static Color setupScreenBackColor;
        public static Color SetupScreenBackColor
        {
            get
            {
                if (setupScreenBackColor.IsEmpty)
                {
                    setupScreenBackColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "SetupScreenBackground").Split(','));
                }

                return setupScreenBackColor;
            }
        }

        private static Color setupScreenForeColor;
        public static Color SetupScreenForeColor
        {
            get
            {
                if (setupScreenForeColor.IsEmpty)
                {
                    setupScreenForeColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "SetupScreenFont").Split(','));
                }

                return setupScreenForeColor;
            }
        }

        private static Color setupScreenUIScreenColor;
        public static Color SetupScreenUIScreenColor
        {
            get
            {
                if (setupScreenUIScreenColor.IsEmpty)
                {
                    setupScreenUIScreenColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "SetupScreenBorder").Split(','));
                }

                return setupScreenUIScreenColor;
            }
        }

        private static Color setupScreenPlayerScreenColor;
        public static Color SetupScreenPlayerScreenColor
        {
            get
            {
                if (setupScreenPlayerScreenColor.IsEmpty)
                {
                    setupScreenPlayerScreenColor = ParseColor(ThemeConfigFile.IniReadValue("Colors", "SetupScreenPlayerBorder").Split(','));
                }

                return setupScreenPlayerScreenColor;
            }
        }


        public static bool ControllerIdentification => bool.Parse(ThemeConfigFile.IniReadValue("Misc", "ControllerIdentificationOn"));

        public static bool UseSetupScreenBorder => bool.Parse(ThemeConfigFile.IniReadValue("Misc", "UseSetupScreenBorder"));

        public static bool UseLayoutSelectionBorder => bool.Parse(ThemeConfigFile.IniReadValue("Misc", "UseLayoutSelectionBorder"));

        public static bool UseSetupScreenImage => bool.Parse(ThemeConfigFile.IniReadValue("Misc", "UseSetupScreenImage"));

        private static Color ParseColor(string[] colorArray)
        {
            if (colorArray.Length == 4)
            {
                return Color.FromArgb(int.Parse(colorArray[0]), int.Parse(colorArray[1]), int.Parse(colorArray[2]), int.Parse(colorArray[3]));
            }
            else if (colorArray.Length == 3)
            {
                return Color.FromArgb(255, int.Parse(colorArray[0]), int.Parse(colorArray[1]), int.Parse(colorArray[2]));
            }
            else
            {
                return Color.Red;
            }
        }
    }
}
