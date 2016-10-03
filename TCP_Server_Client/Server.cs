using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

/**
 * 
 * @author Jason Bensel
 * @version TCP_Project_Part_1
 * 
 **/

namespace TCP_Server_Client
{

    /**
     * 
     * Handles all incoming data from individual client and file requests
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

                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                //Check for file request key
                if (data.StartsWith("~"))
                {
                    searchAndSendFileToClient(connection, data);
                }

                if (data == "Quit")
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                    Console.WriteLine("Client has left");
                    break;
                }

                // Show the data on the console.
                Console.WriteLine("CLIENT: {0}", data);
            }
        }

        /**
         * (HELPER)
         * Searches for requested file and sends to client, if file is not found
         * it will send a notification to client that no such file exists.
         * 
         **/
        private void searchAndSendFileToClient(Socket connection, string data)
        {
            //remove tilda to look for file look up
            string filename = data.Remove(0, 1);
            //echo back to client original file name request to save file.
            char[] echofile = data.ToCharArray();

            try
            {
                //Echo the file and send the file conetents
                connection.Send(Encoding.ASCII.GetBytes(echofile));
                connection.SendFile("C:\\Users\\Jason\\Documents\\ServerFiles\\" +
                                    filename);
            }
            catch (FileNotFoundException)
            {
                string clientMessage = "FNF";
                char[] convertedClientMessage = clientMessage.ToCharArray();
                connection.Send(Encoding.ASCII.GetBytes(convertedClientMessage));
            }
        }
    }

    /**
     * 
     * Handles output to client who most recently sent data (Chat)
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
            //Send connection message to client indicating how to request a file
            string clientMessage = "To request a file type ~filename";
            char[] convertedClientMessage = clientMessage.ToCharArray();
            byte[] informClient = Encoding.ASCII.GetBytes(convertedClientMessage);
            connection.Send(informClient);

            while (true)
            {
                //Wait for user input from server side to send to latest client
                string userInput = Console.ReadLine();
                char[] message = userInput.ToCharArray();
                byte[] byteToSend = Encoding.ASCII.GetBytes(message);

                connection.Send(byteToSend);

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
        private int clientNumber;

        public ClientHandler(Socket client, int clientNumber)
        {
            this.client = client;
            this.clientNumber = clientNumber;
        }
        
        //Create input and output threads for each client that connected
        public void handleNewClient()
        {

            string verifyConnection = client.Connected ? "Client "+clientNumber+" connected" : 
                "Attempted connection by client "+clientNumber;

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
        public static void StartListening(string incomingPort)
        {

            //used to assign values(names) to clients
            int count = 0;
            Socket listener = constructClientServerSocket(incomingPort);

            try
            {
                // Start listening for connections.
                while (true)
                {
                    createNewClientThread(listener, count);
                    count++;
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong during connection attempt");
            }
        }


        /**
         * (HELPER)
         * Creates indivudal thread specific to client that is requesting connection
         * 
         **/
        private static void createNewClientThread(Socket listener, int count)
        {
            try
            {
                // Program is suspended while waiting for an incoming connection.
                Socket newClient = listener.Accept();

                ClientHandler ch = new ClientHandler(newClient, count);

                Thread connection = new Thread(new ThreadStart(ch.handleNewClient));

                connection.Start();
            }
            catch (Exception)
            {
                throw; //toss the exception up
            }
        }

        /**
         * 
         * Create TCP/IP socket and establish the local endpoint. Notify user
         * Server is listening.
         * 
         **/
        private static Socket constructClientServerSocket(string incomingPort)
        {

            try
            {
                int portNumber = int.Parse(incomingPort);
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNumber);

                Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
                
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection on port {0}...", portNumber);

                return listener;
            }
            catch (Exception)
            {
                throw; //toss the exception up
            }

            
        }

        /**
         * 
         * Requests IP address and Port Number for server to initialize
         * 
         **/
        public static void Main(String[] args)
        {

            Console.WriteLine("Enter a port number (EX: 9876):");
            string portNumber = Console.ReadLine();

            StartListening(portNumber);

        }
    }
}
