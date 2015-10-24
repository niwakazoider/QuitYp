using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QuitYpApplication
{
    class QuitServer
    {
        private Socket server;
        private string channelData = "";

        public QuitServer()
        {
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 47144);

                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                server.Bind(ipEndPoint);
                server.Listen(5);
            }
            catch (SocketException)
            {
                //Console.Write(e.Message);
            }
        }

        public void SetChannelData(string channelDataText)
        {
            channelData = channelDataText;
        }

        public void Stop()
        {
            try
            {
                server.Close();
            }
            catch (Exception)
            {

            }
        }

        public void Run()
        {

            try
            {
                while (true)
                {
                    Socket client = server.Accept();
                    Response response = new Response(this, client);
                    Thread thread = new Thread(response.Run);
                    thread.Start();
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                try
                {
                    server.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        class Response
        {
            private Socket client;
            private QuitServer server;

            public Response(QuitServer server, Socket client)
            {
                this.server = server;
                this.client = client;
            }

            public void Run()
            {
                try
                {
                    string request = ReadRequest();
                    string response = DateTime.Now + "";
                    if (request.StartsWith("GET /index.txt"))
                    {
                        response = server.channelData;
                        SendResponse(200, response);
                    }
                    else
                    {
                        SendResponse(404, "<html><body><a href=\"http://localhost:47144/index.txt\">http://localhost:47144/index.txt</a></body></html>");
                    }

                }
                catch (SocketException e)
                {
                    Console.Write(e.Message);
                }
                finally
                {
                    client.Close();
                }
            }

            private string ReadRequest() {
                byte[] buffer = new byte[1024 * 8];
                int len = client.Receive(buffer);
                if (len <= 0)
                    return "";

                String request = Encoding.ASCII.GetString(buffer, 0, len);
                //Console.Write(request);

                return request;
            }

            private void SendResponse(int status, string response)
            {
                byte[] responseBody = Encoding.UTF8.GetBytes(response);

                String httpHeader = String.Format(
                    "HTTP/1.0 200 OK\r\n" +
                    "Content-type: text/plain; charset=UTF-8\r\n" +
                    "Content-length: {0}\r\n" +
                    "Connection:close\r\n" + 
                    "\r\n",
                    responseBody.Length);

                if (status != 200)
                {
                    httpHeader = String.Format(
                    "HTTP/1.0 404 Not Found\r\n" +
                    "Content-type: text/html; charset=UTF-8\r\n" +
                    "Content-length: {0}\r\n" +
                    "Connection:close\r\n" +
                    "\r\n",
                    responseBody.Length);
                }

                byte[] responseHeader = Encoding.UTF8.GetBytes(httpHeader);

                client.Send(responseHeader);
                client.Send(responseBody);
            }
        }
    }

}
