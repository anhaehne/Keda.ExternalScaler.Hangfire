﻿using System.Collections.Generic;
using Serilog.Events;

namespace Hangfire.Dashboard.Configuration
{
    public class Settings : ISettings
    {
        public string HttpLogEndpoint { get; set; }
        public LogEventLevel MinimumLogLevel { get; set; }
        public IEnumerable<HangfireSqlServerSettings> HangfireSqlInstances { get; set; }
    }
}