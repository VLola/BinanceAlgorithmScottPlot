using Binance.Net.Objects.Models.Futures;
using System.Data.Entity;

namespace BinanceAlgorithmScottPlot.Model
{
    public class ModelBinanceFuturesOrder : DbContext
    {
        public ModelBinanceFuturesOrder()
            : base("name=ModelBinanceFuturesOrder")
        {
        }
        public DbSet<BinanceFuturesOrder> BinanceFuturesOrders { get; set; }
    }
}