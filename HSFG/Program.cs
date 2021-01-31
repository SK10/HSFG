using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace HSFG
{
    class Program
    {
        static void Main(string[] args)
        {
            int Installed;
            string csgoFolder = "";
            string fortniteDL = "https://github.com/Franc1sco/Fortnite-Emotes-Extended/archive/master.zip";
            string saySoundsDL = "";
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string downloadedFileName = "";
            int startVal;
            var originalColor = Console.ForegroundColor;
            string gistURL = "";

            checkWhatToInstall();  

            // check if csgo is installed 
            Console.Write("# Trying to check if CSGO is installed ");
            try
            {
                using (RegistryKey steamInstallCheck = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam\Apps\730"))
                {
                    if (steamInstallCheck != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write('\u2713' + Environment.NewLine);
                        Console.ForegroundColor = originalColor;
                        Object steamInstallCheckKeyVal = steamInstallCheck.GetValue("Installed");
                        Installed = (int)steamInstallCheckKeyVal;
                    }
                    else
                    {
                        Console.WriteLine("# CSGO is not installed on this machine");
                        exitApp();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.Write("# Trying to find CSGO installation folder ");
            try
            {
                using (RegistryKey csgoFolderKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 730"))
                {
                    if (csgoFolderKey != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write('\u2713' + Environment.NewLine);
                        Console.ForegroundColor = originalColor;                  
                        Object csgoFolderKeyVal = csgoFolderKey.GetValue("InstallLocation");
                        csgoFolder = csgoFolderKeyVal + @"\csgo\";
                        Console.WriteLine("# CSGO installation folder is: " + csgoFolder);
                    }
                    else
                    {
                        Console.WriteLine(Environment.NewLine + "# CSGO Installation folder was not found");
                        Console.WriteLine("# You need to enter CSGO installation folder path manually");
                        Console.WriteLine("# Opening a youtube tutorial on how to get CSGO folder path");
                        System.Threading.Thread.Sleep(3000);
                        System.Diagnostics.Process.Start("https://www.youtube-nocookie.com/embed/iYdKWi17ZBk");
                        Console.Write("# Enter CSGO path: ");
                        csgoFolder = Console.ReadLine() + @"\csgo\";
                        Console.WriteLine("# CSGO installation folder is: " + csgoFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // Getting the saysounds download file link
            Console.Write("# Getting the saysounds download link ");
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string webData = wc.DownloadString(gistURL);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('\u2713' + Environment.NewLine);
                    Console.ForegroundColor = originalColor;
                    Console.WriteLine("# Sayounds download link is: " + webData);
                    saySoundsDL = webData;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("# couldn't get the saysounds download link, Check your internet connection");
                Console.WriteLine(e.Message);
                exitApp();
            }

            Console.WriteLine("# Fortnite emotes plugin download link is: " + fortniteDL);

            DownloadFiles(saySoundsDL, "saysounds.zip");

            // Downloading Files
            void DownloadFiles(string link, string fileName)
            {
                try
                {
                    using (WebClient wcds = new WebClient())
                    {
                        wcds.Credentials = CredentialCache.DefaultNetworkCredentials;
                        wcds.DownloadFileAsync(new Uri(link), fileName);
                        wcds.DownloadFileCompleted += wcds_DownloadCompleted;
                        wcds.DownloadProgressChanged += wcds_DownloadProgressChanged;
                        GetFilenameFromWebServer(link);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("# couldn't download files, Check your internet connection");
                    Console.WriteLine(e.Message);
                    exitApp();
                }
            }

            void wcds_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Console.Write("\r" + "# Downloading " + downloadedFileName + " > Downloaded " + e.BytesReceived / 1048576 + "MB");
            }

            void wcds_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
            {
                Console.WriteLine(Environment.NewLine + "# " + downloadedFileName + " Finished Downloading");
                if (downloadedFileName == "saysounds.zip")
                {
                    if (DirExists(csgoFolder + @"sound\misc\saysounds"))
                    {
                        Console.Write("# Deleting old saysounds folder ");
                        Directory.Delete(csgoFolder + @"sound\misc\saysounds", true);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write('\u2713' + Environment.NewLine);
                        Console.ForegroundColor = originalColor;
                    }

                    Console.Write("# Extracting the saysounds ");
                    UnZipFiles(downloadedFileName, @"misc\", csgoFolder + @"\sound\");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('\u2713' + Environment.NewLine);
                    Console.ForegroundColor = originalColor;
                    File.Delete(currentPath + "saysounds.zip");
                }

                if (startVal == 1)
                {
                    if (!File.Exists(currentPath + "master.zip"))
                    {
                        DownloadFiles(fortniteDL, "master.zip");
                    }

                    if (File.Exists(currentPath + "master.zip"))
                    {
                        long length = new FileInfo(currentPath + "master.zip").Length / 1048576;
                        if (length > 5)
                        {
                            Console.Write("# Extracting master.zip ");
                            UnZipFiles("master.zip", @"\", currentPath);
                        }
                    }
                }
                else if (startVal == 2)
                {
                    Console.WriteLine("# saysounds should be updated. HF");
                    exitApp();
                }
            }

            void GetFilenameFromWebServer(string url)
            {
                var req = WebRequest.Create(url);
                req.Method = "HEAD";
                using (WebResponse resp = req.GetResponse())
                {
                    if (!string.IsNullOrEmpty(resp.Headers["Content-Disposition"]))
                    {
                        downloadedFileName = resp.Headers["Content-Disposition"].Substring(resp.Headers["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                    }
                }
            }

            void UnZipFiles(string zipFileName, string zipFileFolder, string exractPath)
            {
                string zipPath = currentPath + @"\" + zipFileName;
                string extractPath = exractPath + zipFileFolder;
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                if (DirExists(currentPath + "Fortnite-Emotes-Extended-master"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('\u2713' + Environment.NewLine);
                    Console.ForegroundColor = originalColor;
                    Console.Write("# Moving fortnite model files ");
                    MoveFiles(currentPath + @"Fortnite-Emotes-Extended-master\models\player\custom_player\kodua\", csgoFolder + @"models\player\custom_player\kodua\");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('\u2713' + Environment.NewLine);
                    Console.ForegroundColor = originalColor;
                    Console.Write("# Moving fortnite sound files ");
                    MoveFiles(currentPath + @"Fortnite-Emotes-Extended-master\sound\kodua\fortnite_emotes\", csgoFolder + @"sound\kodua\fortnite_emotes\");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write('\u2713' + Environment.NewLine);
                    Console.ForegroundColor = originalColor;
                    Directory.Delete(currentPath + "Fortnite-Emotes-Extended-master", true);
                    File.Delete(currentPath + "master.zip");
                    Console.WriteLine("All server files should be installed now. HF");
                    exitApp();
                }
            }

            void MoveFiles(string source, string dest)
            {
                try
                {
                    if (!DirExists(csgoFolder + @"models\player\custom_player\kodua\"))
                    {
                        Directory.CreateDirectory(csgoFolder + @"models\player\custom_player\kodua\");
                    }

                    if (!DirExists(csgoFolder + @"sound\kodua\fortnite_emotes\"))
                    {
                        Directory.CreateDirectory(csgoFolder + @"sound\kodua\fortnite_emotes\");
                    }

                    if (DirExists(source))
                    {
                        string[] files = Directory.GetFiles(source);

                        foreach (string s in files)
                        {
                            File.Copy(s, dest + Path.GetFileName(s), true);
                        }
                    }
                    else
                    {
                        Console.WriteLine("# Ops, something went wrong with moving the fortnite emotes plugin files");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("# Ops, something went wrong : " + e.Message);
                }
            }

            void checkWhatToInstall()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("===========================");
                Console.WriteLine("  HOM SERVER FILE GRABBER  ");
                Console.WriteLine("===========================");
                Console.ForegroundColor = originalColor;
                Console.WriteLine("# Enter [1] to install all server files");
                Console.WriteLine("# Enter [2] to update saysounds only");
                Console.Write("# Your input: ");
                startVal = Convert.ToInt32(Console.ReadLine());
                if (startVal != 1 && startVal != 2)
                {
                    Console.WriteLine("# Wrong input, Press Enter to continue");
                    Console.ReadLine();
                    Console.Clear();
                    checkWhatToInstall();
                }
            }

            void exitApp()
            {
                Console.WriteLine("# Press ENTER to exit");
                Console.ReadLine();
                Environment.Exit(0);
            }

            bool DirExists(string dirName)
            {
                return Directory.Exists(dirName);
            }
            Console.ReadLine();
        }
    }
}
