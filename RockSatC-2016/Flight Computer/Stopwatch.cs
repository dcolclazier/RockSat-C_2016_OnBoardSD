using Microsoft.SPOT.Hardware;

namespace System.Diagnostics
{
    internal class Stopwatch
    {
        private long _mStartTicks;
        private static Stopwatch _instance;
        private static readonly object locker = new object();
        private const long MTicksPerMillisecond = TimeSpan.TicksPerMillisecond;

        public static Stopwatch Instance
        {
            get
            {
                lock(locker)
                    return _instance ?? (_instance = new Stopwatch());
            }
        }

        private Stopwatch() { }
        
        public void Start() {
            lock(locker)
                _mStartTicks = Utility.GetMachineTime().Ticks; 
        }
        
        public long ElapsedMilliseconds
        {
            get {
                lock(locker)
                    return (Utility.GetMachineTime().Ticks - _mStartTicks)/MTicksPerMillisecond;
            }
        }
    }
}