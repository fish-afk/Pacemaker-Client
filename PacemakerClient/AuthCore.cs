using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Net.Http;

namespace PacemakerClient
{
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


        public string refresh()
        {
            // tba
            return "";
        }

    }
}
