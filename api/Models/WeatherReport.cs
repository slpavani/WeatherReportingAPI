using System;
using System.Collections.Generic;

namespace weatherReport.Models
{
    public class WeatherReport
    {   
        public string City { get; set; }
        public List<Detail> details { get; set; }
    }

    public class Detail
    {
        public DateTime Date { get; set; }

        public int MeanTemperature { get; set; }

    }
}
