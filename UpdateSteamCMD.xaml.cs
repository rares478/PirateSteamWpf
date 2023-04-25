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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for UpdateSteamCMD.xaml
    /// </summary>
    public partial class UpdateSteamCMD : Window
    {
        private bool unlisted = false;
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        public static List<Game> games = new List<Game>();


        public UpdateSteamCMD()
        {
            Directory.CreateDirectory(path);
            string GamesXml = path + "\\Games.xml";
            if (File.Exists(GamesXml))
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
            }

            string settings_xml = path + "\\settings.xml";
            if (File.Exists(settings_xml) == false)
            {
                File.Create(settings_xml).Close();
                XmlDocument doc = new XmlDocument();

                XmlElement root = doc.CreateElement("Application");
                doc.AppendChild(root);

                XmlElement crackElement = doc.CreateElement("Crack");
                crackElement.InnerText = "Goldberg";
                root.AppendChild(crackElement);

                XmlElement startupElement = doc.CreateElement("Startup");
                startupElement.InnerText = "No";
                root.AppendChild(startupElement);

                XmlElement unlistedElement = doc.CreateElement("Unlisted");
                unlistedElement.InnerText = "false";
                root.AppendChild(unlistedElement);

                doc.Save(settings_xml);
            }


            XmlDocument docsettings = new XmlDocument();
            docsettings.Load(settings_xml);
            XmlNodeList UnlistedElements = docsettings.GetElementsByTagName("Unlisted");
            foreach (XmlNode UnlistedElement in UnlistedElements)
            {
                if (UnlistedElement.InnerText == "true")
                {
                    if (File.Exists(path + "\\Games_unlisted.xml") == false)
                    {
                        unlisted = false;
                    }
                    else
                    { 
                        unlisted = true;
                        XmlDocument unlistedGamesDoc = new XmlDocument();
                        docsettings.Load(path + "\\Games_unlisted.xml");
                        XmlNodeList gameNodes = docsettings.SelectNodes("/games/game");
                        foreach (XmlNode gameNode in gameNodes)
                        {
                            XmlNode nameNode = gameNode.SelectSingleNode("title");
                            string name = nameNode.InnerText.Trim();


                            XmlNode appidNode = gameNode.SelectSingleNode("steamappid");
                            int appid = Convert.ToInt32(appidNode.InnerText.Trim());

                            Games_unlisted.Add(name, appid);
                        }
                    }
                }
            }

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
        bool done_games = false;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
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
                        WebRequest request2 = WebRequest.Create("http://api.steampowered.com/ISteamApps/GetAppList/v0001");
                        HttpWebResponse responseName = (HttpWebResponse)request2.GetResponse();
                        Task.Delay(100);
                        if (responseName.StatusDescription == "OK")
                        {
                            Stream datastream2 = responseName.GetResponseStream();
                            StreamReader reader2 = new StreamReader(datastream2);
                            string response3 = reader2.ReadToEnd();

                            JObject data = JObject.Parse(response3);
                            JArray apps = data["applist"]["apps"]["app"] as JArray;
                            foreach (JObject app in apps)
                            {
                                string name = app["name"].ToString();
                                int appId = Convert.ToInt32(app["appid"].ToString());
                                if (!Games.ContainsKey(name))
                                {
                                    Games.Add(name, appId);
                                }
                            }
                            done_games = true;
                            Process_Games();
                            
                        }
                    }
                }
            }
        }

        private async Task Process_Game(int appid)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "D:\\Codeblocks\\coduri\\Visual Studio\\WpfApp3\\bin\\Debug\\net7.0-windows\\Steam\\script_info.bat",
                WorkingDirectory = "D:\\Codeblocks\\coduri\\Visual Studio\\WpfApp3\\bin\\Debug\\net7.0-windows\\Steam",
                CreateNoWindow = true,
                Arguments = appid.ToString()
            };

            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();

            while ((Process.GetProcessesByName("cmd").Length > 0) == true)
            {

            }

            string info_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "app_info.txt");
            using (StreamReader sr = new StreamReader(info_path))
            {
                string fileContents = sr.ReadToEnd();
                string pattern = "\"name\"\\s+\"([^\"]+)\"";
                Match match = Regex.Match(fileContents, pattern);

                if (match.Success)
                {
                    string nameValue = match.Groups[1].Value;
                    if (Games_owned.ContainsKey(nameValue) == false)
                    {
                        Games_owned.Add(nameValue, appid);
                        this.Dispatcher.Invoke(() =>
                        {
                            tb_un.Text = (Convert.ToInt32(tb_un.Text) + 1).ToString();
                        });
                        Games_unlisted.Add(nameValue, appid);
                    }
                }
                sr.Close();
            }
        }


        private async void Process_Games()
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "output.txt");
            this.Dispatcher.Invoke(() =>
            {
                tb_Task.Text = "Loading your Steam library";
            });
            
            using (StreamReader sr = new StreamReader(file_path))
            {
                string fileContents = sr.ReadToEnd();
                Regex regex_state = new Regex(@"- State\s+:\s+([A-Za-z]+)");
                Regex regex_apps = new Regex(@"- Apps\s*:\s*([\d,\s]+),\s*\(\d+\s*in total\)");
                MatchCollection matches_apps = regex_apps.Matches(fileContents);
                MatchCollection matches_states = regex_state.Matches(fileContents);


                for (int i = 1; i < matches_apps.Count; i++)
                {
                    
                    Match stateMatch = matches_states[i];
                    Match appMatch = matches_apps[i];

                    this.Dispatcher.Invoke(() =>
                    {
                        double percentage;
                        percentage = (double)((double)i / (double)matches_apps.Count *100);
                        LoadingBar.Value = percentage;
                    });

                    if (stateMatch.Groups[1].Value.Trim().ToLower() == "active")
                    {
                        string appIdStr = appMatch.Groups[1].Value.Replace(",", "").Trim();
                        string[] appIdsArray = appIdStr.Split(' ');
                        foreach (string appId in appIdsArray)
                        {
                            if (int.TryParse(appId, out int id))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    tb_care_ii.Text = id.ToString();
                                });
                                if (Games.ContainsValue(id))
                                {
                                    string targetKey = Games.FirstOrDefault(x => x.Value == id).Key;
                                    if (!Games_owned.ContainsKey(targetKey))
                                    {
                                        Games_owned.Add(targetKey, id);
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            tb_un_Copy.Text = (Convert.ToInt32(tb_un_Copy.Text) + 1).ToString();
                                        });
                                    }
                                }
                                else
                                {

                                    WebRequest request2 = WebRequest.Create("https://store.steampowered.com/api/libraryappdetails/?appid=" + id.ToString());
                                    HttpWebResponse responseName = (HttpWebResponse)request2.GetResponse();
                                    Task.Delay(200);
                                    if (responseName.StatusDescription == "OK")
                                    {
                                        Stream datastream2 = responseName.GetResponseStream();
                                        StreamReader reader2 = new StreamReader(datastream2);
                                        string response3 = reader2.ReadToEnd();

                                        JObject data = JObject.Parse(response3);
                                        string success = (string)data["status"];
                                        if (success == "1")
                                        {
                                            string name = (string)data["name"];
                                            if (!Games_owned.ContainsKey(name))
                                            {
                                                Games_owned.Add(name, id);
                                                this.Dispatcher.Invoke(() =>
                                                {
                                                    tb_un_Copy.Text = (Convert.ToInt32(tb_un_Copy.Text) + 1).ToString();
                                                });
                                            }
                                        }
                                        else
                                        {
                                            if (unlisted == true)
                                            {
                                                ints.Add(id);
                                            }
                                        }
                                    }

                                    
                                }

                            }
                        }
                    }
                }
                sr.Close();
            }
            Games.Clear();


            if (unlisted == true)
            {
                foreach (int id in ints)
                {
                    await Process_Game(id);
                }
            }

            foreach (Game game in games)
            {
                if (Games_owned.ContainsValue(game.SteamAppid))
                {
                    game.Title = game.Title + " Cracked";
                }
            }
            foreach (KeyValuePair<string, int> game in Games_owned)
            {
                Game game_to_add = new Game();
                game_to_add.Title = game.Key;
                game_to_add.SteamAppid = game.Value;

                games.Add(game_to_add);
            }
            Games_owned.Clear();

            if(Games_unlisted.Count >0)
            {
                string unlistedPath = path + "\\Games_unlisted.xml";


                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("games");
                doc.AppendChild(root);

                
                foreach(KeyValuePair<string, int> game in Games_unlisted)
                {
                    XmlElement gameElement = doc.CreateElement("game");
                    root.AppendChild(gameElement);

                    // Create the title element for the game
                    XmlElement titleElement = doc.CreateElement("title");
                    titleElement.InnerText = game.Key;
                    gameElement.AppendChild(titleElement);

                    // Create the steamappid element for the game
                    XmlElement steamAppIdElement = doc.CreateElement("steamappid");
                    steamAppIdElement.InnerText = game.Value.ToString();
                    gameElement.AppendChild(steamAppIdElement);
                }
                // Save the XML document
                doc.Save(unlistedPath);
            }

            
            
        }


        List<int> ints = new List<int>();

        Dictionary<string, int> Games = new Dictionary<string, int>();
        Dictionary<string, int> Games_owned = new Dictionary<string, int>();
        Dictionary<string, int> Games_unlisted = new Dictionary<string, int>();

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
