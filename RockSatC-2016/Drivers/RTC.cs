using Microsoft.SPOT.Hardware;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;

namespace RockSatC_2016.Drivers
{
    // ReSharper disable once InconsistentNaming
    internal static class RTC
    {
        private static readonly I2CDevice.Configuration SlaveConfig;
        private const int TransactionTimeout = 1000;
        private const byte ClockRateKHz = 59;

        public static byte Address => 0x68;
        public static byte StatusReg => 0x0f;

        static RTC()
        {
            SlaveConfig = new I2CDevice.Configuration(Address,ClockRateKHz);
        }

        public static void Adjust(byte newHour, byte newMin, byte newSec, byte newDay, byte newMonth, int newYear)
        {
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x00,newSec,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x01,Tools.Bin2Bcd(newMin)[0],TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x02,newHour,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x03, 1,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x04,newDay,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x05,newMonth,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x06,(byte)(newYear-2000),TransactionTimeout);
        }

        public static byte[] CurrentTime()
        {

            var time = new byte[7];
            I2CBus.GetInstance().ReadRegister(SlaveConfig, 0x00, time, TransactionTimeout);

            var realseconds = Tools.Bcd2Bin(new [] { time[0] });
            var minutes = Tools.Bcd2Bin(new [] { time[1] });
            var hours = Tools.Bcd2Bin(new [] { time[2] });

            //Debug.Print("Current time: " + hours + ":" + minutes + ":" + realseconds);
            return new[]
            {
                (byte)hours,
                (byte)minutes,
                (byte)realseconds
            };

        }
 
    }
}