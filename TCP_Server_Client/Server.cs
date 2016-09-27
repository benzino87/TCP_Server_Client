﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

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

                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.StartsWith("~"))
                {
                    //remove tilda to look for file look up
                    string filename = data.Remove(0, 1);
                    //echo back to client original file name request to save file.
                    char[] echofile = data.ToCharArray();

                    try
                    {
                        connection.Send(Encoding.ASCII.GetBytes(echofile));
                        connection.SendFile("C:\\Users\\Jason\\Documents\\ServerFiles\\" + filename);
                    }
                    catch(FileNotFoundException)
                    {
                        string clientMessage = "FNF";
                        char[] convertedClientMessage = clientMessage.ToCharArray();
                        connection.Send(Encoding.ASCII.GetBytes(convertedClientMessage));
                    }
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
            //Send message to client indicating how to request file
            string clientMessage = "To request a file type ~filename";
            char[] convertedClientMessage = clientMessage.ToCharArray();
            byte[] informClient = Encoding.ASCII.GetBytes(convertedClientMessage);
            connection.Send(informClient);

            while (true)
            {
                //Store user input to convert to bytes
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

            //Will be used to give a name to client(NOT USED YET)
            int count = 0;

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            int portNumber = int.Parse(incomingPort);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
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

                    ClientHandler ch = new ClientHandler(newClient, count);

                    Thread connection = new Thread(new ThreadStart(ch.handleNewClient));

                    connection.Start();

                    count++;

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("You are here - SERVER");
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

            Console.WriteLine("Enter a port number (EX: 9876):");
            string portNumber = Console.ReadLine();

            StartListening(portNumber);

        }
    }
}