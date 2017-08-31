using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace srv_kon
{

    public class XMLLogs
    {

        //public static List<Tuple<DateTime, string>> Logs = new List<Tuple<DateTime, string>>();
        public static List<MessageLog> mLogs = new List<MessageLog>();
        public static string logAddress = @"C:\Users\Polgard-PC\Desktop\New folder (2)\srv kon\srv kon\Logs.xml";
        [ThreadStatic]
        public static string fromDate;
        [ThreadStatic]
        public static string toDate;

        public DateTime dateXml;
        public string messageXml;
        public XMLLogs()
        {

        }
        public XMLLogs(Tuple<DateTime,string> x)
        {
            dateXml = x.Item1;
            messageXml = x.Item2;
        }

        public static void SerializeXML(List<MessageLog> mlist)
        {
            using (TextWriter tw = new StreamWriter(logAddress))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<MessageLog>));
                xs.Serialize(tw, logAddress);
            }
        }

        public static bool MatchPatternXML(string message)
        {
            string compareData = "$MsG_";
            for (int i = 0; i < compareData.Length; i++)
            {
                if (!(compareData[i] == message[i]))
                    return false;
            }
            if (!(message[message.Length - 1] == '$'))
                return false;

            return true;
        }

        public static void ReturnDatesXML(string message)
        {
            string fromString = "";
            string toString = "";
            for (int i = 5; i < message.Length; i++)
            {
                if (message[i] == '_')
                {
                    for (int j = i; j < message.Length; j++)
                    {
                        if (message[j] != '$')
                        {
                            toString += message[j];
                            i++;
                            continue;
                        }
                        i++;
                        break;
                    }
                }
                fromString += message[i];
            }
            fromDate = fromString;
            toDate = toString;
        }

    }
}

