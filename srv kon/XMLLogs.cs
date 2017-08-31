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
        [XmlIgnore]
        public static string logAddress = @"C:\Users\Polgard-PC\Desktop\New folder (2)\srv kon\srv kon\Logs.xml";
        [ThreadStatic][XmlIgnore]
        public static string fromDate;
        [ThreadStatic][XmlIgnore]
        public static string toDate;
        public static void Serialize()
        {
            using (TextWriter tw = new StreamWriter(logAddress))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<Tuple<DateTime, string>>));
                xs.Serialize(tw, logAddress);
            }
        }
        //queryLogs = "$MsG_"+from.ToString()+"_"+to.ToString()+"$";
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

