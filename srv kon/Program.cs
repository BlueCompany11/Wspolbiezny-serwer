using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
class Program
{
    public static TcpListener server = null;
    public static TcpClient client = null;
    public static void Main()
    {

        List<TcpClient> clientsList = new List<TcpClient>();
        List<Task> taskList = new List<Task>();
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;

            server = new TcpListener(IPAddress.Any, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            //List < TcpClient > clientsList= new List<TcpClient>();
            //ustanawiam placzenie

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected");
                //dodaje go do listy subskrybentow
                //zle dzialao bo klienty roznia sie numerami socketow,a  przez to samymi socketami, za kazdym razem bedzie nadany inny, nawet z tego samego konta
                //trzeba sprawdzac po ip
                if (!clientsList.Contains(client)) { clientsList.Add(client); client.Client.SendTimeout = 1000; }

                //wczensije sie co chwile laczylem, teraz musz jakos utrzymac polaczenie
                Action a = () =>
                {
                    while (true)
                    {
                        try
                        {
                            data = "";
                            NetworkStream stream = client.GetStream();

                            int i;
                            //w osobnych watkach wysylam im wiadomosci 1 watek na kazdego klienta
                            // Loop to receive all the data sent by the client.

                            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                // Translate data bytes to a ASCII string.
                                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                                Console.WriteLine("Received: {0}", data);

                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                                try
                                {
                                    foreach (var clients in clientsList)
                                    {
                                        NetworkStream stream2 = clients.GetStream();
                                        // Send back a response.

                                        stream2.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent to {1}: {0}", data, clients.Client.AddressFamily.ToString());
                                        Console.WriteLine("Amount fo clients: {0}", clientsList.Count);
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
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                };

                lock ((object)taskList)
                {
                    taskList.Add(new Task(a));
                    foreach (var elem in taskList)
                    {
                        try
                        {
                            if (elem.Status != TaskStatus.Running)
                            {
                                elem.Start();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
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

}
//teoretyczna funkcja do sprawdzania czy client jest juz na liscie
//int counter = 0;
//if (client != null)
//{
//    foreach (var elem in clientsList)
//    {
//        //if ((elem.Client.LocalEndPoint.AddressFamily == client.Client.LocalEndPoint.AddressFamily))
//        //{
//        //    counter++;
//        //}
//        if ((elem.Client.RemoteEndPoint.AddressFamily == client.Client.RemoteEndPoint.AddressFamily))
//        {
//            counter++;
//        }
//    }
//    if (counter == 0)
//    {
//        clientsList.Add(client);
//    }
//}
//Console.WriteLine(client.ToString());