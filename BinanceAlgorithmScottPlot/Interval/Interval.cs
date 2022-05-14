using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Interval
{
    public class Interval
    {
        public string name { get; set; }
        public KlineInterval interval { get; set; }
        public long timespan { get; set; }
        public override string ToString()
        {
            return name;
        }
    }
}
