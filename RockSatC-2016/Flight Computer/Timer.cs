using System;

namespace RockSatC_2016.Flight_Computer
{
    internal class Timer
    {
        private long _mStartTicks;
        private static Timer _instance;
        private static readonly object locker = new object();
        private const long MTicksPerMillisecond = TimeSpan.TicksPerMillisecond;

        public static Timer Instance {
            get{
                lock(locker) return _instance ?? (_instance = new Timer());
            }
        }

        private Timer() { }
        
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