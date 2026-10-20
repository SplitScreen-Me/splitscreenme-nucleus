using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Http_Client;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Nucleus.Coop.Tools
{
    public static class GameAssets
    {
        public static void LoadGameAssets()
        {
            var gameGUID = Core_Interface.Current_UserGameInfo?.GameGuid;

            if (gameGUID == null)
            {
                return;
            }

            Image coverBmp = null;

            string coverDir = Path.Combine(Application.StartupPath, $"gui\\covers");
            string backgroundsDir = Path.Combine(Application.StartupPath, $"gui\\screenshots");

            if (Directory.Exists(coverDir))
            {
                var coversPath = Directory.GetFiles(Path.Combine(Application.StartupPath, $"gui\\covers")).Where(s => s.Contains(gameGUID) && (
                    s.EndsWith(".png") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".jpg") ||
                    s.EndsWith(".bmp") ||
                    s.EndsWith(".gif"))
                    ).ToList();

                if (coversPath.Count > 0)
                {
                    coverBmp = new Bitmap(coversPath[0]);
                }
                else
                {
                    coverBmp = ImageCache.GetImage(Globals.ThemeFolder + "no_cover.png");
                }

                UI_Interface.Cover.BackgroundImage = coverBmp;
            }
            else
            {
                coverBmp = ImageCache.GetImage(Globals.ThemeFolder + "no_cover.png");
            }

            if (!Theme_Settings.UseBackgroundImage)
            {
                return;
            }
                ///Apply screenshots randomly
            string gameBackgroundDir = Path.Combine(backgroundsDir, gameGUID);

            if (Directory.Exists(gameBackgroundDir))
            {
                var imgsPath = Directory.GetFiles(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameGUID}")).Where(s =>
                s.EndsWith(".png") ||
                s.EndsWith(".jpeg") ||
                s.EndsWith(".jpg") ||
                s.EndsWith(".bmp") ||
                s.EndsWith(".gif")
                ).ToList();

                if (imgsPath.Count > 0)
                {
                    Random rNum = new Random();
                    int RandomIndex = rNum.Next(0, imgsPath.Count);

                    Bitmap backgroundImg = UI_Graphics.ApplyBlur(new Bitmap(imgsPath[RandomIndex]));
                    UI_Graphics.BackgroundImg = backgroundImg;
                }
                else
                {
                    Bitmap def = UI_Graphics.ApplyBlur(new Bitmap((Bitmap)UI_Graphics.DefaultBackground.Clone()));
                    UI_Graphics.BackgroundImg = def;
                    UI_Graphics.GameBorderGradientTop = Theme_Settings.DefaultBorderGradientColor;
                    UI_Graphics.GameBorderGradientBottom = Theme_Settings.DefaultBorderGradientColor;
                }
            }
            else
            {
                Bitmap def = UI_Graphics.ApplyBlur(new Bitmap((Bitmap)UI_Graphics.DefaultBackground.Clone()));
                UI_Graphics.BackgroundImg = def;
                UI_Graphics.GameBorderGradientTop = Theme_Settings.DefaultBorderGradientColor;
                UI_Graphics.GameBorderGradientBottom = Theme_Settings.DefaultBorderGradientColor;
            }

            Generic_Functions.RefreshAll();
        }

        public static void CleanGamesAssets(UserGameInfo userGameInfo,bool askUser)
        {
            bool foundScreenshotsDir = false;

            if (Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{userGameInfo.GameGuid}")))
            {
                foundScreenshotsDir = true;
            }

           
            string coversDir = Path.Combine(Application.StartupPath, $"gui\\covers");
            var coverFiles = new List<string>();

            if (Directory.Exists(coversDir))
            {
                coverFiles = Directory.EnumerateFiles(coversDir, "*", SearchOption.TopDirectoryOnly).Where(f => Path.GetFileNameWithoutExtension(f)
                                     .Equals(userGameInfo.GameGuid, StringComparison.OrdinalIgnoreCase)).ToList(); 
            }

            if (askUser && (coverFiles.Count > 0 || foundScreenshotsDir))
            {
                DialogResult assetsDialogResult =
                 MessageBox.Show($"Do you want to delete {userGameInfo.HandlerInfo.GenericGameInfo.GameName} cover and backgrounds too?", "Delete game cover and backgrounds?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (assetsDialogResult != DialogResult.Yes)
                {
                    return;
                }
            }

            if(foundScreenshotsDir)
            {
                try
                {
                    Directory.Delete(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{userGameInfo.GameGuid}"), true);
                }
                catch
                {
                }
            }

            if (coverFiles.Count > 0)
            {
                foreach (var file in coverFiles)
                {
                    if (Core_Interface.Current_UserGameInfo?.GameGuid == userGameInfo.GameGuid)
                    {
                        var img = UI_Interface.Cover.BackgroundImage;
                        UI_Interface.Cover.BackgroundImage = null;
                        img?.Dispose();
                    }

                    File.Delete(file);
                }
            }

        }

        public class AssetsDownloader
        {
            private int maxScreenshotsToDownload;

            public async Task DownloadAllGamesAssets(GameControl currentControl)
            {
                bool proceed = await Http_Client.CheckInternetAccessAsync(5000);

                if (!proceed)
                {
                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|No internet access or the connection is too slow.@*");
                    return;
                }

                List<UserGameInfo> games = GameManager.Instance.User.Games;

                for (int i = 0; i < games.Count; i++)
                {
                    UserGameInfo userGameInfo = games[i];

                    string id = userGameInfo?.HandlerInfo.GenericGameInfo?.HandlerId;

                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    Handler handler = await HubCache.SearchById(id);

                    if (handler == null)
                    {
                        continue;
                    }

                    Globals.MainOSD.Show(80000, $"Downloading Assets For *@{OSDHighlightColors.Regular}|{userGameInfo.GameGuid}@*");

                    string coverUri = $@"https://images.igdb.com/igdb/image/upload/t_cover_big/{handler.GameCover}.jpg";
                    string screenshotsUri = await HubCache.GetScreenshotsUri(handler.Id);

                    await DownloadCovers(coverUri, userGameInfo.GameGuid);
                    await DownloadScreenshots(screenshotsUri, userGameInfo.GameGuid);

                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Confirm}|Download Completed!@*");
                }
            }


            public async Task DownloadGamesAssets(List<GenericGameInfo> genericGameInfos)
            {
                bool proceed = await Http_Client.CheckInternetAccessAsync(5000);

                if (!proceed)
                {
                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|No internet access or the connection is too slow.@*");
                    return;
                }

                for (int i = 0; i < genericGameInfos.Count; i++)
                {
                    GenericGameInfo genericGameInfo = genericGameInfos[i];

                    string id = genericGameInfo?.HandlerId;

                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    Handler handler = await HubCache.SearchById(id);

                    if (handler == null)
                    {
                        continue;
                    }

                    Globals.MainOSD.Show(80000, $"Downloading Assets For *@{OSDHighlightColors.Regular}|{genericGameInfo.GameName}@*");

                    string coverUri = $@"https://images.igdb.com/igdb/image/upload/t_cover_big/{handler.GameCover}.jpg";
                    string screenshotsUri = await HubCache.GetScreenshotsUri(handler.Id);

                    await DownloadCovers(coverUri, genericGameInfo.GUID);
                    await DownloadScreenshots(screenshotsUri, genericGameInfo.GUID);

                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Confirm}|Download Completed!@*");
                }
            }

            public async Task DownloadGameAssets(UserGameInfo userGameInfo, GameControl currentControl)
            {
                bool proceed = await Http_Client.CheckInternetAccessAsync(5000);

                if (!proceed)
                {
                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|No internet access or connection to slow.@*");
                    return;
                }

                string id = userGameInfo?.HandlerInfo?.GenericGameInfo?.HandlerId;

                if (string.IsNullOrEmpty(id))
                {
                    return;
                }

                Handler handler = await HubCache.SearchById(id);

                if (handler != null)
                {
                    Globals.MainOSD.Show(80000, $"Downloading Assets For *@{OSDHighlightColors.Regular}|{userGameInfo.GameGuid}@*");
                    string coverUri = $@"https://images.igdb.com/igdb/image/upload/t_cover_big/{handler.GameCover}.jpg";
                    string screenshotsUri = await HubCache.GetScreenshotsUri(handler.Id);

                    await DownloadCovers(coverUri, userGameInfo.GameGuid);
                    await DownloadScreenshots(screenshotsUri, userGameInfo.GameGuid);

                    Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Confirm}|Download Completed!@*");
                }              
            }

            public async Task DownloadCovers(string url, string gameGuid)
            {
                if (!Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\covers")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, $"gui\\covers"));
                }

                try
                {
                    string outputFile = Path.Combine(Application.StartupPath, $"gui\\covers\\{gameGuid}.jpeg");

                    if (!File.Exists(outputFile))
                    {
                        await Http_Client.DownloadImageAsync(url, outputFile, 15000);
                    }
                }
                catch
                {
                    Globals.MainOSD.Show(2000, $"Download Aborted: *@{OSDHighlightColors.Warning}|Timeout@*");
                }
            }

            public async Task DownloadScreenshots(string json, string gameName)
            {
                if (json == null)
                {
                    return;
                }

                if (!Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\screenshots")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, $"gui\\screenshots"));
                }

                try
                {
                    JObject jsonData = JsonConvert.DeserializeObject(json) as JObject;
                    JArray array = jsonData["screenshots"] as JArray;

                    if (array.Count < 5)// <= if there is less than 5 screenshots available in the igdb's database
                    {
                        maxScreenshotsToDownload = array.Count;
                    }
                    else
                    {
                        maxScreenshotsToDownload = 5;
                    }

                    if (!Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameName}")))
                    {
                        Directory.CreateDirectory((Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameName}")));
                    }

                    for (int i = 0; i < maxScreenshotsToDownload; i++)// <= we don't want to download all screenshots available in the igdb's database
                    {
                        string outputFile = Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameName}\\{i}_{gameName}.jpeg");

                        if (!File.Exists(outputFile))
                        {
                            if (!File.Exists(outputFile))
                            {
                                string url = $"https:{array[i]["url"]}".Replace("t_thumb", "t_original");

                                await Http_Client.DownloadImageAsync(url, outputFile, 15000);
                            }
                        }
                    }
                }
                catch
                {
                    Globals.MainOSD.Show(2000, $"Download Aborted: *@{OSDHighlightColors.Warning}|Timeout@*");
                }
            }

            public void SaveDescriptions(string desc, string gameGuid)
            {
                if (!Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\descriptions")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.StartupPath, $"gui\\descriptions"));
                }

                if (!File.Exists(Path.Combine(Application.StartupPath, $"gui\\descriptions\\{gameGuid}.txt")))
                {
                    using (FileStream stream = new FileStream(Path.Combine(Application.StartupPath, $"gui\\descriptions\\{gameGuid}.txt"), FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(desc);
                            stream.Flush();
                            writer.Dispose();
                        }

                        stream.Dispose();
                    }
                }
            }


            public static bool ShouldPromptUserForAssestsDownload(UserGameInfo userGameInfo)
            {
                string backgroundsDir = Path.Combine(Application.StartupPath, $"gui\\screenshots");
                string gameBackgroundDir = Path.Combine(backgroundsDir, userGameInfo.GameGuid);

                if (Directory.Exists(gameBackgroundDir))
                {
                    var imgsPath = Directory.GetFiles(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{userGameInfo.GameGuid}")).Where(s =>
                    s.EndsWith(".png") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".jpg") ||
                    s.EndsWith(".bmp") ||
                    s.EndsWith(".gif")
                    ).ToList();

                    if (imgsPath.Count > 0)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return true;
                }
            }
        }
    }

}
