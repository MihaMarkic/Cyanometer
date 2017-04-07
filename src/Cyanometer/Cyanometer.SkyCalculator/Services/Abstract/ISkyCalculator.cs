using Cyanometer.Core.Core;
using System.Collections.Generic;

namespace Cyanometer.SkyCalculator.Services.Abstract
{
    public interface ISkyCalculator
    {
        GetBluenessIndexResult GetBluenessIndexTopPixels(IList<Color> image, int pixelsCount);
    }
}
