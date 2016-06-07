using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Flight_Computer;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Work_Items {
    public class GeigerUpdater  {

        static readonly InterruptPort ShieldedGeiger = new InterruptPort(Pins.GPIO_PIN_D2, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
        static readonly InterruptPort UnshieldedGeiger = new InterruptPort(Pins.GPIO_PIN_D3, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);

        private readonly WorkItem _workItem;

        private int ShieldedCounts { get; set; }
        private int UnshieldedCounts { get; set; }

        private readonly int _delay;
        public byte[] _dataArray;

        private int _dataCount;
        private int _metadataCount = 2; 
        private int _timedataCount = 6; // 1 x 8 bytes
        private long _timeSinceLastRun;

        public GeigerUpdater(int delay, int size)
        {
            _dataCount = size;

            _delay = delay;


            Debug.Print("Adding interrupt action for shielded geiger counter.");
            ShieldedGeiger.OnInterrupt += Shielded_Counter;

            Debug.Print("Adding interrupt action for unshielded geiger counter.");
            UnshieldedGeiger.OnInterrupt += Unshielded_Counter;

            Debug.Print("Creating Threadpool action, repeats every 5 seconds.");

            _dataArray = new byte[_dataCount + _metadataCount + _timedataCount];
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.Geiger;

            _workItem = new WorkItem(GatherCounts, ref _dataArray, loggable:true, pauseable:true, persistent:true);

            
        }

        private void GatherCounts() {

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            var currentDataIndex = _metadataCount;

            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];
      


            //if we only need 1 byte per update (at 20+ hz)
            //_dataArray[currentDataIndex++] = (byte)ShieldedCounts;
            //_dataArray[currentDataIndex] = (byte)UnshieldedCounts;

            for (int i = 0; i < _dataCount; i++)
            {
                if (i%2 == 0)
                {
                    _dataArray[currentDataIndex++] = (byte) ShieldedCounts;
                    ShieldedCounts = 0;
                }
                else
                {
                    _dataArray[currentDataIndex++] = (byte)UnshieldedCounts;
                    UnshieldedCounts = 0;
                }
            }
            //_dataArray[currentDataIndex++] = (byte)UnshieldedCounts;

            time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex] = time[2];
            //Thread.Sleep(_delay);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
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