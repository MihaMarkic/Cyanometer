using Cyanometer.Core.Core;
using System.Collections.Generic;

namespace Cyanometer.SkyCalculator.Services.Abstract
{
    public interface IColorCalculator
    {
        Color CalculateNearest(Color source);
        void Init(IList<Color> colors);
    }
}
