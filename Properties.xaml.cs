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
using System.Windows.Shapes;
using WpfApp3.Classes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class PropertiesYeah : Window
    {
        public PropertiesYeah(Game game1)
        {
            InitializeComponent();
            game = game1;
            tb_Name.Text = game.Title;
            frame.Content = new Properties.General(game);
        }
        private Game game;
        private void lb_Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lb_Properties.SelectedIndex)
            {
                case 0: 
                    frame.Content = new Properties.General(game);
                    break;
                case 1:
                    frame.Content = new Properties.Files(game);
                    break;
            }
        }
    }
}
