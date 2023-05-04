using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Xml.Linq;
using WpfApp3.Classes;

namespace WpfApp3.Properties
{
    public partial class Files : Page
    {
        private Game game;
        private static string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PirateSteam");
        private static string xml = path + "\\Games.xml";
        public Files(Game game1)
        {
            InitializeComponent();

            if(game1.Installed == 0){
                tb_Size.Text = "Not installed";

                bt_Browse.Visibility = Visibility.Hidden;
                bt_Backup.Visibility = Visibility.Hidden;
                bt_Move.Visibility = Visibility.Hidden;
            }
            else
            {
                game = game1;

                DirectoryInfo folder = new DirectoryInfo(game.Path_Directory);
                long size = 0;

                FileInfo[] files = folder.GetFiles("*.*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)
                {
                    size += file.Length;
                }
                tb_Size.Text = FormatBytes(size);
            }
            
        }


        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void bt_Browse_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", game.Path_Directory);
        }

        private void bt_Move_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select where to move the installation folder";
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    string exename = System.IO.Path.GetFileName(game.Path);
                    Directory.Move(game.Path_Directory, dialog.FileName);
                    XDocument xml1 = XDocument.Load(xml);
                    XElement gameElement = xml1.Descendants("game")
                                        .FirstOrDefault(e => (int)e.Element("steamappid") == game.SteamAppid);
                    if (gameElement != null)
                    {
                        gameElement.Element("path_directory").Value = dialog.FileName;
                        gameElement.Element("path").Value = dialog.FileName + "\\" + exename;
                        xml1.Save(xml);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void bt_Backup_Click(object sender, RoutedEventArgs e)
        {
            string destination = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\backups\\" + game.Title;

            CopyDirectory(game.Path_Directory, destination, true);
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = System.IO.Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = System.IO.Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
