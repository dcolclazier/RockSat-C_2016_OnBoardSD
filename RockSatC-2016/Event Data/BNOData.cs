using RockSatC_2016.Abstract;

namespace RockSatC_2016.Event_Data {
    
    public class BNOData : IEventData
    {
        public bool loggable => true;
        public short accel_x { get; set; }
        public short accel_y { get; set; }
        public short accel_z { get; set; }
        public short gyro_x { get; set; }
        public short gyro_y { get; set; }
        public short gyro_z { get; set; }
        public short temp { get; set; }

    }
}