using weatherReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace weatherReport
{
    public class SharedMemory
    {
        public List<WeatherReport> weatherReports = new List<WeatherReport>();
    }
}
