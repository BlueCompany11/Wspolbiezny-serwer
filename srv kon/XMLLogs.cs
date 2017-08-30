using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace srv_kon
{
    [Serializable]
    public class XMLLogs
    {
        public static List<Tuple<DateTime, string>> Logs = new List<Tuple<DateTime, string>>();
        public static string logAddress = @"C:\Users\Polgard-PC\Desktop\New folder (2)\srv kon\srv kon\Logs.xml";
        public static void Serialize()
        {
            using (TextWriter tw = new StreamWriter(logAddress))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<Tuple<DateTime,string>>));
                xs.Serialize(tw,logAddress);
            }
        }
    }
}
