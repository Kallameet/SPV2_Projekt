using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SensorValuesServer;

namespace SensorValueVisualization.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private const string IpAdress = "127.0.0.1";
        private const int Port = 1234;
        private readonly ChatServer _chatServer;
        private readonly BackgroundWorker _chatServerWorker;
        public MainViewModel()
        {
            _chatServerWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _chatServer = new ChatServer(IpAdress, Port, _chatServerWorker);
            try
            {
                _chatServerWorker.DoWork += RunChatServer;
                _chatServerWorker.ProgressChanged += ReadSensorValues;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unexpected Exception occured.");
                Debug.WriteLine(e.ToString());
            }
        }

        ~MainViewModel()
        {
            _chatServer.Stop();
            IsConnected = false;
        }

        private void RunChatServer(object sender, DoWorkEventArgs e)
        {
            _chatServer.Start();
        }

        private void ReadSensorValues(object sender, ProgressChangedEventArgs e)
        {
            SensorValues sensorValues = e.UserState as SensorValues;

            if (sensorValues != null)
            {
                AccelerometerX = sensorValues.AccelerometerX;
                AccelerometerY = sensorValues.AccelerometerY;
                AccelerometerZ = sensorValues.AccelerometerZ;
            }
        }

        private int _accelerometerX;
        public int AccelerometerX
        {
            get { return _accelerometerX; }
            set
            {
                _accelerometerX = value;
                RaisePropertyChanged(() => AccelerometerX);
            }
        }

        private int _accelerometerY;
        public int AccelerometerY
        {
            get { return _accelerometerY; }
            set
            {
                _accelerometerY = value;
                RaisePropertyChanged(() => AccelerometerY);
            }
        }

        private int _accelerometerZ;
        public int AccelerometerZ
        {
            get { return _accelerometerZ; }
            set
            {
                _accelerometerZ = value;
                RaisePropertyChanged(() => AccelerometerZ);
            }
        }

        private RelayCommand _clickStartCommand;

        public RelayCommand ClickStartCommand
        {
            get { return _clickStartCommand ?? (_clickStartCommand = new RelayCommand(OnClickStart)); }
        }

        private void OnClickStart()
        {
            if (!_chatServerWorker.IsBusy)
            {
                _chatServerWorker.RunWorkerAsync();
                IsConnected = true;
            }
        }

        private RelayCommand _clickStopCommand;

        public RelayCommand ClickStopCommand
        {
            get { return _clickStopCommand ?? (_clickStopCommand = new RelayCommand(OnClickStop)); }
        }

        private void OnClickStop()
        {
            _chatServer.Stop();
            IsConnected = false;
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged(() => IsConnected);
            }
        }
    }
}