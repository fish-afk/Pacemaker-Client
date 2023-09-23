using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacemakerClient
{
    public class CmdResult
    {
        public string Result { get; set; }  
        public UInt64 ResultSize { get; set; }
        public string CommandId { get; set; }

        public string EncryptResult(string Result)
        {
            // to be implemented
            return Result;

        }

    }
}
