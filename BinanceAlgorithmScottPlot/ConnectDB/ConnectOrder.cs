using Binance.Net.Objects.Models.Futures;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BinanceAlgorithmScottPlot.ConnectDB
{
    public static class ConnectOrder
    {
        public static string connectionString = @"Data Source=DESKTOP-TBFG5D3\SQLEXPRESS;Initial Catalog=ModelBinanceFuturesOrder;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //public static string connectionString = @"Data Source=WIN-D7QGD778252\SQLEXPRESS;Initial Catalog=ModelBinanceFuturesOrder;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static long Insert(BinanceFuturesOrder order)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Insert(order);
            }
        }
        public static List<BinanceFuturesOrder> Get()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                return connection.Query<BinanceFuturesOrder>($"SELECT * FROM BinanceFuturesOrders").ToList();
            }
        }
        public static void DeleteAll()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.DeleteAll<BinanceFuturesOrder>();
            }
        }
        public static bool Update(BinanceFuturesOrder order)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Update(order);
            }
        }
        public static void Delete(long id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Delete(new BinanceFuturesOrder() { Id = id });
            }
        }
    }
}
