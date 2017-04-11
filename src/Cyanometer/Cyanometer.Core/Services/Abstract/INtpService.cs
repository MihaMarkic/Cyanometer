using System;

namespace Cyanometer.Core.Services.Abstract
{
    public interface INtpService
    {
        DateTime GetNetworkTime();
    }
}