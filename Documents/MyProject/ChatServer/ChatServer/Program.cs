using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;

namespace ConsoleApplication1
{
    class Program
    {
        public static Hashtable clientsList = new Hashtable();

        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            while ((true))
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();

                byte[] bytesFrom = new byte[100025];
                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                clientsList.Add(counter, clientSocket);

                broadcast(dataFromClient + " Joined ", dataFromClient, false);

                Console.WriteLine(dataFromClient + " Joined chat room ");
                handleClinet client = new handleClinet();
                client.startClient(clientSocket, dataFromClient, clientsList);
            }

            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.ReadLine();
        }

        public static void broadcast(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                try {
                    TcpClient broadcastSocket;
                    broadcastSocket = (TcpClient)Item.Value;
                    NetworkStream broadcastStream = broadcastSocket.GetStream();
                    Byte[] broadcastBytes = null;

                    if (flag == true)
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(uName + " says : " + msg);
                    }
                    else
                    {
                        broadcastBytes = Encoding.ASCII.GetBytes(msg);
                    }

                    broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                    broadcastStream.Flush();
                }
                catch(Exception e){
                    
                }
                
            }
        }  //end broadcast function
    }//end Main class


    public class handleClinet
    {
        TcpClient clientSocket;
        string clNo;
        Hashtable clientsList;
        Thread ctThread;

        public void startClient(TcpClient inClientSocket, string clineNo, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            ctThread = new Thread(() => doChat(cList));
            ctThread.Start();
        }

        private void doChat(Hashtable cList)
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[100025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;

            while ((true))
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    if (dataFromClient == "0")
                    {
                        Console.WriteLine(clNo + " exited the chat room!");
                        Program.broadcast(clNo + " exited the chat room!", clNo, true);
                        cList.Remove(clNo);
                        ctThread.Abort();
                    }
                    else {
                        Console.WriteLine("From client - " + clNo + " : " + dataFromClient);
                        rCount = Convert.ToString(requestCount);
                        Program.broadcast(dataFromClient, clNo, true);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                    if (!clientSocket.Connected)
                    {
                        Console.WriteLine(clNo + " exited the chat room!");
                        Program.broadcast(clNo + " exited the chat room!", clNo, true);
                        cList.Remove(clNo);
                        ctThread.Abort();
                        
                        break;
                    }
                }
            }//end while
        }//end doChat
    } //end class handleClinet
}//end namespace