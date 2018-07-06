// 2018-07-06 - test code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PJServerTest
{
    class Program
    {
        static int AnswerLength = -1;
        static bool AsReceiveData = false;
        static ManualResetEvent EndReadEvent;

        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("172.16.104.5", 4352);
            byte[] Message = Encoding.ASCII.GetBytes("%1POWR 1\r");
            client.GetStream().Write(Message, 0, Message.Length);
            byte[] Answer = new byte[255];
            EndReadEvent = new ManualResetEvent(false);
            client.GetStream().BeginRead(Answer, 0, Answer.Length, new AsyncCallback(ReceiveData), client);
            if (EndReadEvent.WaitOne(1000))
            {
                if (AsReceiveData)
                {
                    Console.WriteLine(Encoding.ASCII.GetString(Answer, 0, AnswerLength));
                }
            }
            Console.WriteLine("Fine");
            Console.ReadLine();
         }

        static void ReceiveData(IAsyncResult ar)
        {
            try
            {
                TcpClient MyClient = ar.AsyncState as TcpClient;
                AnswerLength = MyClient.GetStream().EndRead(ar);
                AsReceiveData = true;
            }
            catch
            {
                AsReceiveData = false;
            }

            EndReadEvent.Set();
        }
    }
}
