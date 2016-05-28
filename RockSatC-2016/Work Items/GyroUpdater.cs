using DemoSat16.Event_Data;
using DemoSat16.Sensor_Libs;
using DemoSat16.Utility;

namespace DemoSat16.Work_Items {
    //this is an example of using the flight computer to execute a persistent work item that
    //also needs to trigger an event upon completion. This will update the gyroscope data!
    public class GyroUpdater {

        //this is the gyroData that gets updated when we run this work item. It's also  the data
        //that gets sent when the gyro event triggers! Since we don't trigger the event until after
        // the work item finishes running, the gyroData will contain the latest update from the gyroscope.
        private readonly GyroData _gyroData;

        //Our work item - in this case, it will be UpdateGyro() below.
        private readonly WorkItem _workItem;

        //A reference to the sensor that has the gyroscope on it - need it to get updates!
        private readonly Bno055 _sensor;

        public GyroUpdater(Bno055 sensor) {
            //assign our sensor and create our data. This only happens once.
            _sensor = sensor;
            _gyroData = new GyroData();

            //create our work item!
            _workItem = new WorkItem(UpdateGyro, true, FlightEventType.Gyro, _gyroData);
        }

        //running Start() will start the GyroUpdater by starting the 
        //    persistent work item we created in the constructor.
        public void Start() {
            //we want the gyro updater to repeat itself, so setRepeat to true, just in case it 
            //was turned off before now, disabling the repeat.
            _workItem.SetRepeat(true);

            //execute the work item.
            FlightComputer.Instance.Execute(_workItem);
        }

        public void Stop() {
            _workItem.SetRepeat(false); // this will mark the action as non-persistent, 
                                        //   stopping it after the next run
        }

        //This is the workItem code! Notice how simple it is - it just updates the gyroData based on the
        // latest vector we got from the sensor.
        public void UpdateGyro() {
            var gyroVector = _sensor.GetVector(Bno055.Bno055VectorType.Vector_Gyroscope);
            _gyroData.X = gyroVector.X; 
            _gyroData.Y = gyroVector.Y; 
            _gyroData.Z = gyroVector.Z;  
        }
    }
}