using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Candlestick
{
    public class ListCandles
    {
        public string symbol { get; set; }
        public List<Candle> listKlines { get; set; }
        public ListCandles(string symbol, List<Candle> listKlines)
        {
            this.symbol = symbol;
            this.listKlines = listKlines;
        }
    }
}
