using Microsoft.SPOT;
using RockSatC_2016.Abstract;

namespace RockSatC_2016.Event_Data {
    internal class GeigerData : IEventData {
        public bool loggable => true;
        public int shielded_geigerCount { get; set; }
        public int unshielded_geigerCount { get; set; }
    }
}