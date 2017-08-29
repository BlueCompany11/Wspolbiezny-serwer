using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Program
{
    public class TcpListenerSample
    {
        public static TcpClient client;
        public static int port = 13000;
        public static TcpListener server = new TcpListener(IPAddress.Any, port);
        public static byte[] bytes = new byte[1024];
        public static string data;
        public static List<TcpClient> clientsList = new List<TcpClient>();
        static void Main(string[] args)
        {
            Action CheckIfConnectedAction = () =>
            {
                while (true)
                {
                    //lock (clientsList)
                    //{
                    //    foreach (var item in clientsList)
                    //    {
                    //        if (item.Client.Connected)
                    //        {
                    //            Console.WriteLine(item.Client.RemoteEndPoint.ToString() + "Connected");
                    //        }
                    //        else
                    //        {
                    //            Console.WriteLine(item.Client.RemoteEndPoint.ToString() + "Disconnected");
                    //        }
                    //    }
                    //}
                    for (int i = 0; i < clientsList.Count; i++)
                    {
                        if (clientsList[i].Client.Connected)
                        {
                            Console.WriteLine(clientsList[i].Client.RemoteEndPoint.ToString() + "Connected");
                        }
                        else
                        {
                            Console.WriteLine(clientsList[i].Client.RemoteEndPoint.ToString() + "Disconnected");
                        }
                    }

                }
            };
            Task CheckIfConnectedTask = new Task(CheckIfConnectedAction);
            try
            {
                server.Start();
                //Enter the listening loop
                while (true)
                {
                    try
                    {
                        if (client != null && CheckIfConnectedTask.Status != TaskStatus.Running)
                        {
                            CheckIfConnectedTask.Start();
                        }
                        Console.Write("Waiting for a connection... ");
                        client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                        if (!clientsList.Contains(client))
                        {
                            clientsList.Add(client);
                        }
                        NetworkStream stream = client.GetStream();
                        int i = stream.Read(bytes, 0, bytes.Length); 
                        while (i != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Console.WriteLine(String.Format("Received: {0}", data));
                            data = "ID " + client.Client.RemoteEndPoint.ToString() + " " + data;
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(String.Format("Sent: {0}", data));
                            //doczytywanie pozostalych danych nadeslanych w miedzyczasie
                            i = stream.Read(bytes, 0, bytes.Length);
                            //testy
                        }
                    }
                    catch(System.IO.IOException e) { }
                    // Shutdown and end connection
                    //client.Close();
                }
            }
            catch(System.IO.IOException e)
            {
                //client.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }


            Console.WriteLine("Hit enter to continue...");
            Console.Read();
        }

    }
}