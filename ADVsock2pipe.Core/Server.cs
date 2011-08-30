/*
 * This file is part of ADVsock2pipe
 * Copyright (c) 2011 - ADVTOOLS SARL
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO.Pipes;
using System.IO;

namespace Advtools.Advsock2pipe
{
    /// <summary>
    /// Server that listen to a TCP socket, a named pipe and redirect data from the first to the second.
    /// </summary>
    public class Server
    {
        #region Constants
        /// <summary>Size of the internal data buffer</summary>
        private const int bufferSize_ = 16384;
        /// <summary>Number of incoming connections that can be queued (only 1)</summary>
        private const int backLog_ = 1;
        /// <summary>Number of server instances that share the same name</summary>
        private const int maxPipeInstances_ = 10;
        #endregion

        #region Private fields
        /// <summary>Internal data buffer</summary>
        private byte[] buffer_ = new byte[bufferSize_];
        /// <summary>Listening TCP socket</summary>
        private Socket listening_;
        /// <summary>Connected TCP socket</summary>
        private Socket socket_;
        /// <summary>Windows named pipe</summary>
        private NamedPipeServerStream pipe_;
        /// <summary>Name of the pipe</summary>
        private string pipeName_;
        /// <summary>Logger for this instance</summary>
        private Logger logger_;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Server()
        {
        }

        /// <summary>
        /// Start the server (both TCP socket and name pipe)
        /// </summary>
        /// <param name="config">Configuration of this instance</param>
        /// <returns>Return true if the server did start, false otherwise</returns>
        public bool Start(Config config)
        {
            try
            {
                // Create an instance of the logger
                logger_ = new Logger(config.LogLevel);

                // Record the name of the pipe, we may need it later to restart the pipe
                pipeName_ = config.Pipe;
                // First, start the named pipe
                StartPipe();
                // Then, start the TCP socket
                StartSocket(config.Port);
            }
            catch(SocketException e)
            {
                // A socket error, display it
                logger_.Log(Level.Error, e.Message);
                // and stop the application there
                return false;
            }
            catch(IOException e)
            {
                // A I/O (probably pipe) error, display it
                logger_.Log(Level.Error, e.Message);
                // and stop the application there
                return false;
            }

            // The server did start
            return true;
        }

        /// <summary>
        /// Start the TCP socket part of this server
        /// </summary>
        /// <param name="port">The number of the TCP port</param>
        private void StartSocket(int port)
        {
            // Create a TCP socket (IPv4)
            listening_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Create an endpoint (IP address and port number)
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            // Bind the socket to the endpoint
            listening_.Bind(ep);
            // Listen for incoming connection attempts
            listening_.Listen(backLog_);

            // Accept asynchronously incoming connection attempt
            listening_.BeginAccept(new AsyncCallback(OnAccept), this);
            logger_.Log(Level.Info, "Waiting on socket {0}", listening_.LocalEndPoint);
        }

        /// <summary>
        /// Called when a new connection is established
        /// </summary>
        /// <param name="result">The result of the asynchronous operation</param>
        private void OnAccept(IAsyncResult result)
        {
            // Asynchronously accepts an incoming connection attempt and get the socket to handle communication
            socket_ = listening_.EndAccept(result);
            logger_.Log(Level.Info, "Receive connection from {0}", socket_.RemoteEndPoint);

            // Accept new connections
            listening_.BeginAccept(new AsyncCallback(OnAccept), null);
            // Begins to asynchronously receive data 
            socket_.BeginReceive(buffer_, 0, buffer_.Length, SocketFlags.None, new AsyncCallback(OnReceive), this);
        }

        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="result">The result of the asynchronous operation</param>
        private void OnReceive(IAsyncResult result)
        {
            // Ends a pending asynchronous read
            int read = socket_.EndReceive(result);
            // Any data?
            if(read <= 0)
            {
                // No data: display a message and close this socket
                logger_.Log(Level.Info, "End of data. TCP port disconnected.");
                CloseSocket();
                return;
            }

            // Process data
            ProcessData(read);

            // Wait for the next data
            socket_.BeginReceive(buffer_, 0, buffer_.Length, SocketFlags.None, new AsyncCallback(OnReceive), this);
        }

        /// <summary>
        /// Close the TCP socket
        /// </summary>
        private void CloseSocket()
        {
            try
            {
                if(socket_ != null)
                {
                    socket_.Shutdown(SocketShutdown.Both);
                    socket_.Close();
                    socket_ = null;
                }
            }
            catch(SocketException)
            {
                // Do not display exception in this case: we just want to close the socket.
            }
        }

        /// <summary>
        /// Process data from the TCP socket: send it to the named pipe
        /// </summary>
        /// <param name="size">Size of the data received (the data are in the internal buffer)</param>
        private void ProcessData(int size)
        {
            // If there is no pipe or if it is not connected, discard the data
            if(pipe_ == null || !pipe_.IsConnected)
            {
                logger_.Log(Level.Debug, "Receive {0} bytes. Nobody is connected on the pipe so discard data.", size);
                return;
            }

            logger_.Log(Level.Debug, "Receive {0} bytes. Write data to the pipe.", size);
            try
            {
                // Simply write the date to the pipe, as-is
                pipe_.Write(buffer_, 0, size);
            }
            catch(IOException e)
            {
                // I/O exception (such as brocken pipe): display the error and start again the pipe
                logger_.Log(Level.Error, e.Message);
                StartPipe();
            }
        }

        /// <summary>
        /// Start the named pipe
        /// </summary>
        private void StartPipe()
        {
            // First, close the previous pipe, if any
            ClosePipe();

            // Create the named pipe
            pipe_ = new NamedPipeServerStream(pipeName_, PipeDirection.Out, maxPipeInstances_, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            // Wait for a client to connect
            pipe_.BeginWaitForConnection(OnPipeConnect, this);
            logger_.Log(Level.Info, "Wait for a connection on the pipe");
        }

        /// <summary>
        /// Called when a client connect to the pipe
        /// </summary>
        /// <param name="result">The result of the asynchronous operation</param>
        private void OnPipeConnect(IAsyncResult result)
        {
            logger_.Log(Level.Info, "Pipe connected");
            // Accept the connection
            pipe_.EndWaitForConnection(result);
        }

        // Close the named pipe
        private void ClosePipe()
        {
            if(pipe_ != null)
            {
                try
                {
                    // Disconnect, if it is stil connected
                    if(pipe_.IsConnected)
                        pipe_.Disconnect();
                    // Close and dispose (a little redundant...)
                    pipe_.Close();
                    pipe_.Dispose();
                    pipe_ = null;
                }
                catch(IOException)
                {
                    // Do nothing: we just want to close the pipe
                }
            }
        }
    }
}
