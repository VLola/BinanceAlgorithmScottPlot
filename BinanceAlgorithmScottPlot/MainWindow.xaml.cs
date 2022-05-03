﻿using BinanceAlgorithmScottPlot.Binance;
using BinanceAlgorithmScottPlot.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using BinanceAlgorithmScottPlot.Candlestick;
using BinanceAlgorithmScottPlot.TimeFiles;
using Binance.Net.Enums;
using ScottPlot;
using System.Drawing;
using Binance.Net.Objects.Models.Spot;

namespace BinanceAlgorithmScottPlot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket socket;
        public List<string> list_sumbols_name = new List<string>();
        public List<ListCandles> list_listcandles = new List<ListCandles>();
        public List<FullListCandles> full_list_candles = new List<FullListCandles>();
        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            Chart();
            Clients();
            CheckTimeFiles();
            LIST_SUMBOLS.ItemsSource = list_sumbols_name;
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
        }



        #region - Create Collection Candles
        private void CollectionCandlestick()
        {
            try
            {
                int count = Convert.ToInt32(COUNT_CANDLES.Text);
                if (count > 0 && count < 500)
                {
                    foreach(var it in list_sumbols_name)
                    {
                        Klines(it, klines_count: count);
                    }
                    SortListCandlestick(count);
                }
            }
            catch (Exception e)
            {
                ErrorText.Add($"CollectionCandlestick {e.Message}");
            }
        }
        public void WriteToFile()
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "times");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach (FullListCandles it in full_list_candles)
                {
                    string name_file = it.number.ToString();
                    if (!File.Exists(path + "/" + name_file))
                    {
                        string json = JsonConvert.SerializeObject(it.list);
                        File.AppendAllText(path + "/" + name_file, json);
                    }
                }
                list_listcandles.Clear();
            }
            catch (Exception e)
            {
                ErrorText.Add($"WriteToFile {e.Message}");
            }
        }
        private void SortListCandlestick(int count)
        {
            
            for(int i = 0; i < count; i++)
            {
                List<Candle> list_candle_sorted = new List<Candle>();
                foreach (var it in list_sumbols_name)
                {
                    foreach(var iterator in list_listcandles)
                    {
                        if(it == iterator.Symbol)
                        {
                            list_candle_sorted.Add(iterator.listKlines[i]);
                        }
                    }
                }
                full_list_candles.Add(new FullListCandles(i, list_candle_sorted));
            }
            WriteToFile();
        }
        #endregion

        #region - List Sumbols -
        private void GetSumbolName()
        {
            foreach (var it in ListSymbols())
            {
                list_sumbols_name.Add(it.Symbol);
            }
            list_sumbols_name.Sort();
            LIST_SUMBOLS.Items.Refresh();
            LIST_SUMBOLS.SelectedIndex = 0;
        }
        public List<BinancePrice> ListSymbols()
        {
            try
            {
                var result = socket.futures.ExchangeData.GetPricesAsync().Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                return result.Data.ToList();
            }
            catch (Exception e)
            {
                ErrorText.Add($"ListSymbols {e.Message}");
                return ListSymbols();
            }
        }

        #endregion

        #region - Canged Time Files -
        private void CheckTimeFiles()
        {
            try
            {
                FileSystemWatcher files_watcher = new FileSystemWatcher();
                files_watcher.Path = System.IO.Path.Combine(Environment.CurrentDirectory, "times");
                files_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                files_watcher.Changed += new FileSystemEventHandler(OnChangedFiles);
                files_watcher.Filter = "*.*";
                files_watcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                ErrorText.Add($"CheckTimeFiles {e.Message}");
            }

        }
        private void OnChangedFiles(object source, FileSystemEventArgs e)
        {
            // загрузка графика
            ErrorText.Add("Super");
        }
        #endregion

        #region - Server Time -
        private void Button_StartConnect(object sender, RoutedEventArgs e)
        {
            GetServerTime();
            GetSumbolName();
        }
        private void Button_StartConnect1(object sender, RoutedEventArgs e)
        {
            CollectionCandlestick();
        }
        private void GetServerTime()
        {
            try
            {
                var result = socket.futures.ExchangeData.GetServerTimeAsync().Result;
                if (!result.Success) ErrorText.Add("Error GetServerTimeAsync");
                else
                {
                    SERVER_TIME.Text = result.Data.ToShortTimeString();
                }
            }

            catch (Exception e)
            {
                ErrorText.Add($"GetServerTime {e.Message}");
            }
        }
        #endregion

        #region - Chart -
        private void Chart()
        {
            // add some random candles
            OHLC[] prices = DataGen.RandomStockPrices(null, 100, TimeSpan.FromMinutes(5));
            double[] xs = prices.Select(x => x.DateTime.ToOADate()).ToArray();
            var candlePlot = plt.Plot.AddCandlesticks(prices);
            candlePlot.YAxisIndex = 1;

            plt.Plot.XAxis.DateTimeFormat(true);

            // add SMA indicators for 8 and 20 days
            var sma8 = candlePlot.GetSMA(8);
            var sma20 = candlePlot.GetSMA(20);
            var sma8plot = plt.Plot.AddScatterLines(sma8.xs, sma8.ys, Color.Cyan, 2, label: "8 day SMA");
            var sma20plot = plt.Plot.AddScatterLines(sma20.xs, sma20.ys, Color.Yellow, 2, label: "20 day SMA");
            sma8plot.YAxisIndex = 1;
            sma20plot.YAxisIndex = 1;

            // customize candle styling
            candlePlot.ColorDown = ColorTranslator.FromHtml("#00FF00");
            candlePlot.ColorUp = ColorTranslator.FromHtml("#FF0000");

            // customize figure styling
            plt.Plot.Layout(padding: 12);
            plt.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
            plt.Plot.Frameless();
            plt.Plot.XAxis.TickLabelStyle(color: Color.White);
            plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            // hide the left axis and show a right axis
            plt.Plot.YAxis.Ticks(false);
            plt.Plot.YAxis.Grid(false);
            plt.Plot.YAxis2.Ticks(true);
            plt.Plot.YAxis2.Grid(true);
            plt.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
            plt.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            // customize the legend style
            var legend = plt.Plot.Legend();
            legend.FillColor = Color.Transparent;
            legend.OutlineColor = Color.Transparent;
            legend.Font.Color = Color.White;
            legend.Font.Bold = true;
        }
        #endregion

        #region - Candles -
        public void Klines(string Symbol, DateTime? start_time = null, DateTime? end_time = null, int? klines_count = null)
        {
            try
            {
                var result = socket.futures.ExchangeData.GetKlinesAsync(symbol: Symbol, interval: KlineInterval.OneMinute, startTime: start_time, endTime: end_time, limit: klines_count).Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                else
                {
                    List<Candle> list = new List<Candle>();
                    foreach (var it in result.Data.ToList())
                    {
                        list.Add(new Candle(Symbol, it.OpenTime, it.OpenPrice, it.HighPrice, it.LowPrice, it.ClosePrice, it.CloseTime));                                 // список монет с ценами
                    }
                    list_listcandles.Add(new ListCandles(Symbol, list));
                }
            }
            catch (Exception e)
            {
                ErrorText.Add($"Klines {e.Message}");
            }
        }
        
        #endregion

        #region - Login -
        private void Button_Save(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = CLIENT_NAME.Text;
                string api = API_KEY.Text;
                string key = SECRET_KEY.Text;
                if (name != "" && api != "" && key != "")
                {
                    string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    if (!File.Exists(path + "/" + CLIENT_NAME.Text))
                    {
                        CLIENT_NAME.Text = "";
                        API_KEY.Text = "";
                        SECRET_KEY.Text = "";
                        Client client = new Client(name, api, key);
                        string json = JsonConvert.SerializeObject(client);
                        File.WriteAllText(path + "/" + name, json);
                        Clients();
                    }
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"Button_Save {c.Message}");
            }
        }
        private void Clients()
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                List<string> filesDir = (from a in Directory.GetFiles(path) select System.IO.Path.GetFileNameWithoutExtension(a)).ToList();
                if (filesDir.Count > 0)
                {
                    ClientList file_list = new ClientList(filesDir);
                    BOX_NAME.ItemsSource = file_list.BoxNameContent;
                    BOX_NAME.SelectedItem = file_list.BoxNameContent[0];
                }
            }
            catch (Exception e)
            {
                ErrorText.Add($"Clients {e.Message}");
            }
        }
        private void Button_Login(object sender, RoutedEventArgs e)
        {
            try
            {
                string api = API_KEY.Text;
                string key = SECRET_KEY.Text;
                if (api != "" && key != "")
                {
                    CLIENT_NAME.Text = "";
                    API_KEY.Text = "";
                    SECRET_KEY.Text = "";
                    socket = new Socket(api, key);
                    Login_Click();
                }
                else if (BOX_NAME.Text != "")
                {
                    string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                    string json = File.ReadAllText(path + "\\" + BOX_NAME.Text);
                    Client client = JsonConvert.DeserializeObject<Client>(json);
                    socket = new Socket(client.ApiKey, client.SecretKey);
                    Login_Click();
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"Button_Login {c.Message}");
            }
        }
        private void Login_Click()
        {
            LOGIN_GRID.Visibility = Visibility.Hidden;
            EXIT_GRID.Visibility = Visibility.Visible;
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            socket = null;
        }
        #endregion

        #region - Error -
        // ------------------------------------------------------- Start Error Text Block --------------------------------------
        private void ErrorWatcher()
        {
            try
            {
                FileSystemWatcher error_watcher = new FileSystemWatcher();
                error_watcher.Path = ErrorText.Directory();
                error_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                error_watcher.Changed += new FileSystemEventHandler(OnChanged);
                error_watcher.Filter = ErrorText.Patch();
                error_watcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                ErrorText.Add($"ErrorWatcher {e.Message}");
            }
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { ERROR_LOG.Text = File.ReadAllText(ErrorText.FullPatch()); }));
        }
        private void Button_ClearErrors(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(ErrorText.FullPatch(), "");
        }
        // ------------------------------------------------------- End Error Text Block ----------------------------------------
        #endregion
    }
}
