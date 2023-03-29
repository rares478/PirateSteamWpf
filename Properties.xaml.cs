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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : Window
    {
        public Properties(int id)
        {
            InitializeComponent();
            id1 = id;
        }
        int id1;
        private void lb_Properties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lb_Properties.SelectedIndex)
            {
                case 0: 
                    frame.Content = new General(id1);
                    break;
                //case 1:
            }
        }
    }
}
