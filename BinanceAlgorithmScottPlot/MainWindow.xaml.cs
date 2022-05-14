using BinanceAlgorithmScottPlot.Binance;
using BinanceAlgorithmScottPlot.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using BinanceAlgorithmScottPlot.Candlestick;
using Binance.Net.Enums;
using ScottPlot;
using System.Drawing;
using Binance.Net.Objects.Models.Spot;
using ScottPlot.Plottable;
using BinanceAlgorithmScottPlot.ConnectDB;
using System.Windows.Threading;
using Binance.Net.Objects.Models.Futures;
using BinanceAlgorithmScottPlot.Interval;

namespace BinanceAlgorithmScottPlot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket socket;
        public IntervalCandles IntervalCandles = new IntervalCandles();
        public List<string> interval = new List<string>();
        public List<string> list_sumbols_name = new List<string>();
        public List<ListCandles> list_listcandles = new List<ListCandles>();
        public List<FullListCandles> full_list_candles = new List<FullListCandles>();
        public FinancePlot candlePlot;
        public ScatterPlot sma_long_plot;
        public ScatterPlot bolinger2;
        public ScatterPlot bolinger3;
        public ScatterPlot order_long_open_plot;
        public ScatterPlot order_long_close_plot;
        public ScatterPlot order_short_open_plot;
        public ScatterPlot order_short_close_plot;
        public List<ScatterPlot> order_long_lines_vertical = new List<ScatterPlot>();
        public List<ScatterPlot> order_long_lines_horisontal = new List<ScatterPlot>();
        public List<ScatterPlot> order_short_lines_vertical = new List<ScatterPlot>();
        public List<ScatterPlot> order_short_lines_horisontal = new List<ScatterPlot>();
        public List<BinanceFuturesUsdtTrade> history;
        public KlineInterval interval_time = KlineInterval.OneMinute;
        public TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerMinute);
        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            Chart();
            Clients();
            INTERVAL_TIME.ItemsSource = IntervalCandles.Intervals;
            INTERVAL_TIME.SelectedIndex = 0;
            LIST_SYMBOLS.ItemsSource = list_sumbols_name;
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            SMA_LONG.TextChanged += SMA_LONG_TextChanged;
            COUNT_CANDLES.TextChanged += COUNT_CANDLES_TextChanged;
            TabHistory.MouseLeftButtonDown += TabHistory_MouseLeftButtonDown;
        }

        private void TabHistory_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //string symbol = LIST_SYMBOLS.Text;
            //var result = socket.futures.ExchangeData.GetAggregatedTradeHistoryAsync(symbol: symbol).Result;
            //if (!result.Success)
            //{
            //    ErrorText.Add($"InfoOrderPositionSide: {result.Error.Message}");
            //}
            //history = result.Data.ToList();
        }

        #region - Coordinate Orders -
        List<double> long_open_order_x = new List<double>();
        List<double> long_open_order_y = new List<double>();
        List<double> long_close_order_x = new List<double>();
        List<double> long_close_order_y = new List<double>();
        List<double> short_open_order_x = new List<double>();
        List<double> short_open_order_y = new List<double>();
        List<double> short_close_order_x = new List<double>();
        List<double> short_close_order_y = new List<double>();
        private void InfoOrderAsunc(DateTime start_time)
        {
            long_open_order_x.Clear();
            long_open_order_y.Clear();
            long_close_order_x.Clear();
            long_close_order_y.Clear();
            short_open_order_x.Clear();
            short_open_order_y.Clear();
            short_close_order_x.Clear();
            short_close_order_y.Clear();

            string symbol = LIST_SYMBOLS.Text;
            if (symbol != "")
            {
                foreach (var it in Algorithm.AlgorithmBet.InfoOrder(socket, symbol, start_time))
                {
                    if (it.PositionSide == PositionSide.Long && it.Side == OrderSide.Buy)
                    {
                        long_open_order_x.Add(it.CreateTime.ToOADate());
                        long_open_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                    }
                    else if (it.PositionSide == PositionSide.Long && it.Side == OrderSide.Sell)
                    {
                        long_close_order_x.Add(it.CreateTime.ToOADate());
                        long_close_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                    }
                    else if (it.PositionSide == PositionSide.Short && it.Side == OrderSide.Sell)
                    {
                        short_open_order_x.Add(it.CreateTime.ToOADate());
                        short_open_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                    }
                    else if (it.PositionSide == PositionSide.Short && it.Side == OrderSide.Buy)
                    {
                        short_close_order_x.Add(it.CreateTime.ToOADate());
                        short_close_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                    }
                }
            }
        }
        #endregion

        #region - Event Text Changed -
        private void COUNT_CANDLES_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text_long = SMA_LONG.Text;
            string count_candles = COUNT_CANDLES.Text;
            if (text_long != "" && count_candles != "")
            {
                if (Convert.ToInt32(count_candles) > Convert.ToInt32(text_long) && Convert.ToInt32(count_candles) < 500)
                {
                    StopAsync();
                    Connect.DeleteAll();
                    LoadCandles();
                    if (ONLINE_CHART.IsChecked == true) StartKlineAsync();
                    startLoadChart();
                }
            }
        }

        private void INTERVAL_TIME_DropDownClosed(object sender, EventArgs e)
        {
            int index = INTERVAL_TIME.SelectedIndex;
            interval_time = IntervalCandles.Intervals[index].interval;
            timeSpan = new TimeSpan(IntervalCandles.Intervals[index].timespan);
            StopAsync();
            Connect.DeleteAll();
            LoadCandles();
            if (ONLINE_CHART.IsChecked == true) StartKlineAsync();
            startLoadChart();
        }
        #endregion

        #region - Event SMA -

        private void SMA_LONG_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadChart();
        }
        #endregion

        #region - Load Chart -

        public List<OHLC> list_candle_ohlc = new List<OHLC>();
        private void LIST_SYMBOLS_DropDownClosed(object sender, EventArgs e)
        {
            StopAsync();
            Connect.DeleteAll();
            LoadCandles();
            if (ONLINE_CHART.IsChecked == true) StartKlineAsync();
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
                        list_candle_ohlc.Add(new OHLC(Decimal.ToDouble(it.Open), Decimal.ToDouble(it.High), Decimal.ToDouble(it.Low), Decimal.ToDouble(it.Close), it.DateTime, timeSpan));
                    }
                    InfoOrderAsunc(list_candle_ohlc[0].DateTime);
                    LoadChart();
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"ComboBox_SelectionChanged {c.Message}");
            }
        }

        public (double[] xs, double[] ys, double[] lower, double[] upper) sma_long;
        private void LoadChart()
        {
            string text_long = SMA_LONG.Text;
            string count_candles = COUNT_CANDLES.Text;
            if (text_long != "" && count_candles != "")
            {
                int sma_indi_long = Convert.ToInt32(text_long);
                if (sma_indi_long > 1 && sma_indi_long < Convert.ToInt32(count_candles))
                {
                    plt.Plot.Remove(candlePlot);
                    plt.Plot.Remove(sma_long_plot);
                    plt.Plot.Remove(order_long_open_plot);
                    plt.Plot.Remove(order_long_close_plot);
                    plt.Plot.Remove(order_short_open_plot);
                    plt.Plot.Remove(order_short_close_plot);
                    if (order_long_lines_vertical.Count > 0) foreach (var it in order_long_lines_vertical) plt.Plot.Remove(it);
                    if (order_long_lines_horisontal.Count > 0) foreach (var it in order_long_lines_horisontal) plt.Plot.Remove(it);
                    if (order_short_lines_vertical.Count > 0) foreach (var it in order_short_lines_vertical) plt.Plot.Remove(it);
                    if (order_short_lines_horisontal.Count > 0) foreach (var it in order_short_lines_horisontal) plt.Plot.Remove(it);
                    plt.Plot.Remove(bolinger2);
                    plt.Plot.Remove(bolinger3);
                    candlePlot = plt.Plot.AddCandlesticks(list_candle_ohlc.ToArray());
                    candlePlot.YAxisIndex = 1;
                    sma_long = candlePlot.GetBollingerBands(sma_indi_long);
                    sma_long_plot = plt.Plot.AddScatterLines(sma_long.xs, sma_long.ys, Color.Cyan, 2, label: text_long + " minute SMA");
                    sma_long_plot.YAxisIndex = 1;
                    bolinger2 = plt.Plot.AddScatterLines(sma_long.xs, sma_long.lower, Color.Blue, lineStyle: LineStyle.Dash);
                    bolinger2.YAxisIndex = 1;
                    bolinger3 = plt.Plot.AddScatterLines(sma_long.xs, sma_long.upper, Color.Blue, lineStyle: LineStyle.Dash);
                    bolinger3.YAxisIndex = 1;
                    if (long_close_order_x.Count != 0 && long_close_order_y.Count != 0)
                    {
                        order_long_close_plot = plt.Plot.AddScatter(long_close_order_x.ToArray(), long_close_order_y.ToArray(), color: Color.Orange, lineWidth: 0, markerSize: 10, markerShape: MarkerShape.eks);
                        order_long_close_plot.YAxisIndex = 1;
                        order_long_lines_vertical.Clear();
                        for (int i = 0; i < long_close_order_x.Count; i++)
                        {
                            double[] x = { long_open_order_x[i], long_open_order_x[i] };
                            double[] y = { long_open_order_y[i], long_close_order_y[i] };
                            ScatterPlot scatter = plt.Plot.AddScatterLines(x, y, Color.Orange, lineStyle: LineStyle.Dash);
                            scatter.YAxisIndex = 1;
                            order_long_lines_vertical.Add(scatter);
                        }
                        order_long_lines_horisontal.Clear();
                        for (int i = 0; i < long_close_order_x.Count; i++)
                        {
                            double[] x = { long_open_order_x[i], long_close_order_x[i] };
                            double[] y = { long_close_order_y[i], long_close_order_y[i] };
                            ScatterPlot scatter = plt.Plot.AddScatterLines(x, y, Color.Orange, lineStyle: LineStyle.Dash);
                            scatter.YAxisIndex = 1;
                            order_long_lines_horisontal.Add(scatter);
                        }
                    }
                    if (long_open_order_x.Count != 0 && long_open_order_y.Count != 0)
                    {
                        order_long_open_plot = plt.Plot.AddScatter(long_open_order_x.ToArray(), long_open_order_y.ToArray(), color: Color.Green, lineWidth: 0, markerSize: 8);
                        order_long_open_plot.YAxisIndex = 1;
                    }
                    if (short_close_order_x.Count != 0 && short_close_order_y.Count != 0)
                    {
                        order_short_close_plot = plt.Plot.AddScatter(short_close_order_x.ToArray(), short_close_order_y.ToArray(), color: Color.Orange, lineWidth: 0, markerSize: 10, markerShape: MarkerShape.eks);
                        order_short_close_plot.YAxisIndex = 1;
                        order_short_lines_vertical.Clear();
                        for (int i = 0; i < short_close_order_x.Count; i++)
                        {
                            double[] x = { short_close_order_x[i], short_close_order_x[i] };
                            double[] y = { short_open_order_y[i], short_close_order_y[i] };
                            ScatterPlot scatter = plt.Plot.AddScatterLines(x, y, Color.Orange, lineStyle: LineStyle.Dash);
                            scatter.YAxisIndex = 1;
                            order_short_lines_vertical.Add(scatter);
                        }
                        order_short_lines_horisontal.Clear();
                        for (int i = 0; i < short_close_order_x.Count; i++)
                        {
                            double[] x = { short_open_order_x[i], short_close_order_x[i] };
                            double[] y = { short_open_order_y[i], short_open_order_y[i] };
                            ScatterPlot scatter = plt.Plot.AddScatterLines(x, y, Color.Orange, lineStyle: LineStyle.Dash);
                            scatter.YAxisIndex = 1;
                            order_short_lines_horisontal.Add(scatter);
                        }
                    }
                    if (short_open_order_x.Count != 0 && short_open_order_y.Count != 0)
                    {
                        order_short_open_plot = plt.Plot.AddScatter(short_open_order_x.ToArray(), short_open_order_y.ToArray(), color: Color.DarkRed, lineWidth: 0, markerSize: 8);
                        order_short_open_plot.YAxisIndex = 1;
                    }



                    StartAlgorithm();
                    plt.Refresh();
                }
            }
        }

        #endregion

        #region - Algorithm -
        public long order_id = 0;
        decimal quantity;
        public bool start;
        public bool position;
        public bool temp_position;
        public bool start_programm = true;

        #region - Check change position (Long, Short) -
        private void Position()
        {
            try
            {
                if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < sma_long.ys[sma_long.ys.Length - 1]) position = false;
                else position = true;
            }
            catch (Exception c)
            {
                ErrorText.Add($"Position {c.Message}");
            }
        }
        private void TempPosition()
        {
            try
            {
                if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < sma_long.ys[sma_long.ys.Length - 1]) temp_position = false;
                else temp_position = true;
            }
            catch (Exception c)
            {
                ErrorText.Add($"TempPosition {c.Message}");
            }
        }

        #endregion

        #region - Check SL TP -
        private bool PriceBolingerLongTP()
        {
            try
            {
                string bolinger_text = BOLINGER_TP.Text;
                if (bolinger_text != "")
                {
                    double bolinger_text_double = Double.Parse(bolinger_text);
                    if (bolinger_text_double > 0)
                    {
                        double price_bolinger = sma_long.upper[sma_long.upper.Length - 1];
                        double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                        double price = (price_bolinger - price_sma) / 100 * bolinger_text_double + price_sma;
                        if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close > price) return true;
                        else return false;
                    }
                    else return false;
                }
                else return false;

            }
            catch (Exception c)
            {
                ErrorText.Add($"PriceBolingerLongTP {c.Message}");
                return false;
            }
        }
        private bool PriceBolingerLongSL()
        {
            try
            {
                string bolinger_text = BOLINGER_SL.Text;
                if (bolinger_text != "")
                {
                    double bolinger_text_double = Double.Parse(bolinger_text);
                    if (bolinger_text_double > 0)
                    {
                        double price_bolinger = sma_long.lower[sma_long.lower.Length - 1];
                        double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                        double price = price_sma - (price_sma - price_bolinger) / 100 * bolinger_text_double;
                        if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < price) return true;
                        else return false;
                    }
                    else return false;
                }
                else return false;

            }
            catch (Exception c)
            {
                ErrorText.Add($"PriceBolingerLongSL {c.Message}");
                return false;
            }
        }
        private bool PriceBolingerShortTP()
        {
            try
            {
                string bolinger_text = BOLINGER_TP.Text;
                if (bolinger_text != "")
                {
                    double bolinger_text_double = Double.Parse(bolinger_text);
                    if (bolinger_text_double > 0)
                    {
                        double price_bolinger = sma_long.lower[sma_long.lower.Length - 1];
                        double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                        double price = price_sma - (price_sma - price_bolinger) / 100 * bolinger_text_double;
                        if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < price) return true;
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            catch (Exception c)
            {
                ErrorText.Add($"PriceBolingerShortTP {c.Message}");
                return false;
            }
        }
        private bool PriceBolingerShortSL()
        {
            try
            {
                string bolinger_text = BOLINGER_SL.Text;
                if (bolinger_text != "")
                {
                    double bolinger_text_double = Double.Parse(bolinger_text);
                    if (bolinger_text_double > 0)
                    {
                        double price_bolinger = sma_long.upper[sma_long.upper.Length - 1];
                        double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                        double price = (price_bolinger - price_sma) / 100 * bolinger_text_double + price_sma;
                        if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close > price) return true;
                        else return false;
                    }
                    else return false;
                }
                else return false;
            }
            catch (Exception c)
            {
                ErrorText.Add($"PriceBolingerShortSL {c.Message}");
                return false;
            }
        }
        #endregion

        private void StartAlgorithm()
        {
            try
            {
                if (START_BET.IsChecked == true && ONLINE_CHART.IsChecked == true && order_id == 0)
                {
                    Position();

                    if (start_programm)
                    {
                        TempPosition();
                        start_programm = false;
                    }

                    if (position == true && temp_position == false) start = true;
                    else if (position == false && temp_position == true) start = true;

                    TempPosition();
                }
                string symbol = LIST_SYMBOLS.Text;


                if (START_BET.IsChecked == true && ONLINE_CHART.IsChecked == true && order_id != 0)
                {
                    bool sl = false;
                    bool tp = false;
                    PositionSide position_side = Algorithm.AlgorithmBet.InfoOrderPositionSide(socket, symbol, order_id);
                    if (position_side == PositionSide.Long)
                    {
                        tp = PriceBolingerLongTP();
                        sl = PriceBolingerLongSL();
                    }
                    else if (position_side == PositionSide.Short)
                    {
                        tp = PriceBolingerShortTP();
                        sl = PriceBolingerShortSL();
                    }
                    if (tp || sl)
                    {
                        order_id = Algorithm.AlgorithmBet.CloseOrder(socket, symbol, order_id, quantity);
                        if (order_id == 0) start_programm = true;
                    }
                }
                if (START_BET.IsChecked == true && ONLINE_CHART.IsChecked == true && start && order_id == 0)
                {
                    string usdt_bet = USDT_BET.Text;
                    string price_symbol = PRICE_SYMBOL.Text;
                    if (usdt_bet != "" && price_symbol != "")
                    {
                        decimal usdt = Decimal.Parse(usdt_bet);
                        decimal price = Decimal.Parse(price_symbol);
                        if (usdt > 0m && price > 0m)
                        {
                            quantity = Math.Round(usdt / price, 1);

                            order_id = Algorithm.AlgorithmBet.OpenOrder(socket, symbol, quantity, list_candle_ohlc[list_candle_ohlc.Count - 1].Close, sma_long.ys[sma_long.ys.Length - 1]);

                            if (order_id != 0) start = false;
                        }
                    }

                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"StartAlgorithm {c.Message}");
            }
        }
        #endregion

        #region - Load Candles -
        private void LoadCandles()
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
                ErrorText.Add($"LoadCandles {c.Message}");
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

        #region - Chart -
        private void Chart()
        {
            plt.Plot.Layout(padding: 12);
            plt.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
            plt.Plot.Frameless();
            plt.Plot.XAxis.TickLabelStyle(color: Color.White);
            plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            plt.Plot.YAxis.Ticks(false);
            plt.Plot.YAxis.Grid(false);
            plt.Plot.YAxis2.Ticks(true);
            plt.Plot.YAxis2.Grid(true);
            plt.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
            plt.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            var legend = plt.Plot.Legend();
            legend.FillColor = Color.Transparent;
            legend.OutlineColor = Color.Transparent;
            legend.Font.Color = Color.White;
            legend.Font.Bold = true;
        }
        #endregion

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
        public OHLC_NEW ohlc_item = new OHLC_NEW();
        public int Id;
        public void StartKlineAsync()
        {
            StartPriceAsync();
            socket.socketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(LIST_SYMBOLS.Text, interval_time, Message =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (ohlc_item.DateTime == Message.Data.Data.OpenTime) Connect.Delete(Id);
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

        private void StartPriceAsync()
        {
            socket.socketClient.UsdFuturesStreams.SubscribeToMarkPriceUpdatesAsync(symbol: LIST_SYMBOLS.Text, updateInterval: 1000, Message =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    PRICE_SYMBOL.Text = Message.Data.MarkPrice.ToString();
                }));
            });
        }
        #endregion

        #region - Candles Save -
        public void Klines(string Symbol, DateTime? start_time = null, DateTime? end_time = null, int? klines_count = null)
        {
            try
            {
                var result = socket.futures.ExchangeData.GetKlinesAsync(symbol: Symbol, interval: interval_time, startTime: start_time, endTime: end_time, limit: klines_count).Result;
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
            GetSumbolName();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            socket = null;
            list_sumbols_name.Clear();
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
