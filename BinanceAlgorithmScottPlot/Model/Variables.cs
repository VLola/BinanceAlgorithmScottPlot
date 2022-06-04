using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceAlgorithmScottPlot.Model
{
    public class Variables : INotifyPropertyChanged
    {
        private decimal _PRICE_SYMBOL;
        public decimal PRICE_SYMBOL
        {
            get { return _PRICE_SYMBOL; }
            set
            {
                _PRICE_SYMBOL = value;
                OnPropertyChanged("PRICE_SYMBOL");
            }
        }
        private bool _START_BET;
        public bool START_BET
        {
            get { return _START_BET; }
            set
            {
                _START_BET = value;
                OnPropertyChanged("START_BET");
            }
        }
        private bool _ONLINE_CHART;
        public bool ONLINE_CHART
        {
            get { return _ONLINE_CHART; }
            set
            {
                _ONLINE_CHART = value;
                OnPropertyChanged("ONLINE_CHART");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
