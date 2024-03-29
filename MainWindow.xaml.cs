﻿using System;
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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frameMain.Content = new Page1();
        }

        private void textBlock_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            frameMain.Content = new Page1();
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
