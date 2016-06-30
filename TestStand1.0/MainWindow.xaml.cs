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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Skunk;
using System.IO;
using OxyPlot;
using ExcelLib;


namespace TestStand1._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExcelLib.ExcelLib _excel;
        private Skunk.Skunk _skunk;
        private string _comPortName;
        private string _excelPath;
        private int _countDataForSaveExcel;
        private int _countDataForSaveOnWorksheetExcel;
        private byte _startByteMTSP;
        private ViewBindings _bindings;
        private int counter = 0;
        public MainWindow()
        {
            InitializeComponent();
            _bindings = new ViewBindings();
            DataContext = _bindings;
            getDataFromConfigFile();
            InitExcel();

        }

        private void InitExcel()
        {
            _excel = new ExcelLib.ExcelLib(_excelPath);
            string[] names = new string[3];
            names[0] = "№";
            names[1] = "Дальномер ик";
            names[2] = "Дальномер уз";
            _excel.EnableTimeColumn = true;
            _excel.CountDataForSave = _countDataForSaveExcel;
            _excel.CountDataForSaveOnWorksheet = _countDataForSaveOnWorksheetExcel;

            _excel.AddColumnsNames(names);
        }

        private void getDataFromConfigFile()
        {
            if (System.IO.File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                _comPortName = sr.ReadLine();
                if (_comPortName == "") MessageBox.Show("Нет файла настроек или он был повреждён!");
                _excelPath = sr.ReadLine();
                var startByteVal = Convert.ToInt32(sr.ReadLine(), 16);
                _startByteMTSP = Convert.ToByte(startByteVal);
                _countDataForSaveExcel = Convert.ToInt32(sr.ReadLine(), 16);
                _countDataForSaveOnWorksheetExcel = Convert.ToInt32(sr.ReadLine(), 16);
                sr.Close();
            }
            else
            {
                MessageBox.Show("Нет файла настроек или он был повреждён!");
                _excelPath = null;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SettingsButton.Width = this.ActualWidth / 10;
            SettingsButton.Height = SettingsButton.Width;

            StartResearch.Width = this.ActualWidth / 2;
            StartResearch.Height = this.ActualHeight / 10;

            ForceTextBox.Width = column1.ActualWidth - 20;
            ForceButton.Width = column1.ActualWidth;
            ForceLabel.Width = column1.ActualWidth;
            if (ForceLabel.Width > 90) ForceLabel.FontSize = 20;
            else
            {
                if (ForceLabel.Width < 90) ForceLabel.FontSize = 15;
                if (ForceLabel.Width < 70) ForceLabel.FontSize = 12;
                if (ForceLabel.Width < 65) ForceLabel.FontSize = 10;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartResearch.Content.ToString() != "Начать исследование")
            {
                MessageBox.Show("Нельзя менять настройки во время исследования");
            }
            else
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.NewConfigData += OnNewConfigData;
                settingsWindow.Show();
            }
        }

        private void OnNewConfigData(object sender, EventArgs e)
        {
            getDataFromConfigFile();
            InitExcel();
        }

        private void StartResearch_Click(object sender, RoutedEventArgs e)
        {
            if (StartResearch.Content.ToString() == "Начать исследование")
            {
                try
                {
                    _skunk = new Skunk.Skunk(_startByteMTSP, 9600, _comPortName);
                    _skunk.MessageReceived += OnMessageReceived;
                    StartResearch.Content = "Остановить исследование";

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Проблемы с передачей данных");
                }
               

            }
            else
            {
                _skunk.ClosePort();
                _excel.CloseExcel();
                // чистим график
                StartResearch.Content = "Начать исследование";
            }
        }

        private void OnMessageReceived(object sender, byte[] e)
        {
            var UsData = (e[0] << 8) + e[1];
            var IrData = (e[2] << 8) + e[3];
            counter++;
            var _point = new DataPoint(counter, IrData);
            var _point1 = new DataPoint(counter, UsData);
            _bindings.AddPoint(_point, _point1);
            string[] data = new string[3];
            data[0] = counter.ToString();
            data[1] = IrData.ToString();
            data[2] = UsData.ToString();
            if (_excel.IsDataSavesToExcel(data))
            {
                _excel.ClearCounterAndChangeWorksheet();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (StartResearch.Content.ToString() != "Начать исследование")
            {
                _skunk.ClosePort();
                _excel.CloseExcel();
            }
        }
    }

    
}
