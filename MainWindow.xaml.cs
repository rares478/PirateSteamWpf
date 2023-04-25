using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfApp3.Classes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Classes.User User;
        public static Process SteamCMD = new Process();

        
        public MainWindow()
        {
            User = new Classes.User("rares478", "Paralelipipedut12.");

            InitializeComponent();
            frameMain.Content = new Library();

        }



        private void textBlock_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            frameMain.Content = new Library();
        }

        private void tb_AddGame_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            frameMain.Content = new AddGame();
        }

        private void tb_Settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            frameMain.Content = new Settings();
        }
        private void tb_Sites_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            frameMain.Content = new Sites();
        }

    }
}
