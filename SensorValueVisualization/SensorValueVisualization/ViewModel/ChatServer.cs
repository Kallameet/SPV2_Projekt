using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using SensorValuesServer;

namespace SensorValueVisualization.ViewModel
{
    public class ChatServer
    {
        public string IpAdress { get; private set; }
        public int Port { get; private set; }

        private const int ReadIntervallInMilliseconds = 500;

        private TcpClient _connectedClient;

        private readonly TcpListener _listener;
        //private readonly IFormatter _formatter;
        private readonly XmlSerializer _formatter;
        private bool _isRunning;
        private BackgroundWorker _backgroundWorker;

        public ChatServer(string ipAdress, int port, BackgroundWorker backgroundWorker)
        {
            //_formatter = new BinaryFormatter();
            _formatter = new XmlSerializer(typeof(SensorValues));

            IpAdress = ipAdress;
            Port = port;

            IPAddress adress = IPAddress.Parse(ipAdress);
            _listener = new TcpListener(adress, port);

            _backgroundWorker = backgroundWorker;
        }

        public void Start()
        {
            _isRunning = true;

            _listener.Start();
            Console.WriteLine("{0} Server started, now listening for clients.", DateTime.Now.ToString("G"));

            while (_isRunning)
            {
                if (!_listener.Pending())
                {
                    Thread.Sleep(500);
                    continue;
                }

                _connectedClient = _listener.AcceptTcpClient();
                
                try
                {
                    ThreadPool.QueueUserWorkItem(ReadClientMessages, null);
                }
                catch (InvalidCastException e)
                {
                    Console.WriteLine("Client has not connected properly.");
                    Console.WriteLine(e.ToString());
                    _connectedClient.Close();
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;

            if (_connectedClient != null)
            {
                _connectedClient.Close();
            }

            _listener.Stop();
        }

        private void ReadClientMessages(Object obj)
        {
            while (_isRunning && _connectedClient.Connected)
            {
                if (_connectedClient.Connected)
                {
                    NetworkStream stream = _connectedClient.GetStream();
                    {
                        try
                        {
                            //SensorValues sensorValues = (SensorValues)_formatter.Deserialize(stream);

                            StringBuilder receivedXml = new StringBuilder(String.Empty);
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string receivedLine;

                                while ((receivedLine = reader.ReadLine()) != null)
                                {
                                    receivedXml.AppendLine(receivedLine);
                                }

                                using (Stream xmlStream = GenerateStreamFromString(receivedXml.ToString()))
                                {
                                    SensorValues sensorValues = (SensorValues)_formatter.Deserialize(xmlStream);
                                    //Console.WriteLine(sensorValues);
                                    _backgroundWorker.ReportProgress(0, sensorValues);
                                }
                            }
                        }
                        catch (IOException)
                        {
                            //Client closed connection
                        }
                        catch (SerializationException)
                        {
                            //currently no new message   
                        }
                        catch (InvalidCastException e)
                        {
                            Console.WriteLine("Could not cast received message.");
                            Console.WriteLine(e.ToString());
                        }
                        finally
                        {
                            Thread.Sleep(ReadIntervallInMilliseconds);
                        }
                    }
                }
            }
        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
