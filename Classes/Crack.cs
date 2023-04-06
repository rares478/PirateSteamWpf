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
using System.Net.Http;
using System.Security.Policy;
using System.Runtime.CompilerServices;

namespace WpfApp3.Classes
{
    class Crack
    {
        static string[] dllpaths64 = { };
        static string[] dllpaths32 = { };
        public static string filepath = "";
        static bool is64 = true;

        //This need rework
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


                        //These are the screenshots, maybe I'll use them  someday
                        /*JArray ss = data[DLCs[j].ToString()]["data"]["screenshots"] as JArray;

                        foreach (JObject screenshot in ss)
                        {
                            string fullPath = screenshot["path_full"].ToString();
                            screenshots.Add(fullPath);
                        }*/

                        using (StreamWriter writer2 = File.AppendText(path))
                        {
                            writer2.WriteLine(DLCs[j].ToString() + " = " + names[j]);
                        }
                    }
                }
            }
        }


        private static void ReplaceDLLs(string directory, bool swap)
        {
            Array.Clear(dllpaths64);
            Array.Clear(dllpaths32);

            if (swap == false)
            {
                dllpaths64 = Directory.GetFiles(directory, "steam_api64.dll", SearchOption.AllDirectories);
                dllpaths32 = Directory.GetFiles(directory, "steam_api.dll", SearchOption.AllDirectories);

                if (dllpaths64.Length == 0)
                    is64 = false;

                foreach (string dllpath in dllpaths64)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api64_o.dll");
                }
                for (int i = 0; i < dllpaths64.Length; i++)
                {
                    dllpaths64[i] = Path.GetDirectoryName(dllpaths64[i]);
                }

                foreach (string dllpath in dllpaths32)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api_o.dll");
                }
                for (int i = 0; i < dllpaths32.Length; i++)
                {
                    dllpaths32[i] = Path.GetDirectoryName(dllpaths32[i]);
                }

            }
            else
            {
                string[] dll64 = Directory.GetFiles(directory, "steam_api64_o.dll", SearchOption.AllDirectories);
                string[] dll32 = Directory.GetFiles(directory, "steam_api_o.dll", SearchOption.AllDirectories);

                foreach (string dllpath in dll64)
                {
                    File.Delete(Directory.GetParent(dllpath).ToString() + "\\steam_api64.dll");
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api64.dll");
                }
                foreach (string dllpath in dll32)
                {
                    File.Delete(Directory.GetParent(dllpath).ToString() + "\\steam_api.dll");
                    Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(dllpath, "steam_api.dll");
                }
            }


        }

        #region CreamAPI
        static public void CreamAPI(int appid, string path_dir, string path_exe)
        {
            ReplaceDLLs(path_dir, false);

            foreach (string dllpath in dllpaths64)
            {
                File.Copy(@"Good_Stuff\CreamAPI\steam_api64.dll", Path.Combine(dllpath, "steam_api64.dll"));
                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath, "cream_api.ini")))
                {
                    sw.Write("; This is a simplier version of Deadmau5's Cream_api with the comments removed, so it becomes easier\r\n; to navigate to the main values. All credits go to Deadmau5 for making this wrapper.  \r\n\r\n[steam]\r\nappid =" + appid + "\r\nwrappermode = true          \r\n\r\n[steam_wrapper]\r\nnewappid =480\r\nwrapperremotestorage = true\r\nwrapperuserstats = true\r\n\r\n[dlc]\r\n");
                    sw.Close();
                }
                getDLC(appid, Path.Combine(dllpath, "cream_api.ini"));
            }
            foreach (string dllpath in dllpaths32)
            {
                File.Copy(@"Good_Stuff\CreamAPI\steam_api.dll", Path.Combine(dllpath, "steam_api.dll"));
                if (is64 == false)
                {
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath, "cream_api.ini")))
                    {
                        sw.Write("; This is a simplier version of Deadmau5's Cream_api with the comments removed, so it becomes easier\r\n; to navigate to the main values. All credits go to Deadmau5 for making this wrapper.  \r\n\r\n[steam]\r\nappid =" + appid + "\r\nwrappermode = true          \r\n\r\n[steam_wrapper]\r\nnewappid =480\r\nwrapperremotestorage = true\r\nwrapperuserstats = true\r\n\r\n[dlc]\r\n");
                        sw.Close();
                    }
                    getDLC(appid, Path.Combine(dllpath, "cream_api.ini"));
                }
            }

            if (File.Exists(Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api64.dll")) == false)
                File.Copy(@"Good_Stuff\CreamAPI\steam_api64.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api64.dll"));

            if (dllpaths32.Length > 0)
                if (File.Exists(Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api.dll")) == false)
                    File.Copy(@"Good_Stuff\CreamAPI\steam_api.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api.dll"));

            using (StreamWriter sw = new StreamWriter(Path.Combine(Directory.GetParent(path_exe).ToString(), "cream_api.ini"), false, Encoding.ASCII))
            {
                sw.Write("; This is a simplier version of Deadmau5's Cream_api with the comments removed, so it becomes easier\r\n; to navigate to the main values. All credits go to Deadmau5 for making this wrapper.  \r\n\r\n[steam]\r\nappid =" + appid + "\r\nwrappermode = true          \r\n\r\n[steam_wrapper]\r\nnewappid =480\r\nwrapperremotestorage = true\r\nwrapperuserstats = true\r\n\r\n[dlc]\r\n");
                sw.Close();
            }
            getDLC(appid, Path.Combine(Directory.GetParent(path_exe).ToString(), "cream_api.ini"));


            if (File.Exists(Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api64_o.dll")) == false)
                File.Copy(dllpaths64[0] + "\\steam_api64_o.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api64_o.dll"));

            if (dllpaths32.Length > 0)
                if (File.Exists(Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api_o.dll")) == false)
                    File.Copy(dllpaths32[0] + "\\steam_api_o.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "steam_api_o.dll"));

            File.Copy(@"Good_Stuff\CreamAPI\SteamOverlay64.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "SteamOverlay64.dll"));

            using (StreamWriter writer = File.AppendText(Path.Combine(Directory.GetParent(path_exe).ToString() + "\\dlllist.txt")))
            {
                writer.WriteLine("SteamOverlay64.dll\n");
            }
            File.Copy(@"Good_Stuff\CreamAPI\winmm.dll", Path.Combine(Directory.GetParent(path_exe).ToString(), "winmm.dll"));
        }
        #endregion

        #region Goldberg
        static public void GoldbergNormal(int appid, string path)
        {
            ReplaceDLLs(path, false);

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
            ReplaceDLLs(path, false);

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


                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                {
                    sw.Write(appid.ToString());
                }
                using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt"))) { }
                getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));

                if (denuvo)
                {
                    denuvocpy(appid);
                    string steamID = "";
                    var url = "https://steamcommunity.com/profiles/[U:1:" + accountid + "]?xml=1";
                    using var client = new WebClient();

                    try
                    {
                        var xmlString = client.DownloadString(url);

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlString);

                        XmlNode steamID64Node = xmlDoc.SelectSingleNode("/profile/steamID64");
                        steamID = steamID64Node.InnerText;
                    }
                    catch (WebException e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }


                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_steamid.txt")))
                    {
                        sw.Write(steamID);
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


                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "steam_appid.txt")))
                    {
                        sw.Write(appid.ToString());
                    }
                    using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "DLC.txt"))) { }
                    getDLC(appid, Path.Combine(dllpath2, "DLC.txt"));

                    if (denuvo)
                    {
                        denuvocpy(appid);

                        string steamID = "";
                        var url = "https://steamcommunity.com/id/[U:1:" + accountid + "?xml=1";
                        using var client = new WebClient();

                        try
                        {
                            var xmlString = client.DownloadString(url);

                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(xmlString);

                            XmlNode steamID64Node = xmlDoc.SelectSingleNode("/profile/steamID64");
                            steamID = steamID64Node.InnerText;
                        }
                        catch (WebException e)
                        {
                            Console.WriteLine($"Error: {e.Message}");
                        }

                        using (StreamWriter sw = File.CreateText(Path.Combine(dllpath2, "force_steamid.txt")))
                        {
                            sw.Write(steamID);
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

        private static long accountid = 0;

        static public void denuvocpy(int appid)
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
                        accountid = Convert.ToInt64(Path.GetFileName(Directory.GetParent(directory).ToString()));

                        foreach (string file in files)
                        {
                            string fileName = Path.GetFileName(file);
                            string destinationFilePath = Path.Combine(denuvoDirPath, fileName);
                            File.Copy(file, destinationFilePath, true);
                        }
                        break;
                    }
                }
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
            File.Copy(@"Good_Stuff\StubDRM\x64\StubDRM64.dll", Path.Combine(path, "StubDRM64.dll"));
            if (File.Exists(path + "\\winmm.dll") == false)
                File.Copy(@"Good_Stuff\StubDRM\winmm.dll", Path.Combine(path, "winmm.dll"));
            using (StreamWriter writer2 = File.AppendText(path + "\\dlllist.txt"))
            {
                writer2.WriteLine("StubDRM64.dll\n");
            }
        }
        static public void SteamStubDRM32(string path)
        {
            File.Copy(@"Good_Stuff\StubDRM\x32\StubDRM32.dll", Path.Combine(path, "StubDRM32.dll"));
            if (File.Exists(path + "\\winmm.dll") == false)
                File.Copy(@"Good_Stuff\StubDRM\winmm.dll", Path.Combine(path, "winmm.dll"));
            using (StreamWriter writer2 = File.AppendText(path + "\\dlllist.txt"))
            {
                writer2.WriteLine("StubDRM32.dll\n");
            }
        }
        #endregion

        private static List<string> GenerateGameNameCombinations(string gameName)
        {
            // Remove invalid characters from game name
            string cleanedName = new string(gameName.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());

            // Split game name into words
            string[] words = cleanedName.Split(' ');

            // Generate all combinations of the words without spaces
            List<string> combinations = new List<string>();
            for (int i = 1; i <= words.Length; i++)
            {
                foreach (var subset in words.Combinations(i))
                {
                    string combination = string.Concat(subset).ToLowerInvariant();
                    combinations.Add(combination);
                }
            }

            return combinations;
        }

        public static string[] FindGameExe(string gameName)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select the Steam installation folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string ParentPath = dialog.FileName;

                List<string> gameNameList = GenerateGameNameCombinations(gameName);
                string exePath = "";
                try
                {
                    // Check each name combination for an executable file in the parent directory
                    foreach (string name in gameNameList)
                    {
                        string[] files = Directory.GetFiles(ParentPath, name + "*.exe", SearchOption.AllDirectories);

                        if (files.Length > 0)
                        {
                            exePath = files[0];
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the search
                    Console.WriteLine($"Error searching for {gameName}: {ex.Message}");
                }

                // if exePath is "" i should ask to select it
                string[] strings = { exePath, dialog.FileName };
                return strings;
            }
            return null;
        }

        public static void RemoveCrack(string path)
        {
            string[] dllpaths64 = { };
            string[] dllpaths32 = { };

            dllpaths64 = Directory.GetFiles(path, "steam_api64_o.dll", SearchOption.AllDirectories);
            dllpaths32 = Directory.GetFiles(path, "steam_api_o.dll", SearchOption.AllDirectories);

            ReplaceDLLs(path, true);

            foreach (string dll in dllpaths64)
            {
                string parent = Directory.GetParent(dll).ToString();

                if (Directory.Exists(parent + "\\steam_settings"))
                    Directory.Delete(parent + "\\steam_settings", true);
                if (File.Exists(parent + "\\cream_api.ini") == true)
                    File.Delete(parent + "\\cream_api.ini");
                if (File.Exists(parent + "\\dlllist.txt") == true)
                    File.Delete(parent + "\\dlllist.txt");
                if (File.Exists(parent + "\\winmm.dll") == true)
                    File.Delete(parent + "\\winmm.dll");
                if (File.Exists(parent + "\\SteamOverlay64.dll") == true)
                    File.Delete(parent + "\\SteamOverlay64.dll");
                if (File.Exists(parent + "\\StubDRM64.dll") == true)
                    File.Delete(parent + "\\StubDRM64.dll");
                if (File.Exists(parent + "\\steamclient64.dll") == true)
                    File.Delete(parent + "\\steamclient64.dll");
            }
            foreach (string dll in dllpaths32)
            {
                string parent = Directory.GetParent(dll).ToString();

                if (Directory.Exists(parent + "\\steam_settings"))
                    Directory.Delete(parent + "\\steam_settings", true);
                if (File.Exists(parent + "\\cream_api.ini") == true)
                    File.Delete(parent + "\\cream_api.ini");
                if (File.Exists(parent + "\\dlllist.txt") == true)
                    File.Delete(parent + "\\dlllist.txt");
                if (File.Exists(parent + "\\winmm.dll") == true)
                    File.Delete(parent + "\\winmm.dll");
                if (File.Exists(parent + "\\SteamOverlay64.dll") == true)
                    File.Delete(parent + "\\SteamOverlay64.dll");
                if (File.Exists(parent + "\\StubDRM32.dll") == true)
                    File.Delete(parent + "\\StubDRM32.dll");
                if (File.Exists(parent + "\\steamclient.dll") == true)
                    File.Delete(parent + "\\steamclient.dll");
            }
        }
    }
    public static class Extensions
    {
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
                elements.SelectMany((e, i) =>
                    elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }
    }
}
