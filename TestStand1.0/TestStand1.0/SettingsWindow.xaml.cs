using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.Win32;

namespace TestStand1._0
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private Thread _comPortThread;
        private int _comPortsCount = 0;
        private bool _wasSaveButtonClicked = false;

        public event EventHandler NewConfigData;

        private const string ConfigFileName = "config.txt";
        public SettingsWindow()
        {
            InitializeComponent();
            var comPorts = SerialPort.GetPortNames();
            COMPortComboBox.ItemsSource = comPorts;
            if (COMPortComboBox.Items != null) COMPortComboBox.SelectedIndex = 0;
            _comPortsCount = comPorts.Length;
            _comPortThread = new Thread(ComPortThread) { IsBackground = true };
            _comPortThread.Start();
            getDataFromConfigFile();
        }

        private void getDataFromConfigFile()
        {
            if (System.IO.File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader(ConfigFileName);
                COMPortComboBox.SelectedItem = sr.ReadLine();
                LabelExcelPath.Content = sr.ReadLine();
                if (LabelExcelPath.Content != null) ExcelToolTip.Text = LabelExcelPath.Content.ToString();
                StartByteTextBox.Text = sr.ReadLine();
                CountDataForSaveExcelTextBox.Text = sr.ReadLine();
                CountDataForSaveOnWorksheetExcelTextBox.Text = sr.ReadLine();
                sr.Close();
            }
        }

        private void ComPortThread()
        {
            while (true)
            {
                var comPorts = SerialPort.GetPortNames();
                if (comPorts.Length != _comPortsCount)
                {
                    Dispatcher.Invoke(() => COMPortComboBox.ItemsSource = comPorts);
                    _comPortsCount = comPorts.Length;
                }
                Thread.Sleep(500);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _wasSaveButtonClicked = true;
            if (SaveConfig())
            {
                MessageBox.Show("Сохранено");
                this.Close();
            }          
        }

        private bool SaveConfig()
        {
            if (COMPortComboBox.SelectedItem == null)
            {
                MessageBox.Show("COM-порт не выбран!");

            }
            else
            {
                if (StartByteTextBox.Text == "" || !IsRightStartByte())
                {
                    MessageBox.Show("Не кореектные данные указаны в поле Стартового байта");
                }
                else
                {
                    StreamWriter sw = new StreamWriter(ConfigFileName);
                    sw.WriteLine(COMPortComboBox.SelectedItem.ToString());
                    sw.WriteLine(LabelExcelPath.Content.ToString());
                    sw.WriteLine(StartByteTextBox.Text);
                    sw.WriteLine(CountDataForSaveExcelTextBox.Text);
                    sw.WriteLine(CountDataForSaveOnWorksheetExcelTextBox.Text);
                    sw.Close();
                    if (NewConfigData != null) NewConfigData(this, EventArgs.Empty);
                    return true;
                }
            }
            return false;
        }

        private bool IsRightStartByte()
        {
            // ТУТ ТОЛЬКО ОТ 0 до 255
            var val = Convert.ToInt32(StartByteTextBox.Text, 16);
            if (val >= 0 && val < 256) return true;
            return false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (!_wasSaveButtonClicked)
            {

                if (MessageBox.Show("Сохранить?", "Сохранение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SaveConfig();
                }
            }
            (this.Owner as Window).Focus();
        }

        private void SearchExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Excel Files (*.xlsx) | *.xlsx";
            if (fileDialog.ShowDialog() == true)
            {
                LabelExcelPath.Content = fileDialog.FileName;
                ExcelToolTip.Text = fileDialog.FileName;
            }
        }


    }
}
