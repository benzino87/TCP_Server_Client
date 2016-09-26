using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCP_Server_Client
{

    /**
     * 
     * Handles all incoming data from individual client
     * 
     **/
    public class InFromClient
    {
        Socket connection;
        public InFromClient(Socket connection)
        {
            this.connection = connection;
        }
        public void readFromClient()
        {
            // Incoming data from the client.
            string data = null;

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // An incoming connection needs to be processed.
            while (true)
            {
                bytes = new byte[1024];

                int bytesRec = connection.Receive(bytes);

                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data == "Quit")
                {
                    connection.Close();
                    Console.WriteLine("Client has left");
                    break;
                }

                // Show the data on the console.
                Console.WriteLine("CLIENT: {0}", data);
            }
        }
    }

    /**
     * 
     * Handles output to client who most recently sent data
     * 
     **/
    public class OutToClient
    {
        Socket connection;

        public OutToClient(Socket connection)
        {
            this.connection = connection;
        }
        public void writeToClient()
        {
            while (true)
            {
                //Store user input to convert to bytes
                string userInput = Console.ReadLine();

                char[] message = userInput.ToCharArray();

                byte[] byteToSend = Encoding.ASCII.GetBytes(message);

                connection.Send(byteToSend);

                if (userInput == "Quit")
                {
                    connection.Close();
                }
                else
                {
                    connection.Send(byteToSend);
                }

            }
        }
    }

    /**
     * 
     * Handles individual clients and multithreads input and output to 
     * send and recieve data in any order
     * 
     **/
    public class ClientHandler
    {
        private Socket client;

        public ClientHandler(Socket client)
        {
            this.client = client;
        }
        public void handleNewClient()
        {

            string verifyConnection = client.Connected ? "Client connected" : null;

            Console.WriteLine(verifyConnection);

            InFromClient inFromClient = new InFromClient(client);
            OutToClient outToClient = new OutToClient(client);

            Thread i = new Thread(new ThreadStart(inFromClient.readFromClient));
            Thread o = new Thread(new ThreadStart(outToClient.writeToClient));

            i.Start();
            o.Start();

        }
    };

    /**
     * 
     * Creates a socket to listen on a specified IP address and port number.
     * Creates individual threads to handle multiple clients.
     * 
     **/
    public class Server
    {
        public static void StartListening(string incomingIP, string incomingPort)
        {

            //Will be used to give a name to client(NOT USED YET)
            int count = 0;

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            int portNumber = int.Parse(incomingPort);
            IPAddress ipAddress = IPAddress.Parse(incomingIP);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNumber);

            // Create a TCP/IP socket.
            Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);


            try
            {

                // Bind the socket to the local endpoint and 
                // listen for incoming connections.
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection on port {0}...", portNumber);

                // Start listening for connections.
                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.
                    Socket newClient = listener.Accept();

                    ClientHandler ch = new ClientHandler(newClient);

                    Thread connection = new Thread(new ThreadStart(ch.handleNewClient));

                    connection.Start();

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //NOT USED(yet)
            // Console.WriteLine("\nPress ENTER to continue...");
            // Console.Read();

        }


        /**
         * 
         * Requests IP address and Port Number for server to initialize
         * 
         **/
        public static void Main(String[] args)
        {

            Console.WriteLine("Enter an IP address (EX: 127.0.0.1):");
            string ipAddress = Console.ReadLine();
            Console.WriteLine("Enter a port number (EX: 9876):");
            string portNumber = Console.ReadLine();

            StartListening(ipAddress, portNumber);

        }
    }
}
