using BinanceAlgorithmScottPlot.Objects;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BinanceAlgorithmScottPlot.ConnectDB
{
    public static class ConnectHistoryOrder
    {
        public static string connectionString = @"Data Source=DESKTOP-TBFG5D3\SQLEXPRESS;Initial Catalog=ModelHistoryOrder;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //public static string connectionString = @"Data Source=WIN-D7QGD778252\SQLEXPRESS;Initial Catalog=ModelHistoryOrder;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static long Insert(HistoryOrder order)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Insert(order);
            }
        }
        public static List<HistoryOrder> Get()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                return connection.Query<HistoryOrder>($"SELECT * FROM HistoryOrders").ToList();
            }
        }
        public static void DeleteAll()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.DeleteAll<HistoryOrder>();
            }
        }
        public static bool Update(HistoryOrder order)
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
                connection.Delete(new HistoryOrder() { Id = id });
            }
        }
    }
}
