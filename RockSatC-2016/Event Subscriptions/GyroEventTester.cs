using DemoSat16.Event_Data;
using DemoSat16.Utility;
using Microsoft.SPOT;

namespace DemoSat16.Event_Subscriptions {
    //This is an example of a class that needs to respond to a Gyro event. It subscribes to the event 
    //system in the constructor, and then only has a single method that pronts the gyro data to the debug log,
    //every time the gyro updates. This assumes that the tester can run as fast as the gyro is updating.
    public class GyroEventTester {
        
        //running start will add TestGyro to the list of actions that need to be run when
        //an event triggers.
        public void Start() {
            FlightComputer.OnEventTriggered += TestGyro;
        }
        //running stop will remove TestGyro from the list of actions that need to be run 
        //when an event triggers.
        public void Stop() {
            FlightComputer.OnEventTriggered -= TestGyro;
        }
        //this is the code that runs when all events trigger!!!
        private void TestGyro(FlightEventType eventType, IEventData eventData) {

            //since this runs when ALL events trigger, we want to stop running it if the 
            //event was NOT a gyro event.
            if (eventType != FlightEventType.Gyro) return;

            //if we got here, the event was a Gyro event, and we can trust that the eventData
            // will be GyroData rather than some other kind.
            var data = eventData as GyroData;

            //if for some reason we didn't get any data, something went wrong - just get out of 
            //here so we don't crash. 
            if (data == null) return;

            //the data was gyro data, and it isn't null! Print it to the debug window.
            Debug.Print("Gyro X: " + data.X + "  Y: " + data.Y + " Z:" + data.Z);
        }
    }
}