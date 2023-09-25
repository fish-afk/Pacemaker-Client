using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacemakerClient
{
    public class CmdResult
    {
        public string result { get; set; }  
        public string username { get; set; }
        public string commandId { get; set; }
        public string jwt_key { get; set; }

        public bool EncryptResult(string Result)
        {
            // to be implemented
            return true;

        }

    }
}
