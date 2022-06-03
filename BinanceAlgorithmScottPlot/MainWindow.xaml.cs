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
using Binance.Net.Enums;
using ScottPlot;
using System.Drawing;
using Binance.Net.Objects.Models.Spot;
using ScottPlot.Plottable;
using BinanceAlgorithmScottPlot.ConnectDB;
using System.Windows.Threading;
using Binance.Net.Objects.Models.Futures;
using BinanceAlgorithmScottPlot.Interval;
using BinanceAlgorithmScottPlot.Model;
using BinanceAlgorithmScottPlot.Objects;

namespace BinanceAlgorithmScottPlot
{
    public partial class MainWindow : Window
    {
        public Variables variables { get; set; } = new Variables();
        public int LINE { get; set; } = 2;
        public string API_KEY { get; set; } = "";
        public string SECRET_KEY { get; set; } = "";
        public string CLIENT_NAME { get; set; } = "";
        public int COUNT_CANDLES { get; set; } = 100;
        public int SMA_LONG { get; set; } = 20;
        public decimal USDT_BET { get; set; } = 10;
        bool ONLINE_CHART { get; set; } = false;
        bool START_BET { get; set; } = false;
        public double BOLINGER_TP { get; set; } = 100;
        public double BOLINGER_SL { get; set; } = 100;
        public Socket socket;
        public List<string> list_sumbols_name = new List<string>();
        public FinancePlot candlePlot;
        public ScatterPlot sma_long_plot;
        public ScatterPlot bolinger_lower;
        public ScatterPlot bolinger_upper;
        public ScatterPlot line_scatter;
        public ScatterPlot order_long_open_plot;
        public ScatterPlot order_long_close_plot;
        public ScatterPlot order_short_open_plot;
        public ScatterPlot order_short_close_plot;
        public List<ScatterPlot> order_long_lines_vertical = new List<ScatterPlot>();
        public List<ScatterPlot> order_long_lines_horisontal = new List<ScatterPlot>();
        public List<ScatterPlot> order_short_lines_vertical = new List<ScatterPlot>();
        public List<ScatterPlot> order_short_lines_horisontal = new List<ScatterPlot>();
        public KlineInterval interval_time = KlineInterval.OneMinute;
        public TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerMinute);
        public List<HistoryOrder> history_order = new List<HistoryOrder>();

        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            Chart();
            Clients();
            HISTORY_ORDER.ItemsSource = history_order;
            INTERVAL_TIME.ItemsSource = IntervalCandles.Intervals();
            INTERVAL_TIME.SelectedIndex = 0;
            LIST_SYMBOLS.ItemsSource = list_sumbols_name;
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            this.DataContext = this;

