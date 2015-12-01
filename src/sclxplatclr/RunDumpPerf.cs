using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SclXplatDnx
{
    public partial class Program
    {
        private static DateTime _baseDate = DateTime.Parse("2015-10-12T03:00:00");
        private DateTime _fromDate = _baseDate.AddHours(-2);
        private DateTime _toDate = _baseDate.AddHours(1);

        /// <summary>
        /// Single Thread Loop
        /// </summary>
        private async Task RunDumpPerf(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            var min = _fromDate.ToUniversalTime().Ticks;
            var max = _toDate.ToUniversalTime().Ticks;

            var table = _tableClient.GetTableReference("WADPerformanceCountersTable");

            var query = new TableQuery<WADPerformanceCountersTable>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, min.ToString("d19")),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, max.ToString("d19"))));

            var result = new List<WADPerformanceCountersTable>();
            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token, null, null, cancellationToken);
                token = segment.ContinuationToken;
                result.AddRange(segment.Results);
               // Console.Write($"{segment.Results.Count}.");

            } while (token != null);
            Console.WriteLine();

            sw.Stop();
            Console.WriteLine("Count:{0}, Min: {1}, Max: {2}, Elapsed: {3:F2} sec",
                result.Count, result.Min(e => e.PartitionKey), result.Max(e => e.PartitionKey), (sw.ElapsedMilliseconds/1000.0));
        }
    }
}
