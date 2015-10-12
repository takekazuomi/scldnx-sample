using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SCLDotNetCore
{
    public class ListTable : ICommand
    {
        private readonly CloudTableClient _tableClient;

        public ListTable(CloudTableClient tableClient)
        {
            _tableClient = tableClient;
            Console.WriteLine("constractor ListTable");
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            TableContinuationToken token = null;
            do
            {
                var result = await _tableClient.ListTablesSegmentedAsync(null, null, token, null, null, cancellationToken);

                result.Results.ToList().ForEach(table => Console.WriteLine($"{table.Name}"));

                token = result.ContinuationToken;
            } while (token != null);
        }
    }
}