            //Create Table BinanceFuturesOrders
            using (ModelBinanceFuturesOrder context = new ModelBinanceFuturesOrder())
            {
                context.BinanceFuturesOrders.Create();
            }
            //Create Table HistoryOrders
            using (ModelHistoryOrder context = new ModelHistoryOrder())
            {
                context.HistoryOrders.Create();
            }
            //Create Table Candles
            using (ModelCandle context = new ModelCandle())
            {
                context.Candles.Create();
            }
        }

        #region - Event CheckBox -
        private void ONLINE_CHART_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = e.Source as CheckBox;
            ONLINE_CHART = (bool)box.IsChecked;
        }
        private void START_BET_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = e.Source as CheckBox;
            START_BET = (bool)box.IsChecked;
        }
        #endregion

        #region - Open order, close order -

        public decimal quantity_bet;
        public long bet_order_id = 0;
        private void LongBet_Click(object sender, RoutedEventArgs e)
        {
            string symbol = LIST_SYMBOLS.Text;
            if (bet_order_id == 0)
            {
                if (USDT_BET > 0m && variables.PRICE_SYMBOL > 0m)
                {
                    quantity_bet = Math.Round(USDT_BET / variables.PRICE_SYMBOL, 1);
                    bet_order_id = Algorithm.Algorithm.OpenOrder(socket, symbol, quantity_bet, PositionSide.Long);
                }
            }
        }
        private void ShortBet_Click(object sender, RoutedEventArgs e)
        {
            string symbol = LIST_SYMBOLS.Text;
            if (bet_order_id == 0)
            {
                if (USDT_BET > 0m && variables.PRICE_SYMBOL > 0m)
                {
                    quantity_bet = Math.Round(USDT_BET / variables.PRICE_SYMBOL, 1);
                    bet_order_id = Algorithm.Algorithm.OpenOrder(socket, symbol, quantity_bet, PositionSide.Short);
                }
            }
        }
        private void CloseOrder_Click(object sender, RoutedEventArgs e)
        {
            string symbol = LIST_SYMBOLS.Text;
            if (bet_order_id != 0) bet_order_id = Algorithm.Algorithm.CloseOrder(socket, symbol, bet_order_id, quantity_bet);
        }
        #endregion

        #region - Trede History -
        private void TAB_CONTROL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            History();
            double sum_total = 0;
            int count_orders = 0;
            foreach (var it in ConnectHistoryOrder.Get())
            {
                history_order.Insert(0, it);
                sum_total += it.total;
                count_orders++;
            }
            COUNT_ORDERS.Content = count_orders;
            SUM_TOTAL.Content = sum_total;
            if (sum_total > 0) SUM_TOTAL.Foreground = System.Windows.Media.Brushes.Green;
            else if (sum_total < 0) SUM_TOTAL.Foreground = System.Windows.Media.Brushes.Red;
            HISTORY_ORDER.Items.Refresh();
        }

        private void History()
        {
            try
            {
                history_order.Clear();
                ConnectHistoryOrder.DeleteAll();
                List<BinanceFuturesOrder> orders = ConnectOrder.Get();
                int i = 0;
                foreach (var it in ConnectOrder.Get())
                {
                    if (it.PositionSide == PositionSide.Long && it.Side == OrderSide.Sell)
                    {
                        ConnectHistoryOrder.Insert(new HistoryOrder(it.CreateTime, it.Symbol, Convert.ToDouble(orders[i - 1].AvgPrice), Convert.ToDouble(it.AvgPrice), Convert.ToDouble(orders[i - 1].QuoteQuantityFilled), Convert.ToDouble(it.QuoteQuantityFilled), it.PositionSide));
                    }
                    else if (it.PositionSide == PositionSide.Short && it.Side == OrderSide.Buy)
                    {
                        ConnectHistoryOrder.Insert(new HistoryOrder(it.CreateTime, it.Symbol, Convert.ToDouble(orders[i - 1].AvgPrice), Convert.ToDouble(it.AvgPrice), Convert.ToDouble(orders[i - 1].QuoteQuantityFilled), Convert.ToDouble(it.QuoteQuantityFilled), it.PositionSide));
                    }
                    i++;
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"History {c.Message}");
            }
        }
        #endregion

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
            try
            {
                long_open_order_x.Clear();
                long_open_order_y.Clear();
                long_close_order_x.Clear();
                long_close_order_y.Clear();
                short_open_order_x.Clear();
                short_open_order_y.Clear();
                short_close_order_x.Clear();
                short_close_order_y.Clear();
                bool check_one = false;
                string symbol = LIST_SYMBOLS.Text;
                if (symbol != "")
                {
                    ConnectOrder.DeleteAll();
                    foreach (var it in Algorithm.AlgorithmBet.InfoOrder(socket, symbol, start_time))
                    {

                        if (it.PositionSide == PositionSide.Long && it.Side == OrderSide.Buy)
                        {
                            long_open_order_x.Add(it.CreateTime.ToOADate());
                            long_open_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                            check_one = true;
                            ConnectOrder.Insert(it);
                        }
                        else if (it.PositionSide == PositionSide.Long && it.Side == OrderSide.Sell && check_one)
                        {
                            long_close_order_x.Add(it.CreateTime.ToOADate());
                            long_close_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                            ConnectOrder.Insert(it);
                        }
                        else if (it.PositionSide == PositionSide.Short && it.Side == OrderSide.Sell)
                        {
                            short_open_order_x.Add(it.CreateTime.ToOADate());
                            short_open_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                            check_one = true;
                            ConnectOrder.Insert(it);
                        }
                        else if (it.PositionSide == PositionSide.Short && it.Side == OrderSide.Buy && check_one)
                        {
                            short_close_order_x.Add(it.CreateTime.ToOADate());
                            short_close_order_y.Add(Double.Parse(it.AvgPrice.ToString()));
                            ConnectOrder.Insert(it);
                        }
                    }
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"InfoOrderAsunc {c.Message}");
            }
        }
        #endregion

        #region - Event Text Changed -
        private void COUNT_CANDLES_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReloadChart();
        }

        private void INTERVAL_TIME_DropDownClosed(object sender, EventArgs e)
        {
            int index = INTERVAL_TIME.SelectedIndex;
            interval_time = IntervalCandles.Intervals()[index].interval;
            timeSpan = new TimeSpan(IntervalCandles.Intervals()[index].timespan);
            ReloadChart();
        }
        #endregion

        #region - Event SMA -

        private void SMA_LONG_TextChanged(object sender, TextChangedEventArgs e)
        {
            ReloadSmaChart();
        }
        #endregion

        #region - Load Chart -

        public List<OHLC> list_candle_ohlc = new List<OHLC>();
        private void LIST_SYMBOLS_DropDownClosed(object sender, EventArgs e)
        {
            ReloadChart();
        }
        private void LoadingCandlesToChart()
        {
            try
            {
                string symbol = LIST_SYMBOLS.Text;
                if (symbol != "")
                {
                    list_candle_ohlc.Clear();
                    List<Candle> list_candles = new List<Candle>();
                    list_candles = ConnectCandle.Get();
                    foreach (Candle it in list_candles)
                    {
                        list_candle_ohlc.Add(new OHLC(it.Open, it.High, it.Low, it.Close, it.DateTime, it.TimeSpan));
                    }
                    InfoOrderAsunc(list_candle_ohlc[0].DateTime);
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"LoadingCandlesToChart {c.Message}");
            }
        }

        private void ReloadChart()
        {
            if (socket != null && SMA_LONG > 1 && COUNT_CANDLES > 0 && COUNT_CANDLES > SMA_LONG && COUNT_CANDLES < 500)
            {
                StopAsync();
                ConnectCandle.DeleteAll();
                LoadingCandlesToDB();
                if (ONLINE_CHART) StartKlineAsync();
                LoadingCandlesToChart();
                LoadingChart();
                plt.Plot.AxisAuto();
                plt.Refresh();
            }
        }
        private void ReloadSmaChart()
        {
            if (socket != null && SMA_LONG > 1 && COUNT_CANDLES > 0 && COUNT_CANDLES > SMA_LONG && COUNT_CANDLES < 500 && SMA_LONG < list_candle_ohlc.Count - 1)
            {
                plt.Plot.Remove(sma_long_plot);
                plt.Plot.Remove(bolinger_lower);
                plt.Plot.Remove(bolinger_upper);
                sma_long = candlePlot.GetBollingerBands(SMA_LONG);
                sma_long_plot = plt.Plot.AddScatterLines(sma_long.xs, sma_long.ys, Color.Cyan, 2, label: SMA_LONG + " minute SMA");
                sma_long_plot.YAxisIndex = 1;
                bolinger_lower = plt.Plot.AddScatterLines(sma_long.xs, sma_long.lower, Color.Blue, lineStyle: LineStyle.Dash);
                bolinger_lower.YAxisIndex = 1;
                bolinger_upper = plt.Plot.AddScatterLines(sma_long.xs, sma_long.upper, Color.Blue, lineStyle: LineStyle.Dash);
                bolinger_upper.YAxisIndex = 1;
                plt.Refresh();
            }
        }
        public (double[] xs, double[] ys, double[] lower, double[] upper) sma_long;
        private void LoadingChart()
        {
            if (COUNT_CANDLES > 0)
            {
                if (SMA_LONG > 1 && SMA_LONG < list_candle_ohlc.Count - 1)
                {

                    plt.Plot.Remove(candlePlot);
                    plt.Plot.Remove(sma_long_plot);
                    plt.Plot.Remove(bolinger_lower);
                    plt.Plot.Remove(bolinger_upper);
                    plt.Plot.Remove(order_long_open_plot);
                    plt.Plot.Remove(order_long_close_plot);
                    plt.Plot.Remove(order_short_open_plot);
                    plt.Plot.Remove(order_short_close_plot);
                    // Candles
                    candlePlot = plt.Plot.AddCandlesticks(list_candle_ohlc.ToArray());
                    candlePlot.YAxisIndex = 1;
                    // Sma
                    sma_long = candlePlot.GetBollingerBands(SMA_LONG);
                    sma_long_plot = plt.Plot.AddScatterLines(sma_long.xs, sma_long.ys, Color.Cyan, 2, label: SMA_LONG + " minute SMA");
                    sma_long_plot.YAxisIndex = 1;
                    // Bolinger lower
                    bolinger_lower = plt.Plot.AddScatterLines(sma_long.xs, sma_long.lower, Color.Blue, lineStyle: LineStyle.Dash);
                    bolinger_lower.YAxisIndex = 1;
                    // Bolinger upper
                    bolinger_upper = plt.Plot.AddScatterLines(sma_long.xs, sma_long.upper, Color.Blue, lineStyle: LineStyle.Dash);
                    bolinger_upper.YAxisIndex = 1;
                    // Orders
                    if (order_long_lines_vertical.Count > 0) foreach (var it in order_long_lines_vertical) plt.Plot.Remove(it);
                    if (order_long_lines_horisontal.Count > 0) foreach (var it in order_long_lines_horisontal) plt.Plot.Remove(it);
                    if (order_short_lines_vertical.Count > 0) foreach (var it in order_short_lines_vertical) plt.Plot.Remove(it);
                    if (order_short_lines_horisontal.Count > 0) foreach (var it in order_short_lines_horisontal) plt.Plot.Remove(it);


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

                }
            }
        }

        #endregion

        #region - Algorithm -
        public long order_id = 0;
        decimal quantity;
        public bool start = false;
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
                if (BOLINGER_TP > 0)
                {
                    double price_bolinger = sma_long.upper[sma_long.upper.Length - 1];
                    double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                    double price = (price_bolinger - price_sma) / 100 * BOLINGER_TP + price_sma;
                    if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close > price) return true;
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
                if (BOLINGER_SL > 0)
                {
                    double price_bolinger = sma_long.lower[sma_long.lower.Length - 1];
                    double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                    double price = price_sma - (price_sma - price_bolinger) / 100 * BOLINGER_SL;
                    if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < price) return true;
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
                if (BOLINGER_TP > 0)
                {
                    double price_bolinger = sma_long.lower[sma_long.lower.Length - 1];
                    double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                    double price = price_sma - (price_sma - price_bolinger) / 100 * BOLINGER_TP;
                    if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close < price) return true;
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
                if (BOLINGER_SL > 0)
                {
                    double price_bolinger = sma_long.upper[sma_long.upper.Length - 1];
                    double price_sma = sma_long.ys[sma_long.ys.Length - 1];
                    double price = (price_bolinger - price_sma) / 100 * BOLINGER_SL + price_sma;
                    if (list_candle_ohlc[list_candle_ohlc.Count - 1].Close > price) return true;
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
                if (START_BET && ONLINE_CHART && order_id == 0)
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


                if (START_BET && ONLINE_CHART && order_id != 0)
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
                if (START_BET && ONLINE_CHART && start && order_id == 0)
                {
                    if (USDT_BET > 0m && variables.PRICE_SYMBOL > 0m)
                    {
                        quantity = Math.Round(USDT_BET / variables.PRICE_SYMBOL, 1);

                        order_id = Algorithm.AlgorithmBet.OpenOrder(socket, symbol, quantity, list_candle_ohlc[list_candle_ohlc.Count - 1].Close, sma_long.ys[sma_long.ys.Length - 1]);

                        start = false;
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
        private void LoadingCandlesToDB()
        {
            try
            {
                string symbol = LIST_SYMBOLS.Text;
                if (symbol != "")
                {
                    Klines(symbol, klines_count: COUNT_CANDLES);
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"LoadingCandlesToDB {c.Message}");
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
        public Candle candle = new Candle();
        public void StartKlineAsync()
        {
            StartPriceAsync();
            socket.socketClient.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(LIST_SYMBOLS.Text, interval_time, Message =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    candle.DateTime = Message.Data.Data.OpenTime;
                    candle.Open = Decimal.ToDouble(Message.Data.Data.OpenPrice);
                    candle.High = Decimal.ToDouble(Message.Data.Data.HighPrice);
                    candle.Low = Decimal.ToDouble(Message.Data.Data.LowPrice);
                    candle.Close = Decimal.ToDouble(Message.Data.Data.ClosePrice);
                    candle.TimeSpan = timeSpan;
                    variables.PRICE_SYMBOL = Message.Data.Data.ClosePrice;
                    if (!ConnectCandle.Update(candle)) ConnectCandle.Insert(candle);
                    LoadingCandlesToChart();
                    LoadingChart();
                    plt.Refresh();
                }));
            });

        }

        private void StartPriceAsync()
        {
            socket.socketClient.UsdFuturesStreams.SubscribeToMarkPriceUpdatesAsync(symbol: LIST_SYMBOLS.Text, updateInterval: 1000, Message =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    variables.PRICE_SYMBOL = Message.Data.MarkPrice;
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
                        candle.DateTime = it.OpenTime;
                        candle.Open = Decimal.ToDouble(it.OpenPrice);
                        candle.High = Decimal.ToDouble(it.HighPrice);
                        candle.Low = Decimal.ToDouble(it.LowPrice);
                        candle.Close = Decimal.ToDouble(it.ClosePrice);
                        candle.TimeSpan = timeSpan;
                        ConnectCandle.Insert(candle);
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
                if (CLIENT_NAME != "" && API_KEY != "" && SECRET_KEY != "")
                {
                    string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    if (!File.Exists(path + "/" + CLIENT_NAME))
                    {

                        Client client = new Client(CLIENT_NAME, API_KEY, SECRET_KEY);
                        string json = JsonConvert.SerializeObject(client);
                        File.WriteAllText(path + "/" + CLIENT_NAME, json);
                        Clients();
                        CLIENT_NAME = "";
                        API_KEY = "";
                        SECRET_KEY = "";
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
                if (API_KEY != "" && SECRET_KEY != "")
                {

                    socket = new Socket(API_KEY, SECRET_KEY);
                    Login_Click();
                    CLIENT_NAME = "";
                    API_KEY = "";
                    SECRET_KEY = "";
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
