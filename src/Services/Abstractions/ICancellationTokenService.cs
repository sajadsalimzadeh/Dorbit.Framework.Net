using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Devor.Framework.Services.Abstractions
{
    public interface ICancellationTokenService
    {
        CancellationToken CancellationToken { get; set; }
    }
}
