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

        private const int TransactionTimeout = 1000;
        private const byte ClockRateKHz = 59;

        public static byte Address => 0x68;

        static RTC()
        {
            SlaveConfig = new I2CDevice.Configuration(Address,ClockRateKHz);
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

namespace System.Diagnostics
{
    internal class Stopwatch
    {
        private long _mStartTicks;
        private static Stopwatch _instance;

        private const long MTicksPerMillisecond = TimeSpan.TicksPerMillisecond;

        public static Stopwatch Instance => _instance ?? (_instance = new Stopwatch());

        private Stopwatch() { }
        
        public void Start() {
            _mStartTicks = Utility.GetMachineTime().Ticks; 
        }
        
        public long ElapsedMilliseconds => (Utility.GetMachineTime().Ticks - _mStartTicks) / MTicksPerMillisecond;
    }
}