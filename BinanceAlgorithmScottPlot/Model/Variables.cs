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
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
