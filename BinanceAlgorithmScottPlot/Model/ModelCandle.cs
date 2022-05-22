using BinanceAlgorithmScottPlot.Objects;
using System.Data.Entity;

namespace BinanceAlgorithmScottPlot.Model
{
    public class ModelCandle : DbContext
    {
        public ModelCandle()
            : base("name=ModelCandle")
        {
        }
        public virtual DbSet<Candle> Candles { get; set; }
    }
}