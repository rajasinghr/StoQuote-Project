using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models
{
    public class StockWithValue
    {

        public string symbol { get; set; }

        public float? week52Value { get; set; }

        public string companyName { get; set; }
        public string primaryExchange { get; set; }
        public string sector { get; set; }
        public string calculationPrice { get; set; }
        public float? open { get; set; }
        public float? close { get; set; }
        public float? high { get; set; }
        public float? low { get; set; }
        public float? latestPrice { get; set; }
        public float? week52High { get; set; }
        public float? week52Low { get; set; }

        
    }
}
