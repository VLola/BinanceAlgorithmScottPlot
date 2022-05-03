using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.Binance
{
    public class ClientList
    {
        public ObservableCollection<string> BoxNameContent { get; set; }
        public ClientList(List<string> list)
        {
            BoxNameContent = new ObservableCollection<string>();
            foreach (var it in list)
            {
                BoxNameContent.Add(it);
            }
        }
    }
}
