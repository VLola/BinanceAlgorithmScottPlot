using BinanceAlgorithmScottPlot.Binance;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BinanceAlgorithmScottPlot.ConnectDB
{
    public static class ConnectClient
    {
        public static string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ModelClient;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static long Insert(Client client)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Insert(client);
            }
        }
        public static List<Client> Get()
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                return connection.Query<Client>($"SELECT * FROM Clients").ToList();
            }
        }
    }
}
