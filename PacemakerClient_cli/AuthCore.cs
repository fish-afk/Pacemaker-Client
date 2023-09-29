using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;

namespace PacemakerClient_cli
{
    public class refreshResponseJson
    {
        public string freshJwt { get; set; }
        public bool status { get; set; }
    }

    [Serializable()]
    public class AuthCore : ISerializable
    {
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        public AuthCore(SerializationInfo info, StreamingContext context) // for constructing deserialized objects out of a file...
        {
            RefreshToken = (string)info.GetValue("RefreshToken", typeof(string));
            Username = (string)info.GetValue("Username", typeof(string));
            JwtToken = (string)info.GetValue("JwtToken", typeof(string));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)  // for serializing objects into a stream of bytes for file storage..
        {                                                                                    // this may be overridden later for different types of users...
            info.AddValue("RefreshToken", RefreshToken);
            info.AddValue("Username", Username);
            info.AddValue("JwtToken", JwtToken);
        }
        public AuthCore() { }


        public async Task<bool> refresh()
        {
            Console.WriteLine("Hit");
            HttpClient Client = new HttpClient();

            string jsonData = "{\"username\": \"" + Username + "\", \"refreshToken\": \"" + RefreshToken + "\"}";

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync(PacemakerClient_cli.Program.server_hostname + "/core/refresh", content);

            refreshResponseJson refreshResponse;

            string jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(jsonString);
            if (response.IsSuccessStatusCode)
            {
                
                refreshResponse = JsonConvert.DeserializeObject<refreshResponseJson>(jsonString);


                if (refreshResponse.status == true && refreshResponse.freshJwt.Length > 0)
                {
                    JwtToken = refreshResponse.freshJwt;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

    }
}
