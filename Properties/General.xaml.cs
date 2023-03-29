using System;
using System.Collections.Generic;
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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : Page
    {
        
        private static string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";

        public General(int id)
        {
            InitializeComponent();
            id1 = id;
        }

        private int id1;

        private void bt_Save_Click(object sender, RoutedEventArgs e)
        {
            string launch_option = tb_LaunchOption.Text;
            Page1.games[id1].Launch_Options = launch_option;

            XDocument xml1 = XDocument.Load(xml);
            XElement game = xml1.Descendants("game")
                                .FirstOrDefault(e => (int)e.Element("steamappid") == Page1.games[id1].SteamAppid);
            if (game != null)
            {
                game.Element("launch").Value = launch_option;

                xml1.Save(xml);
            }
        }

        private void bt_Close_Click(object sender, RoutedEventArgs e)
        {
            bt_Save_Click(sender, e);
            
        }
    }
}
