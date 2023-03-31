using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for AddGame.xaml
    /// </summary>
    public partial class AddGame : Page
    {
        public AddGame()
        {
            InitializeComponent();
        }

        Game game = new Game();
        private int type;

        private void bt_SelectPath_Click(object sender, RoutedEventArgs e)
        {
            string[] strings = Crack.FindGameExe(tb_Name.Text);
            game.Path = strings[0];
            tb_Path.Text = game.Path;
            game.Path_Directory = strings[1];
        }

        private void bt_Click(object sender, RoutedEventArgs e)
        {
            ///Ignore
            tb_Link.Visibility = Visibility.Visible;
            tb_Name.Visibility = Visibility.Visible;
            tb_Path.Visibility = Visibility.Visible;
            tb_Link_text.Visibility = Visibility.Visible;
            tb_Name_text.Visibility = Visibility.Visible;
            tb_Path_text.Visibility = Visibility.Visible;
            bt_Save.Visibility = Visibility.Visible;
            bt_SelectPath.Visibility = Visibility.Visible;
            bt_Denuvo.Visibility = Visibility.Hidden;
            bt_PreCracked.Visibility = Visibility.Hidden;
            bt_Crack.Visibility = Visibility.Hidden;

            Button bt = sender as Button;
            switch (bt.Content){
                case "Denuvo": type = 0;
                    break;
                case "Crack": type = 1;
                    break;
                case "PreCracked": type = 2;
                    break;
            }
        }

        private void  bt_Save_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
            string xml = path + "\\Games.xml";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml);

            if(tb_Link.Text == "" || tb_Link.Text == "Please add the link to the game")
            {
                tb_Link_text.Foreground = Brushes.Red;

            }
            else if(tb_Path.Text == ""|| tb_Path.Text == "Please select the path to the game")
            {
                tb_Path_text.Foreground= Brushes.Red;
            }
            else if( tb_Name.Text == "" || tb_Name.Text == "Please add the name of the game")
            {
                tb_Name_text.Foreground = Brushes.Red;
            }
            else
            {
                XmlElement gameElement = xmlDoc.CreateElement("game");

                int appIndex = tb_Link.Text.IndexOf("app");
                string appIdString = tb_Link.Text.Substring(appIndex + 4, tb_Link.Text.IndexOf("/", appIndex + 5) - appIndex - 4);
                int appid = int.Parse(appIdString);

                XmlElement titleElement = xmlDoc.CreateElement("title");
                titleElement.InnerText = tb_Name.Text;
                gameElement.AppendChild(titleElement);
                game.Title = titleElement.InnerText;

                XmlElement pathElement = xmlDoc.CreateElement("path");
                pathElement.InnerText = game.Path;
                gameElement.AppendChild(pathElement);

                XmlElement pathdirElement = xmlDoc.CreateElement("path_directory");
                pathdirElement.InnerText = game.Path_Directory;
                gameElement.AppendChild(pathdirElement);

                XmlElement crackElement = xmlDoc.CreateElement("type");
                {
                    if (type == 0)
                    { 
                        crackElement.InnerText = "Denuvo";
                        Crack.GoldbergExperimental(appid, game.Path_Directory, true);
                    }
                    else if (type == 1)
                    {
                        string settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam\\settings.xml");
                        XmlDocument doc = new XmlDocument();
                        doc.Load(settings);
                        XmlNodeList crackElements = doc.GetElementsByTagName("Crack");
                        
                        foreach (XmlNode crack in crackElements)
                        {
                            MessageBox.Show(crack.InnerText);
                            switch (crack.InnerText)
                            {
                                
                                case "CreamAPI":
                                    crackElement.InnerText = "CreamAPI";
                                    Crack.CreamAPI(appid, game.Path_Directory, game.Path);
                                    break;
                                case "Goldberg":
                                    Crack.GoldbergNormal(appid, game.Path_Directory);
                                    crackElement.InnerText = "Goldberg";
                                    break;
                                case "Goldberg Experimental":
                                    Crack.GoldbergExperimental(appid, game.Path_Directory, false);
                                    crackElement.InnerText = "Goldberg Experimental";
                                    break;
                            }
                        }
                    }
                    else crackElement.InnerText = "PreCracked";
                }
                gameElement.AppendChild(crackElement);
                game.Type = crackElement.InnerText;

                XmlElement backgroundElement = xmlDoc.CreateElement("background");
                backgroundElement.InnerText = "https://cdn.cloudflare.steamstatic.com/steam/apps/"+ appid.ToString() +"/library_hero.jpg?t=1624181121";
                gameElement.AppendChild(backgroundElement);
                game.Background = backgroundElement.InnerText;

                XmlElement logoElement = xmlDoc.CreateElement("logo");
                logoElement.InnerText = "https://cdn.cloudflare.steamstatic.com/steam/apps/" + appid.ToString() + "/logo.png?t=1624181121";
                gameElement.AppendChild(logoElement);
                game.Logo = logoElement.InnerText;

                XmlElement dateElement = xmlDoc.CreateElement("date");
                long date= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                dateElement.InnerText = date.ToString();
                gameElement.AppendChild(dateElement);
                game.Date_Added = date;

                XmlElement last_playedElement = xmlDoc.CreateElement("last_played");
                last_playedElement.InnerText = "";
                gameElement.AppendChild(last_playedElement);
                game.Last_Played = 0;

                XmlElement steamAppIdElement = xmlDoc.CreateElement("steamappid");
                steamAppIdElement.InnerText = appid.ToString();
                gameElement.AppendChild(steamAppIdElement);
                game.SteamAppid = Convert.ToInt32(steamAppIdElement.InnerText);

                XmlElement playtimeElement = xmlDoc.CreateElement("playtime");
                playtimeElement.InnerText = "0.0";
                gameElement.AppendChild(playtimeElement);
                game.Playtime = 0.0f;

                XmlNode gamesNode = xmlDoc.SelectSingleNode("/games");
                gamesNode.AppendChild(gameElement);

                xmlDoc.Save(xml);

            }
            
        }

        public static List<string> GenerateGameNameCombinations(string gameName)
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
                    string combination = String.Concat(subset).ToLowerInvariant();
                    combinations.Add(combination);
                }
            }

            return combinations;
        }

        public static string FindGameDirectory(string gameName, string ParentPath)
        {
            List<string> gameNameList = GenerateGameNameCombinations(gameName);
            string exePath = null;
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

            return exePath;
        }
    }
}
