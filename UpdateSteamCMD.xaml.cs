using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using WpfApp3.Classes;
using System.Xml.Linq;
using Newtonsoft.Json;
using SteamAppInfoParser;
using Octokit;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for UpdateSteamCMD.xaml
    /// </summary>
    public partial class UpdateSteamCMD : Window
    {
        private bool unlisted = false;
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        public static List<Game> Items = new List<Game>();



        public UpdateSteamCMD()
        {
            Directory.CreateDirectory(path);
            string GamesXml = path + "\\Games.xml";
            /*if (File.Exists(GamesXml))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(GamesXml);

                XmlNodeList gameNodes = doc.SelectNodes("/games/game");

                foreach (XmlNode gameNode in gameNodes)
                {
                    
                    Game game = new Game();

                    game.Title = gameNode.SelectSingleNode("title").InnerText.Trim();
                    game.Path = gameNode.SelectSingleNode("path").InnerText;
                    game.Path_Directory = gameNode.SelectSingleNode("path_directory").InnerText;
                    game.Type = gameNode.SelectSingleNode("type").InnerText;
                    game.Background = gameNode.SelectSingleNode("background").InnerText;
                    game.Logo = gameNode.SelectSingleNode("logo").InnerText;
                    game.Date_Added = long.Parse(gameNode.SelectSingleNode("date").InnerText);
                    if (gameNode.SelectSingleNode("last_played").InnerText == "")
                    {
                        game.Last_Played = 0;
                    }
                    else
                        game.Last_Played = long.Parse(gameNode.SelectSingleNode("last_played").InnerText);
                    game.SteamAppid = int.Parse(gameNode.SelectSingleNode("steamappid").InnerText);
                    game.Installed = true;
                    games.Add(game);
                }
            }*/


            InitializeComponent();
            AsyncUpdateSteamCMD();
        }

        private async void AsyncUpdateSteamCMD()
        {
            string steamcmdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "steamcmd.exe");

            if (File.Exists(steamcmdPath) == false)
            {
                WebClient client = new WebClient();
                string downloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                client.DownloadFile(downloadUrl, "steamcmd.zip");
                client.Dispose();
                ZipFile.ExtractToDirectory("steamcmd.zip", AppDomain.CurrentDomain.BaseDirectory + "\\Steam");
            }
            string da = "D:\\Codeblocks\\coduri\\Visual Studio\\WpfApp3\\bin\\Debug\\net7.0-windows\\Steam\\RTconsole.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = da,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = steamcmdPath + " +login rares478 +quit"
            };

            Process process = new Process();
            process.StartInfo = startInfo;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }


        bool uptodate = false;
        bool done_games= false;

        private async void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string output = e.Data;
            if (output != null)
            {
                if(output.Contains("to Steam Public...OK"))
                    uptodate = true;


                if (uptodate == false)
                {
                    // Check for update message
                    if (output.Contains("Downloading update"))
                    {
                        // Update the progress bar
                        Regex regex = new Regex(@"^\[\s*(\d{1,3})%\]");
                        Match match = regex.Match(output);
                        if (match.Success)
                        {
                            int percentage = int.Parse(match.Groups[1].Value);
                            this.Dispatcher.Invoke(() =>
                            {
                                LoadingBar.Value = percentage;
                            });
                        }
                    }
                    else if (output.Contains("Download complete"))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            LoadingBar.Value = 100;
                        });
                        uptodate = true;
                    }
                }
                else
                {
                    if (done_games == false)
                    {
                        if(await Process_Games())
                        {
                            done_games = true;
                            this.Dispatcher.Invoke(() =>
                            {
                                LoadingBar.Value = 100;
                            });
                        }
                    }
                }
            }
        }


        private async Task<bool> Process_Games()
        {
            string appinfoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam\\appcache", "appinfo.vdf");
            var appInfo = new AppInfo();
            appInfo.Read(appinfoPath);

            foreach (var app in appInfo.Apps)
            {
                Game game = new Game();

                var common = app.Data.Children.FirstOrDefault(c => c.Name == "common");
                if (common != null)
                {
                    var name = common["name"].ToString().Trim();
                    var type = common["type"].ToString().Trim();
                    var appid = Convert.ToInt32(common["gameid"].ToString().Trim());
                    if (appid == 61800 || appid == 61810 || appid== 61820 || appid == 61830)
                    {
                        continue;
                    }

                    if (type == "Game" || type == "game" || type == "Application")
                    {
                        game.Title = name;
                        game.SteamAppid = appid;

                        if (type == "Application")
                            game.Type = "App";
                        else game.Type = "Game";

                        var extended = app.Data.Children.FirstOrDefault(c => c.Name == "extended");

                        if (extended != null)
                        {
                            if (extended["listofdlc"] != null)
                            {
                                var list = extended["listofdlc"].ToString();
                                string[] dlcs = list.Split(',');

                                List<DLC> DLCs = new List<DLC>();
                                foreach (var dlc in dlcs)
                                {
                                    int id;
                                    int.TryParse(dlc, out id);
                                    DLC dlc2 = new DLC { Id = id, Name = "" };
                                    DLCs.Add(dlc2);
                                }

                                game.DLCs = DLCs;
                            }
                        }


                        var depots = app.Data.Children.FirstOrDefault(c => c.Name == "depots");

                        double size = 0;

                        if (depots != null)
                        {
                            foreach (var depot in depots.Children)
                            {
                                if (depot.Name == "1659421")
                                {
                                    Console.WriteLine("da");
                                }

                                if (depot.Children.Count() > 0)
                                {
                                    if (depot.Children.FirstOrDefault(a => a.Name == "depotfromapp") != default || depot.Children.FirstOrDefault(a => a.Name == "sharedinstall") != default)
                                        continue;

                                    if (depot.Children.FirstOrDefault(a => a.Name == "config") != default)
                                    {
                                        var config = depot.Children.FirstOrDefault(a => a.Name == "config");

                                        if (config.Children.Count() > 0)
                                        {
                                            if (config.Children.FirstOrDefault(a => a.Name == "oslist") != default)
                                            {
                                                if (config["oslist"].ToString() == "macos" || config["oslist"].ToString() == "linux" || config["oslist"].ToString() == "linux,macos" || config["oslist"].ToString() == "macos,linux")
                                                    continue;
                                            }
                                            if (config.Children.FirstOrDefault(a => a.Name == "language") != default)
                                            {
                                                if (config["language"].ToString() != "english" && config["language"].ToString() != "")
                                                    continue;
                                            }
                                        }
                                    }

                                    if (depot.Children.FirstOrDefault(a => a.Name == "maxsize") != default)
                                    {
                                        size = size + Convert.ToDouble(depot["maxsize"].ToString());
                                    }
                                    else if (depot.Children.FirstOrDefault(a => a.Name == "manifests") != default)
                                    {
                                        var manifest = depot.Children.FirstOrDefault(a => a.Name == "manifests");

                                        var publicDep = manifest.Children.FirstOrDefault(a => a.Name == "public");
                                        if (publicDep != null)
                                        {
                                            var sizeDep = publicDep.Children.FirstOrDefault(a => a.Name == "size");
                                            if (sizeDep != null)
                                            {
                                                size = size + Convert.ToDouble(depot["manifests"]["public"]["size"].ToString());
                                            }
                                        }
                                    }
                                }

                            }
                        }

                        game.Size = size;

                        Items.Add(game);



                    }

                    else if (type == "DLC")
                    {
                        DLC dlc = new DLC();
                        dlc.Id = appid;
                        dlc.Name = name;

                        int parentid = Convert.ToInt32(common["parent"].ToString());

                        Game game1 = Items.FirstOrDefault(a => a.SteamAppid == parentid);
                        if (game1 == null)
                        {
                            game1 = new Game { SteamAppid = parentid, DLCs = new List<DLC>() };
                            game1.DLCs.Add(dlc);
                            Items.Add(game1);
                        }

                        DLC existingDLC = game1.DLCs.FirstOrDefault(a => a.Id == dlc.Id);
                        if (existingDLC != default)
                        {
                            existingDLC.Name = name;

                        }
                        else
                        {
                            game1.DLCs.Add(dlc);
                        }
                    }
                }

            }

            return true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
