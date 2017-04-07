using System;

namespace Cyanometer.Core.Services.Abstract
{
    public interface IDaylightManager
    {
        bool IsDay();
        void Load();
    }
}
