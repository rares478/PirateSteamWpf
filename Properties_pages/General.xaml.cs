using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace WpfApp3.Properties
{
    public partial class General : Page
    {
        
        private static string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";

        Game game = new Game();

        public General(Game game1)
        {
            InitializeComponent();
            game = game1;

            switch (game.Type)
            {
                case "CreamAPI":
                    rb_CreamAPI.IsChecked = true; break;
                case "Goldberg":
                    rb_Goldberg.IsChecked = true; break;
                case "Goldberg Experimental":
                    rb_Goldberg_Experimental.IsChecked = true; break;
                case "Denuvo":
                    rb_Goldberg_Experimental.IsChecked = true; break;
                default:rb_None.IsChecked = true; break;
            }

        }

        private void bt_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bt_Close_Click(object sender, RoutedEventArgs e)
        {
            bt_Save_Click(sender, e);
            
        }

        private void rb_CreamAPI_Checked(object sender, RoutedEventArgs e)
        {
            if(game.Type != "CreamAPI")
            {
                Crack.RemoveCrack(game.Path_Directory);
                MessageBox.Show("Done Deleting");
                Crack.CreamAPI(game.SteamAppid, game.Path_Directory, game.Path);
                game.Type = "CreamAPI";

                XDocument doc = XDocument.Load(xml);

                // find the game element with the specified steamappid
                XElement gameElement = doc.Descendants("game")
                                  .Where(e => (int)e.Element("steamappid") == game.SteamAppid)
                                  .FirstOrDefault();

                if (game != null)
                {
                    // set the value of the launch element to the desired value
                    gameElement.Element("type").Value = "CreamAPI";

                    // save the modified XML file
                    doc.Save(xml);
                }
            }
        }
        private void rb_Goldberg_Checked(object sender, RoutedEventArgs e)
        {
            if (game.Type != "Goldberg")
            {
                Crack.RemoveCrack(game.Path_Directory);
                MessageBox.Show("Done Deleting");
                Crack.GoldbergNormal(game.SteamAppid, game.Path_Directory);
                game.Type = "Goldberg";

                XDocument doc = XDocument.Load(xml);

                // find the game element with the specified steamappid
                XElement gameElement = doc.Descendants("game")
                                  .Where(e => (int)e.Element("steamappid") == game.SteamAppid)
                                  .FirstOrDefault();

                if (game != null)
                {
                    // set the value of the launch element to the desired value
                    gameElement.Element("type").Value = "Goldberg";

                    // save the modified XML file
                    doc.Save(xml);
                }
            }
        }
        private void rb_GoldbergExperimental_Checked(object sender, RoutedEventArgs e)
        {
            if (game.Type != "Goldberg Experimental" && game.Type != "Denuvo" )
            {
                Crack.RemoveCrack(game.Path_Directory);
                MessageBox.Show("Done Deleting");
                Crack.GoldbergExperimental(game.SteamAppid, game.Path_Directory, false);
                game.Type = "Goldberg Experimental";

                XDocument doc = XDocument.Load(xml);

                // find the game element with the specified steamappid
                XElement gameElement = doc.Descendants("game")
                                  .Where(e => (int)e.Element("steamappid") == game.SteamAppid)
                                  .FirstOrDefault();

                if (game != null)
                {
                    // set the value of the launch element to the desired value
                    gameElement.Element("type").Value = "Goldberg Experimental";

                    // save the modified XML file
                    doc.Save(xml);
                }
            }
        }

        private void cb_SteamStub_Checked(object sender, RoutedEventArgs e)
        {
            Crack.SteamStubDRM64(Directory.GetParent(game.Path).ToString());
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
