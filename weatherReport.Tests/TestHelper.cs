using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using weatherReport.Models;

namespace weatherReport.Tests
{
    public class TestHelper
    {
        private IConfiguration _config;
        private SharedMemory _sharedMemory;

        public TestHelper()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(Configuration);
        }

        public IConfiguration Configuration
        {
            get
            {
                if (_config == null)
                {
                    var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", optional: false);
                    _config = builder.Build();
                }

                return _config;
            }
        }

        public WeatherReport weatherReport()
        {
            var report = new WeatherReport
            {
                City = "Vancouver",
                details = new List<Detail>()
                {
                    new Detail {
                    Date = DateTime.Parse("2020-04-17"),
                    MeanTemperature = 10
                    },
                    new Detail {
                    Date = DateTime.Parse("2020-04-18"),
                    MeanTemperature = 20
                    },
                    new Detail {
                    Date = DateTime.Parse("2020-04-19"),
                    MeanTemperature = 17
                    },
                    new Detail {
                    Date = DateTime.Parse("2020-04-25"),
                    MeanTemperature = 16
                    }
                }
            };
            return report;
        }
    }
}
