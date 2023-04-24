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
            Loaded += MyWindow_Loaded;
            this.game = game;
            this.library = library;
        }
        

        private async void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await GetTotalBytesOnDiskAsync();
        }

        public static async Task<string> downloadManifest(string username, string password, int appid, string Path = null)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "D:\\Downloads\\tulip\\downloader\\DepotDownloader.exe",
                Arguments = $"-app 1326470 -manifest-only -username {MainWindow.User.Username} -password {MainWindow.User.Password}{(Path != null ? $" -dir \"{Path}\"" : $" -dir \"{AppContext.BaseDirectory}\\manifest\"")}",
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = processInfo };
            process.Start();
            process.BeginOutputReadLine();
            process.StandardInput.WriteLine("rpkck");

            await process.WaitForExitAsync();
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
                tb_Location.Text = dialog.FileName;
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

        private async Task WaitForButtonClickAsync()
        {
            // Use TaskCompletionSource to create a task that completes when the button is clicked
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Attach the event handler to the button click event
            bt_Crack.Click += (sender, e) =>
            {
                // Complete the task when the button is clicked
                Library libraryPage = System.Windows.Application.Current.MainWindow.Content as Library;
                libraryPage?.downloadGame(game.SteamAppid, tb_Location.Text,Convert.ToInt64(tb_DiskRequired.Text));

                tcs.SetResult(true);
            };

            // Wait for the task to complete
            await tcs.Task;
        }

        public async void bt_Crack_Click(object sender, RoutedEventArgs e)
        {
            //DownloadReleaseAsset("SteamRE", "DepotDownloader");
            this.Close();
            library.downloadGame(game.SteamAppid, tb_Location.Text, totalBytesOnDisk);


            
        }

        private long totalBytesOnDisk;

        public async Task GetTotalBytesOnDiskAsync()
        {
            string manifestFilePath = await downloadManifest(MainWindow.User.Username, MainWindow.User.Password,game.SteamAppid);
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
                            this.totalBytesOnDisk = totalBytesOnDisk;
                            tb_DiskRequired.Text = Util.FormatBytes(totalBytesOnDisk);
                        }
                    }
                }
            }
        }


    }
}
