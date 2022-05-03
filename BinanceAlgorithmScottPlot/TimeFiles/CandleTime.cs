using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAlgorithmScottPlot.TimeFiles
{
    public static class CandleTime
    {
        public static string Directory()
        {
            return System.IO.Path.Combine(Environment.CurrentDirectory, "times");
        }
        public static string FullPatch()
        {
            return Directory();
        }
    }
}
