using Microsoft.SPOT.Hardware;

namespace RockSatC_2016.Work_Items
{
    // ReSharper disable once InconsistentNaming
    internal static class RTC
    {
        private static readonly I2CDevice.Configuration SlaveConfig;
        private static byte _h;
        private static byte _m;
        private static byte _s;
        private static readonly object locker = new object();
        private const int TransactionTimeout = 1000;
        private const byte ClockRateKHz = 59;

        public static byte Address => 0x68;

        static RTC()
        {
            SlaveConfig = new I2CDevice.Configuration(Address,ClockRateKHz);
        }

        public static void Adjust(byte newHours, byte newMin, byte newSec, byte newDay, byte newMonth, int newYear)
        {
            var buffer = new byte[7];
            buffer[0] = newHours;
            buffer[1] = newMin;
            buffer[2] = newSec;
            buffer[3] = 0x00;
            buffer[4] = newDay;
            buffer[5] = newMonth;
            buffer[6] = (byte)(newYear - 2000);
            I2CBus.GetInstance().WriteRegister(SlaveConfig, 0x00, buffer, TransactionTimeout);
        }

        public static byte[] CurrentTime()
        {
            
            var time = new byte[7];
            I2CBus.GetInstance().ReadRegister(SlaveConfig, 0x00, time ,TransactionTimeout);

            _s = (byte) (time[0] & 0x7F);
            _m = (byte) (time[1]);
            _h = (byte) (time[2]);

            return new[] { _h, _m, _s };

        }
 
    }
}