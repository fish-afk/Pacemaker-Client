using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacemakerStager
{
    internal class B64
    {
        public static string b64Encode(string str)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(str);
            var b64 = System.Convert.ToBase64String(textBytes);

            return b64;

        }

        public static string b64Decode(string b64)
        {
            var base64Bytes = System.Convert.FromBase64String(b64);
            var str = System.Text.Encoding.UTF8.GetString(base64Bytes);
            return str;
        }
    }
}
