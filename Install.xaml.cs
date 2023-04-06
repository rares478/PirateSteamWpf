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

        private MainWindow mainWindow;
        private Game game;

        public Install(MainWindow main,Game game)
        {
            InitializeComponent();
            Loaded += MyWindow_Loaded;
            mainWindow = main;
            this.game = game;
        }
        

        private async void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await GetTotalBytesOnDiskAsync("rares478", "Paralelipipedut12.", 1326470);
        }

        public static async Task<string> downloadManifest(string username, string password, int appid, string Path = null)
        {
            /*var processInfo = new ProcessStartInfo
            {
                FileName = "D:\\Downloads\\tulip\\downloader\\DepotDownloader.exe",
                Arguments = $"-app {appid} -manifest-only -username {username} -password {password}{(Path != null ? $" -dir \"{Path}\"" : $" -dir \"{AppContext.BaseDirectory}\\manifest\"")}",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processInfo };
            process.Start();
            process.BeginOutputReadLine();
            process.StandardInput.WriteLine("8c6w4");

            await process.WaitForExitAsync();*/
            if (Path != null)
            {
                return Directory.GetFiles(Path, "*.txt", SearchOption.AllDirectories).FirstOrDefault();
            }
            else
            {
                return Directory.GetFiles(AppContext.BaseDirectory + "\\manifest", "*.txt", SearchOption.AllDirectories).FirstOrDefault();
            }
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
                string dir = Path.GetPathRoot(InstallPath);
                long size = 0;

                DriveInfo driveinfo = new DriveInfo(dir);

                long availableSpaceInBytes = driveinfo.AvailableFreeSpace;
                tb_DiskAvailable.Text = Util.FormatBytes(availableSpaceInBytes);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task DownloadReleaseAsset(string owner, string repo)
        {
            var client = new GitHubClient(new ProductHeaderValue("DepotDownloader"));
            var release = await client.Repository.Release.GetLatest(owner, repo);

            foreach (var asset in release.Assets)
            {
                var downloadUrl = asset.BrowserDownloadUrl;
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(downloadUrl);
                var contentStream = await response.Content.ReadAsStreamAsync();
                var path = @"DepotDownloader";
                var zipArchive = new ZipArchive(contentStream);

                foreach (var entry in zipArchive.Entries)
                {
                    var fullName = Path.Combine(path, entry.FullName);

                    if (entry.Name == "")
                    {
                        // Create directory if the entry is a directory
                        Directory.CreateDirectory(fullName);
                    }
                    else
                    {
                        // Extract file if the entry is a file
                        entry.ExtractToFile(fullName, true);
                    }
                }

                return;
            }

            throw new Exception($"Could not find asset");
        }

        private async void bt_Crack_Click(object sender, RoutedEventArgs e)
        {
            //DownloadReleaseAsset("SteamRE", "DepotDownloader");

            this.Close();
        }


        public async Task GetTotalBytesOnDiskAsync(string username, string password,int appid)
        {
            string manifestFilePath = await downloadManifest(username, password,appid);
            tb_EstTime.Text = manifestFilePath;

            if (!string.IsNullOrEmpty(manifestFilePath))
            {
                string[] fileContents = System.IO.File.ReadAllLines(manifestFilePath);

                foreach (string line in fileContents)
                {
                    if (line.StartsWith("Total bytes on disk"))
                    {
                        string[] parts = line.Split(':');
                        if (parts.Length == 2 && long.TryParse(parts[1].Trim(), out long totalBytesOnDisk))
                        {
                            tb_DiskRequired.Text = Util.FormatBytes(totalBytesOnDisk);
                        }
                    }
                }
            }
        }


    }
}
