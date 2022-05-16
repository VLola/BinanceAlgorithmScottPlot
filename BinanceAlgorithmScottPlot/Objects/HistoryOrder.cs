using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Objects
{
    public class HistoryOrder
    {
        public long Id { get; set; }
        public DateTime date { get; set; }
        public string symbol { get; set; }
        public double open_price { get; set; }
        public double close_price { get; set; }
        public double qty { get; set; }
        public double qty_execute { get; set; }
        public double qty_usdt { get; set; }
        public PositionSide side { get; set; }
        public double profit { get; set; }
        public double profit_percent { get; set; }
        public double commission { get; set; }
        public double total { get; set; }
        public HistoryOrder() { }
        public HistoryOrder(DateTime date, string symbol, double open_price, double close_price, double qty, double qty_execute, PositionSide side)
        {
            this.date = date;
            this.symbol = symbol;
            this.open_price = open_price;
            this.close_price = close_price;
            this.qty = qty;
            this.qty_execute = qty_execute;
            this.side = side;
        }
    }
}
