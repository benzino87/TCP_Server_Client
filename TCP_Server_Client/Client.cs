using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace TCP_Server_Client
{

    /**
     * 
     * Reads all data recieved from server
     * 
     **/
    public class InFromServer
    {
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        Socket connection;

        public InFromServer(Socket connection)
        {
            this.connection = connection;
        }
        public void readFromServer()
        {
            while (true)
            {
                // Receive the response from the remote device.
                int bytesRec = connection.Receive(bytes);

                Console.WriteLine("SERVER: {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));
            }
        }
    }

    /**
     * 
     * Handles all writing out to server
     * 
     **/
    public class OutToServer
    {
        Socket connection;

        public OutToServer(Socket connection)
        {
            this.connection = connection;
        }
        public void writeToServer()
        {

            while (true)
            {
                Console.WriteLine("Enter a message");

                //Store user input to convert to bytes
                string userInput = Console.ReadLine();

                char[] message = userInput.ToCharArray();

                byte[] byteToSend = Encoding.ASCII.GetBytes(message);

                connection.Send(byteToSend);

                if (userInput == "Quit")
                {
                    connection.Close();
                }
            }
        }
    }

    /**
     * 
     * Initializes client and creats individual threads for reading and writing to server
     * 
     **/
    public class Client
    {

        public static void StartClient(string incomingIP, string incomingPort)
        {

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.

                IPAddress ipAddress = IPAddress.Parse(incomingIP);
                int portNumber = int.Parse(incomingPort);

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, portNumber);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {

                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    //Create sepereate threads for input and output
                    InFromServer inFromServer = new InFromServer(sender);
                    OutToServer outToServer = new OutToServer(sender);

                    Thread i = new Thread(new ThreadStart(inFromServer.readFromServer));
                    Thread o = new Thread(new ThreadStart(outToServer.writeToServer));

                    i.Start();
                    o.Start();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /**
         * 
         * Prompts user for desired IP address and port number
         * 
         **/
        public static void Main(String[] args)
        {
            Console.WriteLine("Enter an IP address (EX: 127.0.0.1):");
            string ipAddress = Console.ReadLine();
            Console.WriteLine("Enter a port number (EX: 9876):");
            string portNumber = Console.ReadLine();

            StartClient(ipAddress, portNumber);
        }
    }
}
