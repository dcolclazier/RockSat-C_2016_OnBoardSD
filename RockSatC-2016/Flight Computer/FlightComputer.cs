using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Flight_Computer {
    public class FlightComputer {
    
        private static FlightComputer _instance;
        public static FlightComputer Instance => _instance ?? (_instance = new FlightComputer());
        public static bool Launched { get; set; }
        public static Logger Logger { get; set; }

        private FlightComputer()
        {
            Launched = false;
        }

        public void Execute(WorkItem workItem) {
            ThreadPool.QueueWorkItem(workItem);
        }
        public static event EventTriggered OnEventTriggered;

        public delegate void EventTriggered(bool loggable, ref byte[] arrayData);

        public void TriggerEvent(bool loggable, ref byte[] arrayData) {

            OnEventTriggered?.Invoke(loggable, ref arrayData);
        }
    }

  
}

