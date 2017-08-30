using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
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
    public static Byte[] bytes = new Byte[256];
    public static void Nasluchiwacz()
    {
        if (client != null)
        {
            Console.WriteLine("User is connected: {0}", client.Client.Connected);
        }
    }

    public static void Main()
    {
        try
        {
            Int32 port = 13000;

            server = new TcpListener(IPAddress.Any, port);

            server.Start();

            while (true)
            {
                Console.Write("Waiting for a connection... ");
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
                    //stream = clientTemp.GetStream();
                    //klienci nasluchuja rownolegle korzystajac z jednego strumienia na zmiane go redefiniujac
                    //jesli sie cos pojawi to od razu to wysylaja to wszystkich klientow poza soba samym
                    int i = stream.Read(bytes, 0, bytes.Length);
                    while (i != 0)  //gdy pojawi sie jakas wiadomosc
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);   //tutaj byl blad
                        data = "ID: " + clientTemp.Client.RemoteEndPoint+": "+data;
                        Console.WriteLine("Received: {0}", data);
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        lock (clientsList)
                        {
                            try
                            {
                                foreach (var clients in clientsList)
                                {
                                    try
                                    {
                                        //if (clientTemp.Client.RemoteEndPoint != clients.Client.RemoteEndPoint)    //wczesniej bylo porownywanie po samych klientach tcp
                                        //{
                                            stream2 = clients.GetStream();
                                            Console.WriteLine(stream2.ToString());
                                            stream2.Write(msg, 0, msg.Length);
                                            Console.WriteLine("Sent to {1}: {0}", data, clients.Client.RemoteEndPoint.ToString());
                                        //}
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
                        i = stream.Read(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
    //    It works like this:
    //s.Poll returns true if
    //connection is closed, reset, terminated or pending (meaning no active connection)
    //connection is active and there is data available for reading
    //s.Available returns number of bytes available for reading
    //if both are true:
    //there is no data available to read so connection is not active
    static bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(10000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if (part1 && part2)
            return false;
        else
            return true;
    }
};