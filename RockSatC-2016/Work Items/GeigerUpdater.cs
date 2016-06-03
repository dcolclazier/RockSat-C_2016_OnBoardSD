using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Abstract;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Work_Items {
    public class GeigerUpdater  {

        static readonly InterruptPort ShieldedGeiger = new InterruptPort(Pins.GPIO_PIN_D2, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
        static readonly InterruptPort UnshieldedGeiger = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);

        //private readonly GeigerData _geigerData;
        private readonly WorkItem _workItem;

        private int ShieldedCounts { get; set; }
        private int UnshieldedCounts { get; set; }

        private readonly int _sleepTime;
        private readonly byte[] _newData;
        private readonly int _offset;


        public GeigerUpdater(int dataCount = 4, int sleepInterval = 1000, int metaDataCount = 10)
        {
            _sleepTime = sleepInterval;
            //_geigerData = new GeigerData();
            Debug.Print("Adding interrupt action for shielded geiger counter.");
            ShieldedGeiger.OnInterrupt += Shielded_Counter;

            Debug.Print("Adding interrupt action for unshielded geiger counter.");
            UnshieldedGeiger.OnInterrupt += Unshielded_Counter;

            Debug.Print("Creating Threadpool action, repeats every 5 seconds.");


            _newData = new byte[dataCount + metaDataCount];
            _newData[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _newData[1] = (byte)PacketType.Geiger;
            _newData[2] = (byte)((dataCount >> 8) & 0xFF);
            _newData[3] = (byte)(dataCount & 0xFF);
            _offset = 4;

            _workItem = new WorkItem(GatherCounts, ref _newData, loggable:true, pauseable:true, persistent:true);
        }

        private void GatherCounts() {
            Thread.Sleep(_sleepTime);
            Debug.Print("Gathering Geiger counts data, resetting. " + Debug.GC(true));

            var dataIndex = 0;

            //var time = RTC.CurrentTime();
            var time = new byte[] {1, 2, 3};
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex++ + _offset] = time[2];

            //_geigerData.shielded_geigerCount = ShieldedCounts;
            _newData[dataIndex++ + _offset] = (byte)((ShieldedCounts >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(ShieldedCounts & 0xFF);

            //_geigerData.unshielded_geigerCount = UnshieldedCounts;
            _newData[dataIndex++ + _offset] = (byte)((UnshieldedCounts >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(UnshieldedCounts & 0xFF);

            //time = RTC.CurrentTime();
            //time = new byte[] { 0, 0, 0 };
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex + _offset] = time[2];

            ShieldedCounts = 0;
            UnshieldedCounts = 0;

        }

        private void Shielded_Counter(uint data1, uint data2, DateTime time) {
            ShieldedCounts++;
        }
        private void Unshielded_Counter(uint data1, uint data2, DateTime time){
            UnshieldedCounts++;
        }

        public void Start() {
            _workItem.Start();
        }
    }
}