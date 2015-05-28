using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace SensorValueVisualization.ViewModel
{
    public class SensorValuesServer
    {
        public string IpAdress { get; private set; }
        public int Port { get; private set; }

        private const int ReadIntervallInMilliseconds = 300;

        private TcpClient _connectedClient;

        private readonly TcpListener _listener;
        //private readonly IFormatter _formatter;
        private readonly XmlSerializer _formatter;
        private bool _isRunning;
        private readonly BackgroundWorker _backgroundWorker;
        private StreamReader _reader;

        public SensorValuesServer(string ipAdress, int port, BackgroundWorker backgroundWorker)
        {
            //_formatter = new BinaryFormatter();
            _formatter = new XmlSerializer(typeof(SensorValues.SensorValues));

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
                    Debug.WriteLine("Client has connected properly.");
                }
                catch (InvalidCastException e)
                {
                    Debug.WriteLine("Client has not connected properly.");
                    Debug.WriteLine(e.ToString());
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

            if (_reader != null)
            {
                _reader.Dispose();
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
                            _reader = new StreamReader(stream);

                            string receivedLine;

                            while ((receivedLine = _reader.ReadLine()) != null)
                            {
                                receivedXml.AppendLine(receivedLine);

                                if (receivedLine == "</SensorValues>")
                                {
                                    break;
                                }
                            }

                            using (Stream xmlStream = GenerateStreamFromString(receivedXml.ToString()))
                            {
                                SensorValues.SensorValues sensorValues = (SensorValues.SensorValues)_formatter.Deserialize(xmlStream);
                                Debug.WriteLine(sensorValues);
                                _backgroundWorker.ReportProgress(0, sensorValues);
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
                            Debug.WriteLine("Could not cast received message.");
                            Debug.WriteLine(e.ToString());
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
