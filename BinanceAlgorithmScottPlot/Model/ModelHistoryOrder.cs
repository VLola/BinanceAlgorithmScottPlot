using BinanceAlgorithmScottPlot.Objects;
using System.Data.Entity;

namespace BinanceAlgorithmScottPlot.Model
{
    public class ModelHistoryOrder : DbContext
    {
        public ModelHistoryOrder()
            : base("name=ModelHistoryOrder")
        {
        }
        public DbSet<HistoryOrder> HistoryOrders {get; set;}
    }

}