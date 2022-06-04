using System.ComponentModel.DataAnnotations;

namespace BinanceAlgorithmScottPlot.Binance
{
    public class Client
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        [Key]
        public string ClientName { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public Client() { }
        public Client(string ClientName, string ApiKey, string SecretKey)
        {
            this.ClientName = ClientName;
            this.ApiKey = ApiKey;
            this.SecretKey = SecretKey;
        }
    }
}
