using System.Threading;
using RockSatC_2016.Abstract;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Utility
{
    public class WorkItem
    {
        public readonly ThreadStart Action = null;
        public readonly EventType EventType = EventType.None;
        public readonly IEventData EventData = null;
        public bool Loggable { get; private set; }

        public byte[] PacketData = null;

        private readonly bool _repeatable;
        public bool Persistent { get; set; }
        public bool Pauseable { get; set; }

        public WorkItem() { }

        public WorkItem(ThreadStart action, ref byte[] packetData, bool loggable, EventType type = EventType.None, bool persistent = false, bool pauseable = false)
        {
            Action = action;
            EventType = type;
            Loggable = loggable;
            //EventData = eventData;
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