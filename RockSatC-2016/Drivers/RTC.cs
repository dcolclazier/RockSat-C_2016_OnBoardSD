using Microsoft.SPOT.Hardware;

namespace RockSatC_2016.Work_Items
{
    internal class RTC
    {
        private readonly I2CDevice.Configuration _slaveConfig;
        private byte _h;
        private byte _m;
        private byte _s;

        private const int TransactionTimeout = 1000;
        private const byte ClockRateKHz = 59;

        public byte Address { get; private set; }

        public RTC(byte address = 0x68)
        {
            Address = address;
            _slaveConfig = new I2CDevice.Configuration(address,ClockRateKHz);
        }

     
        public byte[] CurrentTime()
        {
            var time = new byte[7];
            I2CBus.GetInstance().ReadRegister(_slaveConfig, 0x00, time ,TransactionTimeout);

            _s = (byte) (time[0] & 0x7F);
            _m = (byte) (time[1]);
            _h = (byte) (time[2]);

            return new[] { _h, _m, _s };

        }
 
    }
}