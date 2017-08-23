using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
class Program
{
    public static void Main()
    {
        //https://blogs.technet.microsoft.com/marcelofartura/2006/11/03/real-case-net-apps-no-connection-could-be-made-because-the-target-machine-actively-refused-it-basic/
        TcpListener server = null;
        List<TcpClient> clientsList = new List<TcpClient>();
        List<Task> taskList = new List<Task>();
        try
        {
            // Set the TcpListener on port 13000.
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            //List < TcpClient > clientsList= new List<TcpClient>();

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                //ustanawiam placzenie

                TcpClient client = server.AcceptTcpClient();
                //dodaje go do listy subskrybentow
                if (!clientsList.Contains(client)) { clientsList.Add(client); }
                //Console.WriteLine(client.ToString());
                Console.WriteLine("Connected!");

                Action<TcpClient> KeepListening = (myClient) =>
                {
                    while (myClient.Connected)
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
                                        Console.WriteLine("Sent: {0}", data);
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    //Console.WriteLine(ex.ToString());
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
                Task<TcpClient> x = new Task<TcpClient>(KeepListening);
                Task<TcpClient> xx =(client);
                lock ((object)taskList)
                {
                    //taskList.Add(new Task<TcpClient>(clientsList[0]));
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
    public static void DoInThread(object param)
    {

    }
}