using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Candlestick
{
    public class FullListCandles
    {
        public int number { get; set; }
        public List<Candle> list { get; set; }
        public FullListCandles(int number, List<Candle> list)
        {
            this.number = number;
            this.list = list;
        }
    }
}
