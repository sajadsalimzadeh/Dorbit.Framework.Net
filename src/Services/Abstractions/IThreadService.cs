using System;
using System.Threading;

namespace Devor.Framework.Services.Abstractions
{
    public interface IThreadService
    {
        Thread MainThread { get; set; }
    }
}
