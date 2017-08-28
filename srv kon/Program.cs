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
    public static TcpClient client;
    public static NetworkStream stream2;
    public static NetworkStream stream;
    public static void Nasluchiwacz()
    {
        if (client != null)
        {
            Console.WriteLine("User is connected: {0}", client.Client.Connected);
        }
    }
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
            //Thread asd = new Thread(Nasluchiwacz);
            //asd.Start();
            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data = null;
            //List < TcpClient > clientsList= new List<TcpClient>();
            //ustanawiam placzenie

            // Enter the listening loop.
            while (true)
            {
                Console.Write("Waiting for a connection... ");
                //dopoki nie zostanie poproszony o dostep do portu to czeka
                client = server.AcceptTcpClient();
                Console.WriteLine("Connected");
                //tutaj zaczalem
                //stream = client.GetStream();
                //string x = "halo";
                //bytes = System.Text.Encoding.ASCII.GetBytes(x);
                //// Send the message to the connected TcpServer. 
                //stream.Write(bytes, 0,bytes.Length);
                //tutaj skonczylem
                //dodaje go do listy subskrybentow
                //zle dzialao bo klienty roznia sie numerami socketow,a  przez to samymi socketami, za kazdym razem bedzie nadany inny, nawet z tego samego konta
                //trzeba sprawdzac po ip
                //czekanie na wiadomosc od klienta do 5 sek
                if (!clientsList.Contains(client)) { client.Client.SendTimeout = 50000; clientsList.Add(client); }

                //wczensije sie co chwile laczylem, teraz musz jakos utrzymac polaczenie
                Action a = () =>
                {
                    while (true)
                    {
                        try
                        {
                            data = "";
                            //stream od klienta z wiadomoscia
                            stream = client.GetStream();
                            //nie jest potrzebny while read bo jesli wiadomosc nie przekracza 1440 bitow to read pochlonie to za jednym razem jbc to dopisac vollitale? by wykonalo sie to jako operacja atomowa?
                            int i;
                            ////w osobnych watkach wysylam im wiadomosci 1 watek na kazdego klienta

                            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)  //gdy pojawi sie jakas wiadomosc
                            {
                                // Translate data bytes to a ASCII string.
                                data = System.Text.Encoding.ASCII.GetString(bytes, 0, stream.Read(bytes, 0, bytes.Length));
                                Console.WriteLine("Received: {0}", data);
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                                lock (clientsList)
                                {
                                    try
                                    {
                                        //Console.WriteLine("Wchodze do clientslist");
                                        foreach (var clients in clientsList)
                                        {
                                           // Console.WriteLine("moj remotesocketport to {0}", clients.Client.RemoteEndPoint.ToString());
                                            //if (SocketConnected(clients.Client))
                                            //{
                                            try
                                            {
                                                stream2 = clients.GetStream();
                                                Console.WriteLine(stream2.ToString());
                                                stream2.Write(msg, 0, msg.Length);
                                                Console.WriteLine("Sent to {1}: {0}", data, clients.Client.RemoteEndPoint.ToString());
                                                //Console.WriteLine("Is he connected? {0}", clients.Client.Connected);
                                                //Console.WriteLine("Amount fo clients: {0}", clientsList.Count);
                                                //}
                                                //Console.WriteLine("Proba przesjcia na okolo");
                                                //for (int j = 0; j < clientsList.Count; ++j)
                                                //{
                                                //    if (clients != clientsList[j])
                                                //    {

                                                //        clients.Client.SendTo(msg, clientsList[j].Client.LocalEndPoint);
                                                //        Console.WriteLine("Wyslano przed chwila wiadomosc");
                                                //    }
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
                                    //
                                    //for (int j = 0; j < clientsList.Count; j++)
                                    //{
                                    //    for (int k = 0; k < clientsList.Count; k++)
                                    //    {
                                    //        if (clientsList[k] != clientsList[j])
                                    //        {
                                    //            clientsList[k].Client.SendTo(msg, clientsList[j].Client.RemoteEndPoint);
                                    //        }
                                    //    }
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                };
                //uruchamianie wszystkich klientow w osobnych zadaniach
                lock (taskList)
                {
                    taskList.Add(new Task(a));
                    foreach (var elem in taskList)
                    {
                        try
                        {
                            if (elem.Status != TaskStatus.Running || elem.Status != TaskStatus.RanToCompletion)
                            {
                                elem.Start();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                //badanie zywotnosci klientow
                lock (clientsList)
                {
                    for (int i = 0; i < clientsList.Count; i++)
                    {
                        //jesli rozlaczony to usun go z listy
                        if (!SocketConnected(clientsList[i].Client))
                        {
                            clientsList[i].Client.Disconnect(true); //jak jest 1 klient to jego disconect powoduje wylaczenie serwera?
                            clientsList[i].Client.Close();

                            clientsList.Remove(clientsList[i]);

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