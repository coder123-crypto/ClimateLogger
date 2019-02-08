// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Ivtn7Monitor.Properties;

namespace Ivtn7Monitor
{
    internal sealed class MonitorModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Свойства COM порта
        public static IEnumerable<string> Ports => SerialPort.GetPortNames().OrderBy(a => int.Parse(Regex.Replace(a, "[^0-9]", ""))).ToList();

        public string Port
        {
            get => Settings.Default.SerialPort;
            set
            {
                Settings.Default.SerialPort = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public TimeSpan Interval
        {
            get => Settings.Default.Interval;
            set
            {
                Settings.Default.Interval = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public int BufferSize
        {
            get => Settings.Default.BufferSize;
            set
            {
                Settings.Default.BufferSize = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        #endregion

        #region Свойства БД
        public string DbHost
        {
            get => Settings.Default.DbHost;
            set
            {
                Settings.Default.DbHost = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public int DbPort
        {
            get => Settings.Default.DbPort;
            set
            {
                Settings.Default.DbPort = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string DbLogin
        {
            get => Settings.Default.DbLogin;
            set
            {
                Settings.Default.DbLogin = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public string DbPassword
        {
            get => Settings.Default.DbPassword;
            set
            {
                Settings.Default.DbPassword = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        #endregion

        #region Свойства БД
        public int DeviceId
        {
            get => Settings.Default.DeviceId;
            set
            {
                Settings.Default.DeviceId = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public int RoomId
        {
            get => Settings.Default.RoomId;
            set
            {
                Settings.Default.RoomId = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        #endregion

        #region Принятые показания
        public IvtmValues Values => new IvtmValues
        {
            Temperature = Temperature,
            Humidity = Humidity,
            Pressure = Pressure,
            Voltage = Voltage
        };

        public double Temperature
        {
            get => _temperature;
            private set
            {
                _temperature = value;
                OnPropertyChanged();
            }
        }
        private double _temperature;

        public double Pressure
        {
            get => _pressure;
            private set
            {
                _pressure = value;
                OnPropertyChanged();
            }
        }
        private double _pressure;

        public double Humidity
        {
            get => _humidity;
            private set
            {
                _humidity = value;
                OnPropertyChanged();
            }
        }
        private double _humidity;

        public double Voltage
        {
            get => _voltage;
            private set
            {
                _voltage = value;
                OnPropertyChanged();
            }
        }
        private double _voltage;
        #endregion

        #region Команды
        public RelayCommand Start { get; }
        private void _start()
        {
            _worker.RunWorkerAsync();
        }

        public RelayCommand Stop { get; }
        private void _stop()
        {
            _worker.CancelAsync();
        }
        #endregion

        #region Выполнение мониторинга
        private void MonitoringProcess(object sender, DoWorkEventArgs e)
        {
            _port.PortName = Port;
            _port.Open();

            while (!_worker.CancellationPending)
            {
                var list = new List<byte> {(byte) '$'};
                list.AddRange(new[] {(byte) 'F', (byte) 'F', (byte) 'F', (byte) 'F'});
                list.AddRange(new[] {(byte) 'R', (byte) 'R'});
                list.AddRange(new[] {(byte) '0', (byte) '0', (byte) '0', (byte) '0'});
                list.AddRange(new[] {(byte) '0', (byte) 'E'});
                list.AddRange(((byte) list.Sum(t => t)).ToString("X2").Select(t => (byte) t));
                list.Add(0x0D);

                var output = list.ToArray();
                _port.Write(output, 0, output.Length);

                int count = 0;

                while (_port.BytesToRead < 13)
                {
                    Thread.Sleep(100);
                    count++;

                    if (count == 10)
                    {
                        break;
                    }
                }

                var answer = _port.ReadExisting().Trim();
                if (answer.Length != 14 * 2 + 9)
                {
                    throw new InvalidOperationException("Answers'length uncorrected");
                }

                if (!answer.StartsWith("!", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Answer must begin from '!'");
                }

                if (answer.Substring(5, 2) != "RR")
                {
                    throw new InvalidOperationException("Command must be 'RR'");
                }

                string data = answer.Substring(7, 14 * 2);
                var bytes = Enumerable.Range(0, data.Length / 2).Select(i => data.Substring(i * 2, 2)).ToList();
                Temperature = BitConverter.ToSingle(bytes.Select(t => byte.Parse(t, NumberStyles.HexNumber)).ToArray(), 0);
                Humidity = BitConverter.ToSingle(bytes.Select(t => byte.Parse(t, NumberStyles.HexNumber)).ToArray(), sizeof(float));
                Voltage = BitConverter.ToSingle(bytes.Select(t => byte.Parse(t, NumberStyles.HexNumber)).ToArray(), sizeof(float) * 2);
                Pressure = BitConverter.ToUInt16(bytes.Select(t => byte.Parse(t, NumberStyles.HexNumber)).ToArray(), sizeof(float) * 3);
                OnPropertyChanged(nameof(Values));

                Thread.Sleep((int) Interval.TotalMilliseconds);
            }
        }

        private void MonitoringCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _port.Close();
            Temperature = 0.0;
            Humidity = 0.0;
            Voltage = 0.0;
            Pressure = 0;
            OnPropertyChanged(nameof(Values));
            Misc.UpdateGui();

            if (e.Error != null)
            {
                Messages.ShowError(e.Error.Message);
            }
        }

        private readonly SerialPort _port = new SerialPort
        {
            BaudRate = 115200,
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None,
            ReadTimeout = 500
        };
        private readonly BackgroundWorker _worker = new BackgroundWorker {WorkerReportsProgress = false, WorkerSupportsCancellation = true};
        #endregion

        public MonitorModel()
        {
            Start = new RelayCommand(_start, () => !_worker.IsBusy);
            Stop = new RelayCommand(_stop, () => _worker.IsBusy);

            _worker.DoWork += MonitoringProcess;
            _worker.RunWorkerCompleted += MonitoringCompleted;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}