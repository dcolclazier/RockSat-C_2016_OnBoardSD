﻿using Microsoft.SPOT;
using RockSatC_2016.Abstract;
using RockSatC_2016.Event_Data;
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

        public delegate void EventTriggered(EventType eventName, IEventData trigger, ref byte[] arrayData);

        public void TriggerEvent(EventType eventType, IEventData trigger, ref byte[] arrayData) {

            OnEventTriggered?.Invoke(eventType, trigger, ref arrayData);
        }
    }

  
}

