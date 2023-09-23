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

namespace PacemakerClient
{
    public partial class Service1 : ServiceBase
    {

        public static string server_hostname = "http://localhost:3000";

        public Service1()
        {
            InitializeComponent();
        }


        public async static void InitialHandshake()
        {
            HttpClient Client = new HttpClient();

            string json = await Client.GetStringAsync(server_hostname + "/initialhandshake");

            string fullPath = Environment.CurrentDirectory + "\\log.txt";

            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(json.ToString());
            }

        }

        public async static void KillSwitch()
        {
            HttpClient Client = new HttpClient();

            var json = await Client.DeleteAsync(server_hostname + "/killswitch");

            string fullPath = Environment.CurrentDirectory + "\\killed.txt";

            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteLine(json.ToString());
            }

        }

        protected override void OnStart(string[] args)
        {
            InitialHandshake();
        }

        protected override void OnStop()
        {
            KillSwitch();
        }
    }
}
