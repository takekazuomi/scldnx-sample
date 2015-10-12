using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Framework.DependencyInjection;


namespace SCLDotNetCore
{
    // https://github.com/dotnet/coreclr

    public partial class Program
    {
        private readonly CloudTableClient _tableClient;
        private IServiceCollection _services;
        private IServiceProvider _provider;

        public static CloudStorageAccount GetStorageAccount()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            return CloudStorageAccount.Parse(connectionString);
        }

        public Program()
        {
            var storageAccount = GetStorageAccount();

            _tableClient = storageAccount.CreateCloudTableClient();

            _tableClient.DefaultRequestOptions = new TableRequestOptions
            {
                PayloadFormat = TablePayloadFormat.JsonNoMetadata,
                ServerTimeout = new TimeSpan(0, 100, 0),
                MaximumExecutionTime = new TimeSpan(4, 0, 0),
                RetryPolicy = new LinearRetry(new TimeSpan(0, 0, 1), 60)
            };

            // https://github.com/dotnet/corefx/issues/1784
            var commands = GetType().GetTypeInfo().Assembly.DefinedTypes.Where(info => info.ImplementedInterfaces.Any(type => type == typeof (ICommand)));
            
            _services = new ServiceCollection();

            foreach (var typeInfo in commands)
                _services.AddTransient(typeof (ICommand), typeInfo.AsType());

            _provider = _services.BuildServiceProvider();

        }

        public CloudTable GetTableReference(string tableName)
        {
            return _tableClient.GetTableReference(tableName);
        }

        public void Run(string commandName)
        {
            var cancelToken = new CancellationToken();

            var typeName = GetType().GetTypeInfo().Assembly.DefinedTypes.FirstOrDefault(info => info.Name == commandName)?.FullName;

            if (typeName == null) throw new ArgumentException($"Command {commandName} not found", nameof(commandName));

            var command = ActivatorUtilities.CreateInstance(_provider, Type.GetType(typeName), _tableClient) as ICommand;

            command?.RunAsync(cancelToken).Wait(cancelToken);
        }


        private static void Main(string[] args)
        {
            var program = new Program();

            if (args.Length > 0)
            {
                foreach (var s in args)
                {
                    program.Run(s);
                }
            }
        }
    }
}
