using System.Threading;
using RockSatC_2016.Flight_Computer;

namespace RockSatC_2016.Work_Items
{
    public class WorkItem
    {
        public readonly ThreadStart Action;

        public bool Loggable { get; private set; }
        public byte[] PacketData;

        private readonly bool _repeatable;
        private readonly object _locker = new object();
        public bool Persistent { get; set; }
        public bool Pauseable { get; set; }

        public WorkItem() { }

        public WorkItem(ThreadStart action, ref byte[] packetData, bool loggable, bool persistent = false, bool pauseable = false)
        {
            Action = action;
            Loggable = loggable;
            PacketData = packetData;
            _repeatable = persistent;
            Persistent = persistent;
            Pauseable = pauseable;

            if (Pauseable) MemoryMonitor.Instance.RegisterPauseableAction(this);
        }

        public void Start()
        {
            lock (_locker)
            {
                if (_repeatable) Persistent = true;
                FlightComputer.Instance.Execute(this);
            }
        }
    

        public void Stop()
        {
            lock(_locker)
                Persistent = false;
        }

    }
}