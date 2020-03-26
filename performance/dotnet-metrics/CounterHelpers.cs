using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;

namespace dotnet_metrics
{
    public class CounterHelpers
    {
        public static EventPipeProvider MakeProvider(string name, int refreshIntervalInSec)
        {
            var filterData = BuildFilterData(refreshIntervalInSec);
            return new EventPipeProvider(name, EventLevel.Verbose, 0xFFFFFFFF, filterData);
        }

        private static Dictionary<string, string> BuildFilterData(int refreshIntervalInSec)
        {
            if (refreshIntervalInSec < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(refreshIntervalInSec), "refreshIntervalInSec must be at least 1 second.");
            }

            return new Dictionary<string, string>
                   {
                       { "EventCounterIntervalSec", refreshIntervalInSec.ToString() }
                   };
        }
    }
}
