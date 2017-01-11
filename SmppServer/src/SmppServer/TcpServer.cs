using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SmsLibrary;
namespace SmppServer
{
    public class TcpHelper
    {
        private static TcpListener listener { get; set; }
        private static bool accept { get; set; } = false;
        public static void StartServer(string ip,int port)
        {
            IPAddress address = IPAddress.Parse(ip);
            listener = new TcpListener(address, port);

            listener.Start();
            accept = true;

   
        }

        public static void Listen()
        {
            if (listener != null && accept)
            {

                // Continue listening.  
                while (true)
                {
                    Console.WriteLine("Waiting for client...");
                    var clientTask = listener.AcceptTcpClientAsync(); // Get the client  

                    if (clientTask.Result != null)
                    {
                        Console.WriteLine("Client connected. Waiting for data.");
                        var client = clientTask.Result;
                        string message = "";

                        while (message != null)
                        {
                            try
                            {
                                byte[] data = Encoding.ASCII.GetBytes("Send next data: [enter 'quit' to terminate] ");
                                client.GetStream().Write(data, 0, data.Length);

                                byte[] buffer = new byte[1024];
                                client.GetStream().Read(buffer, 0, buffer.Length);

                                message = Encoding.ASCII.GetString(buffer);
                                string pduSource =      PduBitPacker.ConvertBytesToHex(buffer);
                                SMSType smsType = SMSBase.GetSMSType(pduSource);
                                Console.WriteLine(pduSource);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                
                            }
                       
                        }
                        Console.WriteLine("Closing connection.");
                        client.GetStream().Dispose();
                    }
                }
            }
        }
    }
}

