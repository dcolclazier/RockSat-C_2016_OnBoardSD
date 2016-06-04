using RockSatC_2016.Utility;

namespace RockSatC_2016.Flight_Computer {
    public class FlightComputer {
    
        private static FlightComputer _instance;
        public static FlightComputer Instance => _instance ?? (_instance = new FlightComputer());

        private FlightComputer() { }

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

