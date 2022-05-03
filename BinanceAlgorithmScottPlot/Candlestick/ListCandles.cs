using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Candlestick
{
    public class ListCandles
    {
        public string Symbol { get; set; }
        public List<Candle> listKlines { get; set; }
        public ListCandles(string Symbol, List<Candle> listKlines)
        {
            this.Symbol = Symbol;
            this.listKlines = listKlines;
        }
    }
}
