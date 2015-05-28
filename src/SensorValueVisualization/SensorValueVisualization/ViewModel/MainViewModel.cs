using System;
using System.ComponentModel;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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
        private const double Gravity = 9.81;
        private const double Multiplier = 360/(2*Gravity);

        private SensorValuesServer _sensorValuesServer;
        private BackgroundWorker _chatServerWorker;
        public MainViewModel()
        {
            IpAdress = "10.0.0.2";
            Port = 1234;
            IsConnected = false;
        }

        ~MainViewModel()
        {
            _sensorValuesServer.Stop();
            IsConnected = false;
        }

        private string _ipAdress;

        public string IpAdress
        {
            get { return _ipAdress; }
            set
            {
                _ipAdress = value;
                RaisePropertyChanged(() => IpAdress);
            }
        }

        private int _port;

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged(() => Port);
            }
        }
        
        private void RunChatServer(object sender, DoWorkEventArgs e)
        {
            _sensorValuesServer.Start();
        }

        private int ConvertFromAccelerometerToAngle(double val)
        {
            return Convert.ToInt32(Math.Round(val*Multiplier));
        }

        private void ReadSensorValues(object sender, ProgressChangedEventArgs e)
        {
            SensorValues.SensorValues sensorValues = e.UserState as SensorValues.SensorValues;

            if (sensorValues != null)
            {
                AccelerometerX = ConvertFromAccelerometerToAngle(sensorValues.AccelerometerX);
                AccelerometerY = ConvertFromAccelerometerToAngle(sensorValues.AccelerometerY);
                AccelerometerZ = ConvertFromAccelerometerToAngle(sensorValues.AccelerometerZ);
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
            _chatServerWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _sensorValuesServer = new SensorValuesServer(IpAdress, Port, _chatServerWorker);
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
            _sensorValuesServer.Stop();
            IsConnected = false;
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                IsDisconnected = !value;
                _isConnected = value;
                RaisePropertyChanged(() => IsConnected);
            }
        }

        private bool _isDisconnected;

        public bool IsDisconnected
        {
            get { return _isDisconnected; }
            set
            {
                _isDisconnected = value;
                RaisePropertyChanged(() => IsDisconnected);
            }
        }
    }
}