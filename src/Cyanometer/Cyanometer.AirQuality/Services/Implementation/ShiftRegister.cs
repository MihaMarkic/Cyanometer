using Cyanometer.AirQuality.Services.Abstract;
using Raspberry.IO.GeneralPurpose;
using System;

namespace Cyanometer.AirQuality.Services.Implementation
{
    public class ShiftRegister: IShiftRegister
    {
        private static readonly Lights[] AllLights = new Lights[] { Lights.One, Lights.Two, Lights.Three, Lights.Four, Lights.Five, Lights.Six, Lights.Seven, Lights.Eight };
        public void EnableLight(Lights light)
        {
            TimeSpan blinkDuration = TimeSpan.FromMilliseconds(10);
            var pinPush = ConnectorPin.P1Pin36.Output();
            var pinLatch = ConnectorPin.P1Pin38.Output();
            var pinValue = ConnectorPin.P1Pin40.Output();

            using (var conn = new GpioConnection(pinPush, pinLatch, pinValue))
            {
                foreach (Lights l in AllLights)
                {
                    conn[pinValue] = (light & l) == l;
                    conn.Blink(pinPush, blinkDuration);
                }
                conn[pinValue] = false;
                // push the rest of non used lights 
                //for (int j = 0; j < (8 - AllLights.Length); j++)
                //{
                //    conn.Blink(pinPush, blinkDuration);
                //}
                // commit
                conn.Blink(pinLatch, blinkDuration);
            }
        }
    }
}
