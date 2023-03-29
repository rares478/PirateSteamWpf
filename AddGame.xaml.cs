using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
            string path = Crack.find_exe();
            game.Path = path;
            tb_Path.Text = game.Path;
            game.Path_Directory = Path.GetDirectoryName(path);
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
            switch (bt.Name){
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

                XmlElement pathElement = xmlDoc.CreateElement("path");
                pathElement.InnerText = tb_Path.Text;
                gameElement.AppendChild(pathElement);

                XmlElement pathdirElement = xmlDoc.CreateElement("path_directory");
                pathdirElement.InnerText = game.Path_Directory;
                gameElement.AppendChild(pathdirElement);

                XmlElement crackElement = xmlDoc.CreateElement("type");
                {
                    if (type == 0)
                    { 
                        crackElement.InnerText = "Denuvo";
                        Crack.GoldbergExperimental(appid, pathdirElement.InnerText, true);
                    }
                    else if (type == 1)
                    {
                        string settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam\\settings.xml");
                        XmlDocument doc = new XmlDocument();
                        doc.Load(settings);
                        XmlNodeList crackElements = doc.GetElementsByTagName("Crack");
                        foreach (XmlNode crack in crackElements)
                        {
                            switch (crackElement.InnerText)
                            {
                                case "CreamAPI":
                                    crackElement.InnerText = "CreamAPI";
                                    Crack.CreamAPI(appid);
                                    break;
                                case "Goldberg":
                                    Crack.GoldbergNormal(appid);
                                    crackElement.InnerText = "Goldberg";
                                    break;
                                case "Goldberg Experimental":
                                    Crack.GoldbergExperimental(appid, pathElement.InnerText, false);
                                    crackElement.InnerText = "Goldberg Experimental";
                                    break;
                            }
                        }
                    }
                    else crackElement.InnerText = "PreCracked";
                }
                gameElement.AppendChild(crackElement);

                XmlElement backgroundElement = xmlDoc.CreateElement("background");
                Random random = new Random();
                int limit = Crack.screenshots.Count;
                int rand = random.Next(0,limit);
                string img = Crack.screenshots[rand];
                backgroundElement.InnerText = img;
                gameElement.AppendChild(backgroundElement);

                XmlElement dateElement = xmlDoc.CreateElement("date");
                long date= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                dateElement.InnerText = date.ToString();
                gameElement.AppendChild(dateElement);

                XmlElement last_playedElement = xmlDoc.CreateElement("last_played");
                last_playedElement.InnerText = "";
                gameElement.AppendChild(last_playedElement);

                XmlElement steamAppIdElement = xmlDoc.CreateElement("steamappid");
                steamAppIdElement.InnerText = appid.ToString();
                gameElement.AppendChild(steamAppIdElement);

                XmlElement playtimeElement = xmlDoc.CreateElement("playtime");
                playtimeElement.InnerText = "0.0";
                gameElement.AppendChild(playtimeElement);

                XmlElement launchElement = xmlDoc.CreateElement("launch");
                launchElement.InnerText = "";
                gameElement.AppendChild(launchElement);

                XmlNode gamesNode = xmlDoc.SelectSingleNode("/games");
                gamesNode.AppendChild(gameElement);

                xmlDoc.Save(xml);
            }
            
        }
    }
}
