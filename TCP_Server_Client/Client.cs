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
            try
            {
                while (true)
                {
                    // Receive the response from Server.
                    int bytesRec = connection.Receive(bytes);

                    string incomingFileName = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    //Check if server echoed a file request
                    if (incomingFileName.IndexOf('~') == 0)
                    {
                        handleFileRequest(connection, incomingFileName, bytes);
                    }
                    else
                    {
                        //print out server messge
                        Console.WriteLine("SERVER: {0}",
                            Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong with the server");
                Environment.Exit(0);
            }
        }
   
        private void handleFileRequest(Socket connection, string incomingFileName, byte[] bytes)
        {
            incomingFileName = incomingFileName.Remove(0, 1);

            Console.WriteLine("FILE REQUEST: " + incomingFileName + "...");

            if (incomingFileName.Contains(".txt"))
            {
                handleTextFile(connection, incomingFileName, bytes);
            }
            if (incomingFileName.Contains(".png") || incomingFileName.Contains(".jpg") ||
                incomingFileName.Contains(".jpeg"))
            {
                handleImgFile(connection, incomingFileName);
            }
        }

        private void handleTextFile(Socket connection, string incomingFileName, byte[] bytes)
        {
            //Receive file contents from connection
            int bytesFromFile = connection.Receive(bytes);

            string fileContents = Encoding.ASCII.GetString(bytes, 0, bytesFromFile);

            if (fileContents == "FNF")
            {
                Console.WriteLine("SERVER: ERROR FILE NOT FOUND");
            }
            else
            {
                try
                {
                    String filePath = "C:\\Users\\Jason\\Documents\\ClientFiles\\" +
                                      incomingFileName;
                    int count = 0;

                    while (File.Exists(filePath))
                    {
                        count++;
                        incomingFileName = count + incomingFileName;
                        filePath = "C:\\Users\\Jason\\Documents\\ClientFiles\\" +
                                      incomingFileName;
                    }
                    Console.WriteLine("Saving File " + incomingFileName + "...");
                    File.WriteAllText(filePath, fileContents);
                    Console.WriteLine("File saved!");
                }
                catch (IOException)
                {
                    Console.WriteLine("There was an error during file save");
                }
            }
        }

        private void fileSaveAttempt(string fileName)
        {

        }

        private void handleImgFile(Socket connection, string incomingFileName)
        {
            //Receive file contents from connection
            byte[] buffer = new byte[100000];
            connection.Receive(buffer, buffer.Length, SocketFlags.None);

            //Check if the server echoed file not found
            if (Encoding.UTF8.GetString(buffer) == "FNF")
            {
                Console.WriteLine("SERVER: ERROR FILE NOT FOUND");
            }

            else
            {
                try
                {
                    String filePath = "C:\\Users\\Jason\\Documents\\ClientFiles\\" +
                                      incomingFileName;
                    int count = 0;

                    while (File.Exists(filePath))
                    {
                        count++;
                        incomingFileName = count + incomingFileName;
                        filePath = "C:\\Users\\Jason\\Documents\\ClientFiles\\" +
                                      incomingFileName;
                    }
                    Console.WriteLine("Saving File " + incomingFileName + "...");
                    File.WriteAllBytes(filePath, buffer);
                    Console.WriteLine("File saved!");
                }
                catch (IOException)
                {
                    Console.WriteLine("There was an error during file save");
                }
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
            try
            {
                while (true)
                {
                    promptUserAndSendMessage();
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void promptUserAndSendMessage()
        {
            Console.WriteLine("Enter a message");

            //Store user input to convert to bytes
            string userInput = Console.ReadLine();

            char[] message = userInput.ToCharArray();

            byte[] byteToSend = Encoding.ASCII.GetBytes(message);

            connection.Send(byteToSend);

            try
            {
                if (userInput == "Quit")
                {
                    Environment.Exit(0);

                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
            }
            catch(Exception)
            {
                Console.WriteLine("You are here");
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
                Socket connection = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    connection.Connect(remoteEP);

                    startClientInputAndOutputThreads(connection);

                    Console.WriteLine("Socket connected to {0}",
                        connection.RemoteEndPoint.ToString());

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException)
                {
                    Console.WriteLine("There is no server available");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" YOU ARE HERE Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Invalid IP or Port number");
            }
        }

        private static void startClientInputAndOutputThreads(Socket connection)
        {
            try
            {
                InFromServer inFromServer = new InFromServer(connection);
                OutToServer outToServer = new OutToServer(connection);

                Thread i = new Thread(new ThreadStart(inFromServer.readFromServer));
                Thread o = new Thread(new ThreadStart(outToServer.writeToServer));


                i.Start();
                o.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("You are here 2");
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
