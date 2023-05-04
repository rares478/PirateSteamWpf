using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Octokit;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using IWshRuntimeLibrary;
using System.Linq;
using WpfApp3.Classes;
using System.Windows.Controls;
using System.Reflection;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Install : Window
    {    
        private string InstallPath = string.Empty;

        private Game game;
        private Library library;

        public Install(Game game, Library library)
        {
            InitializeComponent();
            tb_DiskRequired.Text = Util.FormatBytes(game.Size);
            this.game = game;
            this.library = library;
        }


        private void tb_Location_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select where to install";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                InstallPath = dialog.FileName;
                tb_Location.Text = dialog.FileName;
                string dir = Path.GetPathRoot(InstallPath);
                DriveInfo driveinfo = new DriveInfo(dir);

                long availableSpaceInBytes = driveinfo.AvailableFreeSpace;
                tb_DiskAvailable.Text = Util.FormatBits(availableSpaceInBytes);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async void bt_Crack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            library.downloadGame(game, tb_Location.Text, game.Size);
        }

    }
}
