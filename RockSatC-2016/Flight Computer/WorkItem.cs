using System.Threading;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Flight_Computer
{
    public class WorkItem
    {
        public readonly ThreadStart Action = null;

        public bool Loggable { get; private set; }
        public byte[] PacketData;

        private readonly bool _repeatable;
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
            if (_repeatable) Persistent = true;
            FlightComputer.Instance.Execute(this);
        }

        public void Stop()
        {
            Persistent = false;
        }

    }
}