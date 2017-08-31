using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace srv_kon
{   [Serializable]
    public class MessageLog:XMLLogs
    {
        DateTime date;
        string message;
        public MessageLog()
        {

        }
        public MessageLog(DateTime x,string z)
        {
            date = x;
            message = z;
        }
    }
}
