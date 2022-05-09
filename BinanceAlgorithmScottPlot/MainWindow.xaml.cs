using BinanceAlgorithmScottPlot.Binance;
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
using ScottPlot.Plottable;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.Net.Interfaces;
using Binance.Net.Clients;
using BinanceAlgorithmScottPlot.ConnectDB;
using BinanceAlgorithmScottPlot.History;

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
        public FinancePlot candlePlot;
        public ScatterPlot sma_long_plot;
        public ScatterPlot sma_short_plot;
        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            Chart();
            Clients();
            CheckTimeFiles();
            LIST_SYMBOLS.ItemsSource = list_sumbols_name;
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            SMA_LONG.TextChanged += SMA_LONG_TextChanged;
            SMA_SHORT.TextChanged += SMA_SHORT_TextChanged;
            START_ASYNC.Click += START_ASYNC_Click;
            STOP_ASYNC.Click += STOP_ASYNC_Click;
            LIST_SYMBOLS.DropDownClosed += LIST_SYMBOLS_DropDownClosed;
            DELETE_TABLE.Click += DELETE_TABLE_Click;
            HistoryList.ItemsSource = history;
        }

        private void DELETE_TABLE_Click(object sender, RoutedEventArgs e)
        {
            Connect.DeleteAll();
        }

        #region - Async klines -

        private void STOP_ASYNC_Click(object sender, RoutedEventArgs e)
        {
            StopAsync();
        }
        private void StopAsync()
        {
            try
            {
                socket.socketClient.UnsubscribeAllAsync();
            }
            catch (Exception c)
            {
                ErrorText.Add($"STOP_ASYNC_Click {c.Message}");
            }
        }
        private void START_ASYNC_Click(object sender, RoutedEventArgs e)
        {
            StartAsync();
        }
        public OHLC_NEW ohlc_item = new OHLC_NEW();
        public int Id;
        public void StartAsync()
        {
            socket.socketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(LIST_SYMBOLS.Text, KlineInterval.OneMinute, Message => {

                Dispatcher.Invoke(new Action(() => {
                    if(ohlc_item.DateTime == Message.Data.Data.OpenTime)Connect.Delete(Id);
                    ohlc_item.DateTime = Message.Data.Data.OpenTime;
                    ohlc_item.Open = Message.Data.Data.OpenPrice;
                    ohlc_item.High = Message.Data.Data.HighPrice;
                    ohlc_item.Low = Message.Data.Data.LowPrice;
                    ohlc_item.Close = Message.Data.Data.ClosePrice;
                    Id = Convert.ToInt32(Connect.Insert(ohlc_item));
                    startLoadChart();
                }));
            });
        }
        #endregion

        #region - Event SMA -
        private void SMA_SHORT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SMA_SHORT.Text != "")
            {
                string text_short = SMA_SHORT.Text;
                int sma_indi_short = Convert.ToInt32(text_short);
                if (sma_indi_short > 1)
                {
                    plt.Plot.Remove(sma_short_plot);
                    var sma_short = candlePlot.GetSMA(sma_indi_short);
                    sma_short_plot = plt.Plot.AddScatterLines(sma_short.xs, sma_short.ys, Color.AntiqueWhite, 2, label: text_short + " minute SMA");
                    sma_short_plot.YAxisIndex = 1;
                    plt.Refresh();
                }
            }
        }

        private void SMA_LONG_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(SMA_LONG.Text != "")
            {
                string text_long = SMA_LONG.Text;
                int sma_indi_long = Convert.ToInt32(text_long);
                if (sma_indi_long > 1)
                {
                    plt.Plot.Remove(sma_long_plot);
                    var sma_long = candlePlot.GetSMA(sma_indi_long);
                    sma_long_plot = plt.Plot.AddScatterLines(sma_long.xs, sma_long.ys, Color.Cyan, 2, label: text_long + " minute SMA");
                    sma_long_plot.YAxisIndex = 1;
                    plt.Refresh();
                }
            }
        }
        #endregion

        #region - Load Chart -

        public List<OHLC> list_candle_ohlc = new List<OHLC>();
        private void LIST_SYMBOLS_DropDownClosed(object sender, EventArgs e)
        {
            
            StopAsync();
            Connect.DeleteAll();
            Refile(); 
            if (online_chart.IsChecked == true) StartAsync();
            startLoadChart();
        }
        private void startLoadChart()
        {
            try
            {
                string symbol = LIST_SYMBOLS.Text;
                if (symbol != "")
                {
                    list_candle_ohlc.Clear();
                    List<OHLC_NEW> olhc_new = new List<OHLC_NEW>();
                    olhc_new = Connect.Get();
                    foreach (OHLC_NEW it in olhc_new)
                    {
                        list_candle_ohlc.Add(new OHLC(Decimal.ToDouble(it.Open), Decimal.ToDouble(it.High), Decimal.ToDouble(it.Low), Decimal.ToDouble(it.Close), it.DateTime, new TimeSpan(TimeSpan.TicksPerMinute)));
                    }
                    LoadChart();
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"ComboBox_SelectionChanged {c.Message}");
            }
        }
        public List<HistoryList> history = new List<HistoryList>();
        public (double[] xs, double[] ys) sma_long;
        public (double[] xs, double[] ys) sma_short;
        private void LoadChart()
        {
            try
            {
                plt.Plot.Remove(candlePlot);
                plt.Plot.Remove(sma_long_plot);
                plt.Plot.Remove(sma_short_plot);
                candlePlot = plt.Plot.AddCandlesticks(list_candle_ohlc.ToArray());
                candlePlot.YAxisIndex = 1;

                if (SMA_LONG.Text != "" && SMA_SHORT.Text != "")
                {
                    string text_long = SMA_LONG.Text;
                    string text_short = SMA_SHORT.Text;
                    int sma_indi_long = Convert.ToInt32(text_long);
                    int sma_indi_short = Convert.ToInt32(text_short);
                    if (sma_indi_long > 1 && sma_indi_short > 1)
                    {
                        sma_long = candlePlot.GetSMA(sma_indi_long);
                        sma_short = candlePlot.GetSMA(sma_indi_short);
                        sma_long_plot = plt.Plot.AddScatterLines(sma_long.xs, sma_long.ys, Color.Cyan, 2, label: text_long + " minute SMA");
                        sma_short_plot = plt.Plot.AddScatterLines(sma_short.xs, sma_short.ys, Color.AntiqueWhite, 2, label: text_short + " minute SMA");
                        sma_long_plot.YAxisIndex = 1;
                        sma_short_plot.YAxisIndex = 1;

                        history.Clear();
                        int count = sma_short.xs.Length - sma_long.xs.Length;

                        int _short = 0;
                        int _long = 0;
                        int _long_short = 0;
                        int long_positive = 0;
                        int short_positive = 0;
                        double price_sma_long = 0;
                        double price_long = 0;
                        double price_short = 0;

                        bool temp_up = false;
                        if (sma_short.ys[0 + count] > sma_long.ys[0]) temp_up = true;

                        for (int i = 0; i < sma_long.xs.Length; i++)
                        {
                            if (sma_short.ys[i + count] > sma_long.ys[i] && !temp_up) 
                            { 
                                if(_long_short > 1)
                                {
                                    price_short += price_sma_long - sma_long.ys[i];
                                    _short++;
                                    if (price_sma_long - sma_long.ys[i] > 0) short_positive++;
                                }
                                price_sma_long = sma_long.ys[i];
                                temp_up = true;
                                _long_short++;
                            }
                            else if (sma_short.ys[i + count] < sma_long.ys[i] && temp_up) 
                            {
                                if (_long_short > 1)
                                {
                                    price_long += sma_long.ys[i] - price_sma_long;
                                    _long++;
                                    if (sma_long.ys[i] - price_sma_long > 0) long_positive++;
                                }
                                price_sma_long = sma_long.ys[i];
                                temp_up = false;
                                _long_short++;
                            }

                        }
                        history.Add(new HistoryList(LIST_SYMBOLS.Text, _long, long_positive, price_long, _short, short_positive, price_short));
                        HistoryList.Items.Refresh();
                    }
                    
                }

                plt.Refresh();
            } 
            catch(Exception e)
            {
                ErrorText.Add($"LoadChart {e.Message}");
            }
        }
        #endregion

        #region - Refile Candles -
        private void Button_Refile(object sender, RoutedEventArgs e)
        {
            Refile();
        }
        private void Refile()
        {
            try
            {
                string symbol = LIST_SYMBOLS.Text;
                int count = Convert.ToInt32(COUNT_CANDLES.Text);
                if (count > 0 && count < 500 && symbol != "")
                {
                    Klines(symbol, klines_count: count);
                }
                else MessageBox.Show("Button_Refile: Не верные условия!");
            }
            catch (Exception c)
            {
                ErrorText.Add($"Refile {c.Message}");
            }
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
            LIST_SYMBOLS.Items.Refresh();
            LIST_SYMBOLS.SelectedIndex = 0;
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
            //// add some random candles
            //OHLC[] prices = DataGen.RandomStockPrices(null, 100, TimeSpan.FromMinutes(5));
            //double[] xs = prices.Select(x => x.DateTime.ToOADate()).ToArray();
            //var candlePlot = plt.Plot.AddCandlesticks(prices);
            //candlePlot.YAxisIndex = 1;

            //plt.Plot.XAxis.DateTimeFormat(true);

            //// add SMA indicators for 8 and 20 days
            //var sma8 = candlePlot.GetSMA(8);
            //var sma20 = candlePlot.GetSMA(20);
            //var sma8plot = plt.Plot.AddScatterLines(sma8.xs, sma8.ys, Color.Cyan, 2, label: "8 day SMA");
            //var sma20plot = plt.Plot.AddScatterLines(sma20.xs, sma20.ys, Color.Yellow, 2, label: "20 day SMA");
            //sma8plot.YAxisIndex = 1;
            //sma20plot.YAxisIndex = 1;

            //// customize candle styling
            //candlePlot.ColorDown = ColorTranslator.FromHtml("#00FF00");
            //candlePlot.ColorUp = ColorTranslator.FromHtml("#FF0000");

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

        #region - Candles Save -
        public void Klines(string Symbol, DateTime? start_time = null, DateTime? end_time = null, int? klines_count = null)
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "symbols");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string path_symbol = path + "\\" + Symbol;
                if (!Directory.Exists(path_symbol)) Directory.CreateDirectory(path_symbol);
                else
                {
                    Directory.Delete(path_symbol, true);
                    Directory.CreateDirectory(path_symbol);
                }

                var result = socket.futures.ExchangeData.GetKlinesAsync(symbol: Symbol, interval: KlineInterval.OneMinute, startTime: start_time, endTime: end_time, limit: klines_count).Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                else
                {
                    foreach (var it in result.Data.ToList())
                    {
                        ohlc_item.DateTime = it.OpenTime;
                        ohlc_item.Open = it.OpenPrice;
                        ohlc_item.High = it.HighPrice;
                        ohlc_item.Low = it.LowPrice;
                        ohlc_item.Close = it.ClosePrice;
                        Id = Convert.ToInt32(Connect.Insert(ohlc_item));
                    }
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
