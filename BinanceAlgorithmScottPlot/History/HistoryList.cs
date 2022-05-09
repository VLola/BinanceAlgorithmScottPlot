using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.History
{
    public class HistoryList
    {
        public string Symbol { get; set; }
        public int Long { get; set; }
        public int long_positive { get; set; }
        public int Short { get; set; }
        public int short_positive { get; set; }
        public double price_long { get; set; }
        public double price_short { get; set; }
        public HistoryList(string Symbol, int Long, int long_positive, double price_long, int Short, int short_positive, double price_short)
        {
            this.Symbol = Symbol;
            this.Long = Long;
            this.long_positive = long_positive;
            this.Short = Short;
            this.short_positive = short_positive;
            this.price_long = price_long;
            this.price_short = price_short;
        }
    }
}
