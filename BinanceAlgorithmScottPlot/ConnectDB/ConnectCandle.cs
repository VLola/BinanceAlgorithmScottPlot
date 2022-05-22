using BinanceAlgorithmScottPlot.Objects;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BinanceAlgorithmScottPlot.ConnectDB
{
    public static class ConnectCandle
    {
        public static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ModelCandle;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static long Insert(Candle candle)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Insert(candle);
            }
        }
        public static List<Candle> Get()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                return connection.Query<Candle>($"SELECT * FROM Candles").ToList();
            }
        }
        public static void DeleteAll()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.DeleteAll<Candle>();
            }
        }
        public static bool Update(Candle candle)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Update(candle);
            }
        }
        public static void Delete(Candle candle)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Delete(candle);
            }
        }
    }
}
