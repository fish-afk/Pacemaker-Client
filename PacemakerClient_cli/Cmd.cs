using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacemakerClient
{
    public class Cmd
    {
        public string Command { get; set; }
        public string TimeStamp { get; set; }
        public UInt64 CmdLength { get; set; }
        public string CommandId { get; set; }


        public string DecryptCmd(string cmd)
        {
            // to be implemented
            return cmd;

        }
    }
}
