#pragma warning disable CA1416 // Validate platform compatibility
using System.Diagnostics;

namespace Devor.Framework.Models
{
    public class NetworkInfo
    {
        internal PerformanceCounter BandwidthCounter { get; }
        internal PerformanceCounter DataSentCounter { get; }
        internal PerformanceCounter DataReceivedCounter { get; }

        public NetworkInfo(string networkCard)
        {
            BandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", networkCard);
            DataSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", networkCard);
            DataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkCard);
        }

        public float BandWidth { get; internal set; }
        public float SendByte { get; internal set; }
        public float ReceiveByte { get; internal set; }
    }
}

