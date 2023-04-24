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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for UpdateSteamCMD.xaml
    /// </summary>
    public partial class UpdateSteamCMD : Window
    {
        public UpdateSteamCMD()
        {
            InitializeComponent();
            //Process_Games();
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
            /*process.Exited += async (s, e) =>
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                    
                });
            };*/
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }


        bool uptodate = false;

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string output = e.Data;
            if (output != null)
            {
                Dispatcher.Invoke(() =>
                {
                    textBox.AppendText(Environment.NewLine + output);
                });
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
                        Process_Games();
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
                Arguments = appid.ToString()
            };

            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();


            string info_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "app_info.txt");
            using (StreamReader sr = new StreamReader(info_path))
            {
                string fileContents = sr.ReadToEnd();
                string pattern = "\"name\"\\s+\"([^\"]+)\"";
                Match match = Regex.Match(fileContents, pattern);

                if (match.Success)
                {
                    string nameValue = match.Groups[1].Value;
                    if (!Games.ContainsKey(nameValue))
                    {
                        Games.Add(nameValue, appid);
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        tb_ids.AppendText(nameValue + Environment.NewLine);
                    });
                }
                sr.Close();
            }
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "app_info.txt"));
        }


        private async void Process_Games()
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "\\Steam", "output.txt");
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
                                    tb_ids.AppendText(id.ToString() + Environment.NewLine);
                                });

                                if(Games.ContainsValue(id) == false)
                                {
                                    await Process_Game(id);
                                }
                            }
                        }
                    }
                }
                sr.Close();
            }
        }

        Dictionary<string, int> Games = new Dictionary<string, int>();
    }
}
