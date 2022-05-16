using System;

namespace BinanceAlgorithmScottPlot.Objects
{
    public class OHLC_NEW
    {
        public int Id { get; set; } 
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public DateTime DateTime { get; set; }
    }
}
