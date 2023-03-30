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

namespace WpfApp3.Properties
{
    public partial class General : Page
    {
        
        private static string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";

        public General(Game game)
        {
            InitializeComponent();
            id1 = game.SteamAppid;
        }

        private int id1;

        private void bt_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bt_Close_Click(object sender, RoutedEventArgs e)
        {
            bt_Save_Click(sender, e);
            
        }

        private void rb_CreamAPI_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
