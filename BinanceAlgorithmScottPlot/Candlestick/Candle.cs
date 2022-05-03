﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Candlestick
{
    public class Candle
    {
        public string Sumbol { get; set; }
        public DateTime OpenTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public DateTime CloseTime { get; set; }
        public Candle(string Sumbol, DateTime OpenTime, decimal Open, decimal High, decimal Low, decimal Close, DateTime CloseTime)
        {
            this.Sumbol = Sumbol;
            this.OpenTime = OpenTime;
            this.Open = Open;
            this.High = High;
            this.Low = Low;
            this.Close = Close;
            this.CloseTime = CloseTime;
        }
    }
}
