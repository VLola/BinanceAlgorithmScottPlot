using Binance.Net.Enums;
using System;
using System.Drawing;

namespace BinanceAlgorithmScottPlot.Objects
{
    public class HistoryOrder
    {
        public long Id { get; set; }
        public DateTime date { get; set; }
        public string symbol { get; set; }
        public double open_price { get; set; }
        public double close_price { get; set; }
        public double qty_open { get; set; }
        public double qty_close { get; set; }
        public PositionSide side { get; set; }
        public string side_color { get; set; }
        public double? profit { get; set; }
        public string profit_color { get; set; }
        public double profit_percent { get; set; }
        public string profit_percent_color { get; set; }
        public double commission { get; set; }
        public double total { get; set; }
        public string total_color { get; set; }
        public HistoryOrder() { }
        public HistoryOrder(DateTime date, string symbol, double open_price, double close_price, double qty_open, double qty_close, PositionSide side)
        {
            this.date = date;
            this.symbol = symbol;
            this.open_price = open_price;
            this.close_price = close_price;
            this.qty_open = qty_open;
            this.qty_close = qty_close;
            this.side = side;
            commission = Math.Round((qty_open / 50 / 2 * 0.04) + (qty_close / 50 / 2 * 0.04), 9);
            if (side == PositionSide.Long)
            {
                side_color = "Green";
                profit = Math.Round(qty_close - qty_open, 9);
                profit_percent = Math.Round((qty_close - qty_open) / qty_close * 50 * 100, 2);
            }
            else if (side == PositionSide.Short) 
            {
                side_color = "Red";
                profit = Math.Round(qty_open - qty_close, 9); 
                profit_percent = Math.Round((qty_open - qty_close) / qty_open * 50 * 100, 2);
            }
            else side_color = "White";
            if(profit != null)
            {
                if (profit > 0) profit_color = "Green";
                else if (profit < 0) profit_color = "Red";
                else profit_color = "White";
                if (profit_percent > 0) profit_percent_color = "Green";
                else if (profit_percent < 0) profit_percent_color = "Red";
                else profit_percent_color = "White";
                total = Math.Round((double)profit - commission , 9);
                if (total > 0) total_color = "Green";
                else if (total < 0) total_color = "Red";
                else total_color = "White";
            }
        }
    }
}
