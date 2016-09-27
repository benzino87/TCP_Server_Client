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
            while (true)
            {
                // Receive the response from the remote device.
                int bytesRec = connection.Receive(bytes);

                string incomingFileName = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                //Check if server echoed a file request
                if (incomingFileName.IndexOf('~') == 0)
                {
                    incomingFileName = incomingFileName.Remove(0, 1);

                    Console.WriteLine("FILE REQUEST: " + incomingFileName + "...");

                    if (incomingFileName.Contains(".txt"))
                    {
                        //Receive file contents from connection
                        int bytesFromFile = connection.Receive(bytes);

                        string fileContents = Encoding.ASCII.GetString(bytes, 0, bytesFromFile);

                        if(fileContents == "FNF")
                        {
                            Console.WriteLine("SERVER: ERROR FILE NOT FOUND");
                        }
                        else
                        {
                            try
                            {
                                Console.WriteLine("Saving File " + incomingFileName + "...");
                                File.WriteAllText("C:\\Users\\benselj\\Documents\\ClientFiles\\" + 
                                                  incomingFileName, fileContents);
                                Console.WriteLine("File saved!");
                            }
                            catch(IOException)
                            {
                                Console.WriteLine("File already exists...");
                            }
                        }

                    }
                    if (incomingFileName.Contains(".png") || incomingFileName.Contains(".jpg") || 
                        incomingFileName.Contains(".jpeg"))
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
                                Console.WriteLine("Saving File " + incomingFileName + "...");
                                File.WriteAllBytes("C:\\Users\\benselj\\Documents\\ClientFiles\\" + 
                                                   incomingFileName, buffer);
                                Console.WriteLine("File saved!");
                            }
                            catch (IOException)
                            {
                                Console.WriteLine("File already exists...");
                            }
                        }
                    }

                }
                else
                {

                    Console.WriteLine("SERVER: {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
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
                    Console.WriteLine("Enter a message");

                    //Store user input to convert to bytes
                    string userInput = Console.ReadLine();

                    char[] message = userInput.ToCharArray();

                    byte[] byteToSend = Encoding.ASCII.GetBytes(message);

                    connection.Send(byteToSend);

                    if (userInput == "Quit")
                    {
                        connection.Shutdown(SocketShutdown.Both);
                        connection.Close();
                    }
                }
            }
            catch(SocketException)
            {
                throw;
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
                catch (SocketException)
                {
                    Console.WriteLine("There is no server available");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception)
            {
                Console.WriteLine("Invalid IP or Port number");
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
