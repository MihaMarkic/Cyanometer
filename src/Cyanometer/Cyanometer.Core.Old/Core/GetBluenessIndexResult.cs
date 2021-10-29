using System.Diagnostics;

namespace Cyanometer.Core.Core
{
    [DebuggerDisplay("{Index}")]
    public struct GetBluenessIndexResult
    {
        public int Index { get; set; }
        public double Percentage { get; set; }
        public int[] Indexes { get; set; }
    }
}
