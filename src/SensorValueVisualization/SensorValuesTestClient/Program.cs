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
        private static Random _randomGenerator;

        private const int RandomValueMax = 360;

        static void Main(string[] args)
        {
            _tcpClient = new TcpClient();
            _formatter = new XmlSerializer(typeof(SensorValues));
            _randomGenerator = new Random();

            while (true)
            {
                SendSensorValues(new SensorValues { AccelerometerX = _randomGenerator.Next(0, RandomValueMax), AccelerometerY = _randomGenerator.Next(0, RandomValueMax), AccelerometerZ = _randomGenerator.Next(0, RandomValueMax) });
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private static void SendSensorValues(SensorValues message)
        {
            IAsyncResult asyncResult = _tcpClient.BeginConnect(IPAddress.Parse("127.0.0.1"), 1234, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                _tcpClient.Close();
                throw new TimeoutException();
            }
            
            if (_tcpClient.Connected)
            {
                _tcpClient.EndConnect(asyncResult);
                _networkStream = _tcpClient.GetStream();
                _formatter.Serialize(Console.Out, message);
                _formatter.Serialize(_networkStream, message);
                _tcpClient.Client.Shutdown(SocketShutdown.Both);
                _networkStream.Close();
            }
            else
            {
                Console.WriteLine("The connection to the server has been lost. Client is no longer connected.");
            }

            _tcpClient.Close();
            _tcpClient = new TcpClient();
        }
    }
}
