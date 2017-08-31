using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;

namespace srv_kon
{
    class Program
    {
        //do npisania poprawne zakonczenie polaczenia po naglym wyjsciu z klienta
        //sprawdzic czy to na gorze ciagle akutalne i dlaczego wysyla mi klika razy z powrotem na tego samego klienta zamiast do wszystkich z listy ciagle potrzebuje opowiedzi na to pytanie
        public static TcpListener server;
        [ThreadStatic]
        public static TcpClient client;
        [ThreadStatic]
        public static NetworkStream stream;
        public static List<TcpClient> clientsList = new List<TcpClient>();
        public static List<Task> taskList = new List<Task>();
        public static Byte[] bytes = new Byte[10000];
        

        public static void Main()
        {
            //List<DateTime> dd = new List<DateTime>();
            //Console.WriteLine(DateTime.Now);

            try
            {
                Int32 port = 13000;
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                IPAddress[] ipv4Addresses = Array.FindAll(
                    Dns.GetHostEntry(string.Empty).AddressList,
                    a => a.AddressFamily == AddressFamily.InterNetwork);

                Console.WriteLine("Server's IP: " + ipv4Addresses[0]);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");
                    //dopoki nie zostanie poproszony o dostep do portu to czeka
                    client = server.AcceptTcpClient();
                    Console.WriteLine("Connected");
                    if (!clientsList.Contains(client))
                    {
                        lock (client)
                        {
                            client.Client.SendTimeout = 50000;
                            clientsList.Add(client);
                            //kazdy klient dostaje wlasny watek do obslugi
                            Task t = new Task((e) =>
                            {
                                TalkWithClient((TcpClient)e);
                            }, client);
                            t.Start();
                            Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
        //jako arguemnt przyjmuje klienta
        public static void TalkWithClient(TcpClient clientTemp)
        {
            NetworkStream stream2;
            string data;
            
            while (true)
            {
                if (clientTemp.Connected)
                {
                    stream2 = clientTemp.GetStream();
                }
                else
                {
                    clientsList.Remove(clientTemp);
                }
                try
                {
                    data = "";
                    while (stream == null)
                    {
                        stream = clientTemp.GetStream();
                    }
                    lock (stream)
                    {
                        //klienci nasluchuja rownolegle korzystajac z jednego strumienia na zmiane go redefiniujac
                        //jesli sie cos pojawi to od razu to wysylaja to wszystkich klientow poza soba samym
                        int i = stream.Read(bytes, 0, bytes.Length);
                        while (i != 0)  //gdy pojawi sie jakas wiadomosc
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            //obsluga zapytania o logi
                            if (XMLLogs.MatchPatternXML(data))
                            {
                                //TO TRZEBA POPRAWIC
                                Console.WriteLine("Recived a query " + data);
                                XMLLogs.ReturnDatesXML(data);
                                //zapisywanie danych do xml
                                Tuple<DateTime, string> t = new Tuple<DateTime, string>(DateTime.Now, data);
                                MessageLog temp = new MessageLog(t.Item1, t.Item2);
                                XMLLogs.mLogs.Add(temp);
                                //testowa wartosc
                                XMLLogs.SerializeXML(XMLLogs.mLogs); //System.InvalidOperationException
                                foreach (var item in XMLLogs.mLogs)
                                {
                                    Console.WriteLine(item);
                                }
                                //koniec zapisywania danych do xml
                            }
                            else
                            {
                                data = "ID: " + clientTemp.Client.RemoteEndPoint + ": " + data;
                                Console.WriteLine("Recived " + data);

                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                                lock (clientsList)
                                {
                                    try
                                    {
                                        foreach (var clients in clientsList)
                                        {
                                            try
                                            {
                                                stream2 = clients.GetStream();
                                                stream2.Write(msg, 0, msg.Length);
                                                Console.WriteLine("Sent to {1}: {0}", data, clients.Client.RemoteEndPoint.ToString());
                                            }
                                            catch (InvalidOperationException e)
                                            {
                                                Console.WriteLine(e.Message.ToString());
                                            }
                                        }
                                    }
                                    catch (InvalidOperationException)
                                    {
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                            }//tutaj zakonczyc else?
                            i = stream.Read(bytes, 0, bytes.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

        }
    }
}