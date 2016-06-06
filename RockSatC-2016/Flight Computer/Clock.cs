using System;

namespace RockSatC_2016.Flight_Computer
{
    internal class Clock
    {
        private long _mStartTicks;
        private static Clock _instance;
        private static readonly object locker = new object();
        private const long MTicksPerMillisecond = TimeSpan.TicksPerMillisecond;

        public static Clock Instance {
            get{
                lock(locker) return _instance ?? (_instance = new Clock());
            }
        }

        private Clock() { }
        
        public void Start() {
            lock(locker)
                _mStartTicks = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks; 
        }
        
        public long ElapsedMilliseconds {
            get {
                lock(locker)
                    return (Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - _mStartTicks)/MTicksPerMillisecond;
            }
        }
    }
}