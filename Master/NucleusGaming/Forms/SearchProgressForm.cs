using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop.Generic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{

    public class SearchProgress
    {
        public class SearchResult
        {
            public HandlerInfo HandlerInfo { get; set; }
            public string ExecutablePath { get; set; }
            public string InstallDirectory { get; set; }
        }

        public int Percent { get; set; }
        public string CurrentPath { get; set; }
        public string CurrentFile { get; set; }

        private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
        {
            Stack<string> dirs = new Stack<string>();
            dirs.Push(root);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();

                if (!Directory.Exists(currentDir) || currentDir.Length > 260)
                    continue;

                string[] files;

                try { files = Directory.GetFiles(currentDir, pattern); }
                catch { continue; }

                foreach (var file in files)
                    yield return file;

                try
                {
                    foreach (var dir in Directory.GetDirectories(currentDir))
                        dirs.Push(dir);
                }
                catch { continue; }
            }
        }


        public class SearchProgressForm : Form
        {          
            private ProgressBar progressBar1;
            private Label lblStatus;
            private Button btnCancel;

            private bool CancelRequested { get; set; } = false;
            public static SearchProgressForm Instance;

            public SearchProgressForm()
            {
                if(Instance != null)
                {
                    return;
                }

                this.Text = "Scanning for Game...";
                this.Width = 600;
                this.Height = 130;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.None;
                this.MaximizeBox = false;
                this.BackColor = Color.Black;
                this.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "other_backgrounds.jpg");
                this.TopMost = true;
                this.FormClosed += OnClosed;
                this.ShowInTaskbar = false;

                progressBar1 = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0,
                    Width = 550,
                    Height = 25,
                    Top = 20,
                    ForeColor = Color.White,
                    Left = 20
                };

                this.Controls.Add(progressBar1);

                lblStatus = new Label
                {
                    AutoSize = false,
                    Width = 550,
                    Height = 20,
                    Top = progressBar1.Bottom + 10,
                    Left = 20,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    Text = "Starting..."
                };
                this.Controls.Add(lblStatus);

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Width = 100,
                    Height = 30,
                    Top = lblStatus.Bottom + 10,
                    BackColor = Color.Transparent,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Left = 20
                };

                btnCancel.FlatAppearance.BorderSize = 1;
                btnCancel.FlatAppearance.BorderColor = Color.White;
                btnCancel.FlatAppearance.MouseOverBackColor = Color.Green;
                btnCancel.Click += (s, e) => { CancelRequested = true; };
                this.Controls.Add(btnCancel);

                Instance = this;
            }

            private IEnumerable<SearchResult> SearchGames(
    IEnumerable<HandlerInfo> games,
    List<string> searchDirs,
    IProgress<SearchProgress> progress = null,
    Func<bool> cancelCheck = null)
            {
                var gameList = games.ToList();

                //exeName -> List<HandlerInfo>
                var exeLookup = new Dictionary<string, List<HandlerInfo>>(StringComparer.OrdinalIgnoreCase);

                foreach (var handler in gameList)
                {
                    var exeNames = handler.GenericGameInfo.ExecutableNames
                                  ?? new[] { handler.GenericGameInfo.ExecutableName };

                    foreach (var exe in exeNames)
                    {
                        if (!exeLookup.TryGetValue(exe, out var list))
                        {
                            list = new List<HandlerInfo>();
                            exeLookup[exe] = list;
                        }

                        list.Add(handler);
                    }
                }

                int totalRoots = searchDirs.Count;
                int rootIndex = 0;

                foreach (var rootPath in searchDirs)
                {
                    if (cancelCheck?.Invoke() == true)
                        yield break;

                    rootIndex++;

                    if (!Directory.Exists(rootPath))
                        continue;

                    List<string> gameDirs;
                    try { gameDirs = Directory.GetDirectories(rootPath).ToList(); }
                    catch { continue; }

                    for (int j = 0; j < gameDirs.Count; j++)
                    {
                        if (cancelCheck?.Invoke() == true)
                            yield break;

                        string gameDir = gameDirs[j];

                        int percent = (int)(((rootIndex - 1) + (j / (double)gameDirs.Count)) / totalRoots * 100);
                        progress?.Report(new SearchProgress
                        {
                            Percent = percent,
                            CurrentPath = gameDir
                        });

                        //Scan once for executables only
                        foreach (var filePath in SafeEnumerateFiles(gameDir, "*.exe"))
                        {
                            if (cancelCheck?.Invoke() == true)
                                yield break;

                            string fileName = Path.GetFileName(filePath);

                            if (!exeLookup.TryGetValue(fileName, out var matchedHandlers))
                                continue;

                            string exeDir = Path.GetDirectoryName(filePath);

                            foreach (var game in matchedHandlers)
                            {
                                //Binaries Folder check
                                if (!string.IsNullOrEmpty(game.GenericGameInfo.BinariesFolder))
                                {
                                    if (!exeDir.EndsWith(game.GenericGameInfo.BinariesFolder, StringComparison.OrdinalIgnoreCase))
                                        continue;
                                }

                                //Context check
                                string[] context = game.GenericGameInfo.ExecutableContext;
                                bool contextFail = false;

                                if (context != null)
                                {
                                    foreach (var ctx in context)
                                    {
                                        if (ctx.StartsWith("!"))
                                        {
                                            string negPath = Path.Combine(exeDir, ctx.TrimStart('!'));
                                            if (File.Exists(negPath) || Directory.Exists(negPath))
                                            {
                                                contextFail = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            string reqPath = Path.Combine(exeDir, ctx);
                                            if (!File.Exists(reqPath) && !Directory.Exists(reqPath))
                                            {
                                                contextFail = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (contextFail)
                                    continue;

                                progress?.Report(new SearchProgress
                                {
                                    Percent = percent,
                                    CurrentPath = gameDir,
                                    CurrentFile = filePath
                                });

                                yield return new SearchResult
                                {
                                    HandlerInfo = game,
                                    ExecutablePath = filePath,
                                    InstallDirectory = exeDir
                                };
                            }
                        }
                    }
                }

                progress?.Report(new SearchProgress
                {
                    Percent = 100,
                    CurrentPath = "Finished"
                });
            }


            public Task<List<SearchResult>> StartSearchAsync(IEnumerable<HandlerInfo> games, List<string> searchDirs)
            {
                var tcs = new TaskCompletionSource<List<SearchResult>>();

                var progress = new Progress<SearchProgress>(p =>
                {
                    UpdateProgress(p.Percent, p.CurrentPath, p.CurrentFile);
                });

                Show();
                Refresh();
                Application.DoEvents();

                Task.Run(() =>
                {
                    var results = new List<SearchResult>();

                    foreach (var result in SearchGames(games, searchDirs, progress,() => CancelRequested))
                    {
                        results.Add(result);
                    }

                    Invoke((Action)(() =>
                    {
                        Close();
                        tcs.SetResult(results);
                    }));

                });

                return tcs.Task;
            }

            public Task<SearchResult> StartSearchAsync(HandlerInfo game, List<string> searchDirs)
            {
                var tcs = new TaskCompletionSource<SearchResult>();

                var progress = new Progress<SearchProgress>(p =>
                {
                    UpdateProgress(p.Percent, p.CurrentPath, p.CurrentFile);
                });

                Show();
                Refresh();
                Application.DoEvents();

                Task.Run(() =>
                {
                    SearchResult result = null;

                    foreach (var r in SearchGames(new[] {game}, searchDirs, progress,() => CancelRequested))
                    {
                        result = r;
                        break; // stop on first match
                    }

                    Invoke((Action)(() =>
                    {
                        Close();
                        tcs.SetResult(result);
                    }));

                });

                return tcs.Task;
            }

            private void UpdateProgress(int percent, string dir, string file = null)
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateProgress(percent, dir, file)));
                    return;
                }

                progressBar1.Value = Math.Min(percent, 100);
                lblStatus.Text = "Scanning: " + dir;
            }

            private void OnClosed(object sender,EventArgs e)
            {
                Instance = null;
            }
        }
    }
}
