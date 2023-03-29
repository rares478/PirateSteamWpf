using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WpfApp3
{
    class Crack
    {
        static string[] dllpaths64;
        static string[] dllpaths32;
        public static string filepath;
        static bool is64 = true;
        public static List<string> screenshots = new List<string>();

        static public void getDLC(int appid, string path)
        {
            WebRequest request = WebRequest.Create("https://store.steampowered.com/dlc/" + appid + "/random/ajaxgetfilteredrecommendations/?query&count=10000");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusDescription == "OK")
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string response2 = reader.ReadToEnd();
                int i = 0;
                int[] dlcid = new int[1000];
                while (response2.Length > 0)
                {
                    try
                    {
                        dlcid[i] = Convert.ToInt32(response2.Substring(response2.IndexOf("data-ds-appid") + 16, 7));
                        response2 = response2.Substring(response2.IndexOf("data-ds-appid") + 23);
                    }
                    catch
                    {
                        break;

                    }


                    if (response2.Contains("data-ds-appid") == false)
                        break;
                    i++;
                }

                int[] DLCs = dlcid.Distinct().ToArray();
                string[] names = new string[DLCs.Length];
                i = 0;
                for (int j = 0; j < DLCs.Length - 1; j++)
                {
                    WebRequest request2 = WebRequest.Create("https://store.steampowered.com/api/appdetails?appids=" + DLCs[j].ToString());
                    HttpWebResponse responseName = (HttpWebResponse)request2.GetResponse();
                    Task.Delay(100);
                    if (responseName.StatusDescription == "OK")
                    {
                        Stream datastream2 = responseName.GetResponseStream();
                        StreamReader reader2 = new StreamReader(datastream2);
                        string response3 = reader2.ReadToEnd();

                        JObject data = JObject.Parse(response3);
                        names[j] = (string)data[DLCs[j].ToString()]["data"]["name"];

                        JArray ss = data[DLCs[j].ToString()]["data"]["screenshots"] as JArray;
                        List<string> fullPaths = new List<string>();

                        foreach (JObject screenshot in ss)
                        {
                            string fullPath = screenshot["path_full"].ToString();
                            screenshots.Add(fullPath);
                        }

                        using (StreamWriter writer2 = File.AppendText(path))
                        {
                            writer2.WriteLine(DLCs[j].ToString() + " = " + names[j]);
                        }
                    }
                }
            }
        }

        static public string find_exe()
        {
            OpenFileDialog saveFile = new OpenFileDialog();
            saveFile.Filter = "File exe|*.exe";
            saveFile.Title = "Select the exe";
            saveFile.ShowDialog();
            string pathtofile = "";
            string directory = "";

            if (saveFile.FileName != "")
            {

                pathtofile = saveFile.FileName;
                directory = Path.GetDirectoryName(saveFile.FileName);

                dllpaths64 = Directory.GetFiles(directory, "steam_api64.dll", SearchOption.AllDirectories);
                dllpaths32 = Directory.GetFiles(directory, "steam_api.dll", SearchOption.AllDirectories);

                if (dllpaths64.Length == 0)
                    is64 = false;

                foreach (string dllpath in dllpaths64)
                {
                    FileInfo inf = new FileInfo(dllpath);
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api64_o.dll");
                }
                for (int i = 0; i < dllpaths64.Length; i++)
                {
                    dllpaths64[i] = Path.GetDirectoryName(dllpaths64[i]);
                }

                foreach (string dllpath in dllpaths32)
                {
                    FileInfo inf = new FileInfo(dllpath);
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api_o.dll");
                }
                for (int i = 0; i < dllpaths32.Length; i++)
                {
                    dllpaths32[i] = Path.GetDirectoryName(dllpaths32[i]);
                }
            }
            return pathtofile;
        }


        #region CreamAPI
        static public void CreamAPI(int appid)
        {
            foreach (string dllpath in dllpaths64)
            {
                File.Copy(@"Good_Stuff\CreamAPI\steam_api64.dll", Path.Combine(dllpath, "steam_api64.dll"));
                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath, "creamapi.ini")))
                {
                    sw.Write("\r\n; This is a simplier version of Deadmau5's Cream_api with the comments removed, so it becomes easier\r\n; to navigate to the main values. All credits go to Deadmau5 for making this wrapper.  \r\n\r\n[steam]\r\nappid =" + appid + "\r\nwrappermode = true          \r\n\r\n[steam_wrapper]\r\nnewappid =480\r\nwrapperremotestorage = true\r\nwrapperuserstats = true\r\n\r\n[dlc]\r\n");
                }
                getDLC(appid, Path.Combine(dllpath, "creamapi.ini"));
            }
            foreach (string dllpath in dllpaths32)
            {
                File.Copy(@"Good_Stuff\CreamAPI\steam_api.dll", Path.Combine(dllpath, "steam_api.dll"));
                if (is64 == false)
                {
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath, "creamapi.ini")))
                    {
                        sw.Write("\r\n; This is a simplier version of Deadmau5's Cream_api with the comments removed, so it becomes easier\r\n; to navigate to the main values. All credits go to Deadmau5 for making this wrapper.  \r\n\r\n[steam]\r\nappid =" + appid + "\r\nwrappermode = true          \r\n\r\n[steam_wrapper]\r\nnewappid =480\r\nwrapperremotestorage = true\r\nwrapperuserstats = true\r\n\r\n[dlc]\r\n");
                    }
                    getDLC(appid, Path.Combine(dllpath, "creamapi.ini"));
                }
            }
        }
        #endregion

        #region Goldberg
        static public void GoldbergNormal(int appid)
        {
            foreach (string dllpath in dllpaths64)
            {
                File.Copy(@"Good_Stuff\Goldberg Normal\steam_api64.dll", Path.Combine(dllpath, "steam_api64.dll"));

                string dllpath2 = Path.Combine(dllpath, "steam_settings");
                Directory.CreateDirectory(dllpath2);

                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                {
                    sw.Write(appid.ToString());
                }
                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt")))
                {

                }
                getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));
            }

            foreach (string dllpath in dllpaths32)
            {
                File.Copy(@"Good_Stuff\Goldberg Normal\steam_api.dll", Path.Combine(dllpath, "steam_api.dll"));
                if (is64 == false)
                {
                    string dllpath2 = Path.Combine(dllpath, "steam_settings");
                    Directory.CreateDirectory(dllpath2);

                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                    {
                        sw.Write(appid.ToString());
                    }
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt")))
                    {

                    }
                    getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));
                }
            }
        }

        static public void GoldbergExperimental(int appid, string path, bool denuvo)
        {
            int steamid = 0;

            string[] pathclient = Directory.GetFiles(path, "*Shipping.exe", SearchOption.AllDirectories);
            if (pathclient.Length > 0)
            {
                if (is64)
                    File.Copy(@"Good_Stuff\Goldberg Experimental\steamclient64.dll", Path.Combine(Path.GetDirectoryName(pathclient[0]), "steamclient64.dll"));
                else
                    File.Copy(@"Good_Stuff\Goldberg Experimental\steamclient.dll", Path.Combine(Path.GetDirectoryName(pathclient[0]), "steamclient.dll"));
            }

            foreach (string dllpath in dllpaths64)
            {
                File.Copy(@"Good_Stuff\Goldberg Experimental\steam_api64.dll", Path.Combine(dllpath, "steam_api64.dll"));


                string dllpath2 = Path.Combine(dllpath, "steam_settings");
                Directory.CreateDirectory(dllpath2);
                denuvocpy(appid, path);

                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                {
                    sw.Write(appid.ToString());
                }
                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt"))) { }
                getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));

                if (denuvo)
                {
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_steamid.txt")))
                    {
                        sw.Write(steamid.ToString());
                    }
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_language.txt")))
                    {
                        sw.Write("english");
                    }
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "listen_port.txt")))
                    {
                        sw.Write("47584");
                    }

                }
            }
            foreach (string dllpath in dllpaths32)
            {
                File.Copy(@"Good_Stuff\Goldberg Experimental\steam_api.dll", Path.Combine(dllpath, "steam_api.dll"));

                if (is64 == false)
                {
                    string dllpath2 = Path.Combine(dllpath, "steam_settings");
                    Directory.CreateDirectory(dllpath2);
                    denuvocpy(appid, path);

                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                    {
                        sw.Write(appid.ToString());
                    }
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt"))) { }
                    getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));

                    if (denuvo)
                    {
                        using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_steamid.txt")))
                        {
                            sw.Write(steamid.ToString());
                        }
                        using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_language.txt")))
                        {
                            sw.Write("english");
                        }
                        using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "listen_port.txt")))
                        {
                            sw.Write("47584");
                        }

                    }
                }
            }

        }

        static public void denuvocpy(int appid, string path)
        {
            string xml = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam\\settings.xml");
            string steamExecutable = "";
            string steamPath = "";
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList steamElements = doc.GetElementsByTagName("Steam");
            foreach (XmlNode steamElement in steamElements)
            {
                if (steamElement.InnerText != "")
                {
                    steamExecutable = steamElement.InnerText + "\\steam.exe";
                    steamPath = steamElement.InnerText;
                }
                else
                {
                    steamExecutable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steam.exe");
                    steamPath = steamElement.InnerText;
                }
            }
            if (File.Exists(steamExecutable))
            {
                string appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string piratedSteamFolderPath = Path.Combine(appDataRoamingPath, "PirateSteam");
                string denuvoDirPath = Path.Combine(piratedSteamFolderPath, "denuvo\\" + appid);
                Directory.CreateDirectory(denuvoDirPath);

                string folderName = appid.ToString();
                string[] directories = Directory.GetDirectories(steamPath + "\\userdata", "*", SearchOption.AllDirectories);

                foreach (string directory in directories)
                {
                    if (directory.EndsWith("\\" + folderName))
                    {
                        string[] files = Directory.GetFiles(directory);

                        foreach (string file in files)
                        {
                            string fileName = Path.GetFileName(file);
                            string destinationFilePath = Path.Combine(denuvoDirPath, fileName);
                            File.Copy(file, destinationFilePath, true);
                        }
                        break;
                    }
                }

                //GoldbergExperimental(appid, path, true);
            }
            else
            {
                MessageBoxResult dialogResult = MessageBox.Show("Steam is not installed on default location. Please select installation folder.", "", MessageBoxButton.OK);
                if (dialogResult == MessageBoxResult.OK)
                {
                    var dialog = new CommonOpenFileDialog();
                    dialog.Title = "Select the Steam installation folder";
                    dialog.IsFolderPicker = true;
                    dialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        // Check if the Steam executable exists in the selected folder
                        string steamExe = Path.Combine(dialog.FileName, "steam.exe");

                        if (File.Exists(steamExe))
                        {
                            MessageBox.Show("Steam is installed at: " + dialog.FileName);

                            XmlDocument doc2 = new XmlDocument();
                            doc2.Load(xml);
                            XmlNodeList SteamElements = doc2.GetElementsByTagName("Steam");
                            foreach (XmlNode SteamElement in SteamElements)
                            {
                                SteamElement.InnerText = dialog.FileName;
                            }
                            doc2.Save(xml);

                            string appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            string piratedSteamFolderPath = Path.Combine(appDataRoamingPath, "PirateSteam");
                            string denuvoDirPath = Path.Combine(piratedSteamFolderPath, "denuvo\\" + appid);
                            Directory.CreateDirectory(denuvoDirPath);

                            string folderName = appid.ToString();
                            string[] directories = Directory.GetDirectories(dialog.FileName + "\\userdata", "*", SearchOption.AllDirectories);

                            foreach (string directory in directories)
                            {
                                if (directory.EndsWith("\\" + folderName))
                                {
                                    string[] files = Directory.GetFiles(directory);

                                    foreach (string file in files)
                                    {
                                        string fileName = Path.GetFileName(file);
                                        string destinationFilePath = Path.Combine(denuvoDirPath, fileName);
                                        File.Copy(file, destinationFilePath, true);
                                    }
                                    break;
                                }
                            }

                            //GoldbergExperimental(appid, path, true);
                        }
                        else
                        {
                            MessageBox.Show("The selected folder is not a valid Steam installation folder.");
                        }
                    }

                }
            }

        }

        #endregion

        #region SteamStub
        static public void SteamStubDRM64(string path)
        {
            File.Copy(@"Good_Stuff\StubDRM\StubDRM64.dll", Path.Combine(path, "StubDRM64.dll"));
            File.Copy(@"Good_Stuff\StubDRM\winmm.dll", Path.Combine(path, "winmm.dll"));
            File.Copy(@"Good_Stuff\StubDRM\dlllist.txt", Path.Combine(path, "dlllist.txt"));
        }
        static public void SteamStubDRM32(string path)
        {
            File.Copy(@"Good_Stuff\StubDRM\StubDRM32.dll", Path.Combine(path, "StubDRM32.dll"));
            File.Copy(@"Good_Stuff\StubDRM\winmm.dll", Path.Combine(path, "winmm.dll"));
            File.Copy(@"Good_Stuff\StubDRM\dlllist.txt", Path.Combine(path, "dlllist.txt"));
        }
        #endregion
    }
}
