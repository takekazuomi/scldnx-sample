using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCLDotNetCore
{
    internal interface ICommand
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
