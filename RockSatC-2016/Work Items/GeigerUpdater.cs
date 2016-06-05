using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
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
        public byte[] _dataArray;
        private readonly int _offset;

        private int _metadataCount = 4; 
        private int _timedataCount = 8; // 1 x 8 bytes

        public GeigerUpdater(int dataCount = 2, int sleepInterval = 1000)
        {
            _sleepTime = sleepInterval;
            //_geigerData = new GeigerData();
            Debug.Print("Adding interrupt action for shielded geiger counter.");
            ShieldedGeiger.OnInterrupt += Shielded_Counter;

            Debug.Print("Adding interrupt action for unshielded geiger counter.");
            UnshieldedGeiger.OnInterrupt += Unshielded_Counter;

            Debug.Print("Creating Threadpool action, repeats every 5 seconds.");


            _dataArray = new byte[dataCount + _metadataCount + _timedataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.Geiger;

            var dataSize = dataCount + _timedataCount;
            _dataArray[2] = (byte)((dataSize >> 8) & 0xFF);
            _dataArray[3] = (byte)(dataSize & 0xFF);
            _offset = 4;

            _workItem = new WorkItem(GatherCounts, ref _dataArray, loggable:true, pauseable:true, persistent:true);
        }

        private void GatherCounts() {
            
            var currentDataIndex = _offset;

            var time = BitConverter.GetBytes(Stopwatch.Instance.ElapsedMilliseconds);
            
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];
            _dataArray[currentDataIndex++] = time[3];
            _dataArray[currentDataIndex++] = time[4];
            _dataArray[currentDataIndex++] = time[5];
            _dataArray[currentDataIndex++] = time[6];
            _dataArray[currentDataIndex++] = time[7];


            //if we only need 1 byte per update (at 20+ hz)
            _dataArray[currentDataIndex++] = (byte)ShieldedCounts;
            _dataArray[currentDataIndex] = (byte)UnshieldedCounts;

            //if we need 2 bytes per update (at 20+hz)
            //_dataArray[currentDataIndex++] = (byte)((ShieldedCounts >> 8) & 0xFF);
            //_dataArray[currentDataIndex++] = (byte)(ShieldedCounts & 0xFF);

            //_dataArray[currentDataIndex++] = (byte)((UnshieldedCounts >> 8) & 0xFF);
            //_dataArray[currentDataIndex] = (byte)(UnshieldedCounts & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

            ShieldedCounts = 0;
            UnshieldedCounts = 0;

            Thread.Sleep(_sleepTime);
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