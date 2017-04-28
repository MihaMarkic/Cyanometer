using System;
using System.Diagnostics;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.InterIntegratedCircuit;

namespace Righthand.WittyPi
{
    /// <summary>
    /// A class representing the Witty Pi board.
    /// </summary>
    public class WittyPiService : IWittyPiService
    {
        private readonly DateTime ZeroDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// Hour format
        /// </summary>
        public const byte HOUR_12 = 0x01 << 6;
        /// <summary>
        /// DS1337 Address
        /// </summary>
        public const byte SsdI2cAddress = 0x68;
        /// <summary>
        /// SDA pin
        /// </summary>
        public readonly ConnectorPin SdaPin;
        /// <summary>
        /// SCL pin
        /// </summary>
        public readonly ConnectorPin SclPin;

        /// <summary>
        /// Initializes an instace with default Raspberry I2C pins (P1Pin03, P1Pin05)
        /// </summary>
        public WittyPiService(): this(ConnectorPin.P1Pin03, ConnectorPin.P1Pin05)
        {}

        /// <summary>
        /// Initializes an instance given I2C pins. 
        /// </summary>
        /// <param name="sdaPin"></param>
        /// <param name="sclPin"></param>
        public WittyPiService(ConnectorPin sdaPin, ConnectorPin sclPin)
        {
            this.SdaPin = sdaPin;
            this.SclPin = sclPin;
        }

        /// <summary>
        /// Gets or sets wake up date.
        /// </summary>
        public WakeUpDateTime WakeUp
        {
            get
            {
                using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
                {
                    var conn = i2c.Connect(SsdI2cAddress);
                    return ReadWakeUpDate(conn);
                }
            }
            set
            {
                using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
                {
                    var conn = i2c.Connect(SsdI2cAddress);
                    WriteWakeUp(conn, value);
                }
            }
        }
        /// <summary>
        /// Gets or sets sleep date.
        /// </summary>
        public SleepDateTime Sleep
        {
            get
            {
                using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
                {
                    var conn = i2c.Connect(SsdI2cAddress);
                    return ReadSleepDate(conn);
                }
            }
            set
            {
                using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
                {
                    var conn = i2c.Connect(SsdI2cAddress);
                    WriteSleep(conn, value);
                }
            }
        }
        /// <summary>
        /// Writes wake up date to board.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="date"></param>
        public static void WriteWakeUp(I2cDeviceConnection conn, WakeUpDateTime date)
        {
            conn.Write(0x07, GetBcd(date.Sec), GetBcd(date.Min), GetBcd(date.Hour), GetBcd(date.Day));
        }
        /// <summary>
        /// Writes sleep date to board.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="date"></param>
        public static void WriteSleep(I2cDeviceConnection conn, SleepDateTime date)
        {
            conn.Write(0x0B, GetBcd(date.Min), GetBcd(date.Hour), GetBcd(date.Day));
        }
        /// <summary>
        /// Reads wake up date from board.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static WakeUpDateTime ReadWakeUpDate(I2cDeviceConnection conn)
        {
            conn.WriteByte(0x07);
            var bytes = conn.Read(4);
            var piDate = new WakeUpDateTime
            (
                GetByte(bytes[3]),
                GetByte(bytes[2]),
                GetByte(bytes[1]),
                GetByte(bytes[0]).Value
            );
            return piDate;
        }
        /// <summary>
        /// Reads sleep date from board.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SleepDateTime ReadSleepDate(I2cDeviceConnection conn)
        {
            conn.WriteByte(0x0B);
            var bytes = conn.Read(3);
            var piDate = new SleepDateTime
            (
                min: GetByte(bytes[0]).Value,
                hour: GetByte(bytes[1]).Value,
                day: GetByte(bytes[2]).Value
            );
            return piDate;
        }
        /// <summary>
        /// Transforms byte to BCD.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetBcd(int? value)
        {
            return GetBcd((byte?)value);
        }
        /// <summary>
        /// Transforms byte to BCD.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte GetBcd(byte? value)
        {
            if (value.HasValue)
            {
                return (byte)(value / 10 * 16 + value % 10);
            }
            else
            {
                return 128;
            }
        }
        /// <summary>
        /// Transforms BDC to byte.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte? GetByte(byte value)
        {
            if (value == 128)
            {
                return null;
            }
            else
            {
                return (byte)(10 * (value >> 4) + (value & 0xf));
            }
        }

        /// <summary>
        /// RTC datetime on DS1337.
        /// </summary>
        public DateTime RtcDateTime
        {
            get
            {
                return ReadRtcDateTime();
            }
            set
            {
                WriteRtcDateTime(value);
            }
        }

        private void WriteRtcDateTime(DateTime value)
        {
            using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
            {
                var conn = i2c.Connect(SsdI2cAddress);
                DateTime utc = value.ToUniversalTime();
                conn.Write(0x00, GetBcd(utc.Second), GetBcd(utc.Minute), GetBcd(utc.Hour), 1, GetBcd(utc.Day),
                    GetBcd(utc.Month),  // month
                    GetBcd(utc.Year % 100)
                );
            }
        }

        private DateTime ReadRtcDateTime()
        {
            using (var i2c = new I2cDriver(SdaPin.ToProcessor(), SclPin.ToProcessor()))
            {
                var conn = i2c.Connect(SsdI2cAddress);
                conn.WriteByte(0x00);
                var buffer = conn.Read(7);
                int hour;
                if ((buffer[2] & HOUR_12) == HOUR_12)
                {
                    hour = ((buffer[2] >> 4) & 0x01) * 12 + ((buffer[2] >> 5) & 0x01) * 12;
                }
                else
                {
                    hour = GetByte(buffer[2]).Value;
                }
                return new DateTime(
                    2000 + GetByte(buffer[6]).Value,
                    GetByte((byte)(buffer[5] & 0x1f)).Value,
                    GetByte(buffer[4]).Value,
                    hour,
                    GetByte(buffer[1]).Value,
                    GetByte(buffer[0]).Value,
                    DateTimeKind.Utc
                );
            }
        }
    }
}
