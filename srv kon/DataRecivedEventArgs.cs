using System;


    public class DataRecivedEventArgs: EventArgs
    {
        public byte[] message { get; set; }
        public DataRecivedEventArgs(byte[] s)
        {
            message = s;
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