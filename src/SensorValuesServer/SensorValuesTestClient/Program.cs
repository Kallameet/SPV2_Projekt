using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SensorValuesServer;

namespace SensorValuesTestClient
{
    class Program
    {
        private static TcpClient _tcpClient;
        private static XmlSerializer _formatter;
        private static NetworkStream _networkStream;

        static void Main(string[] args)
        {
            _tcpClient = new TcpClient();
            _formatter = new XmlSerializer(typeof(SensorValues));

            while (true)
            {
                SendSensorValues(new SensorValues { AccelerometerX = 10, AccelerometerY = 20, AccelerometerZ = 30 });
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            Console.ReadLine();
        }

        private static void SendSensorValues(SensorValues message)
        {
            IAsyncResult asyncResult = _tcpClient.BeginConnect(IPAddress.Parse("127.0.0.1"), 12345, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                _tcpClient.Close();
                throw new TimeoutException();
            }
            _tcpClient.EndConnect(asyncResult);
            _networkStream = _tcpClient.GetStream();

            if (_tcpClient.Connected)
            {
                _formatter.Serialize(Console.Out, message);
                _formatter.Serialize(_networkStream, message);
                _tcpClient.Client.Shutdown(SocketShutdown.Both);
            }
            else
            {
                Console.WriteLine("The connection to the server has been lost. Client is no longer connected.");
                _networkStream.Close();

            }
            _tcpClient.Close();
            _tcpClient = new TcpClient();
        }
    }
}
