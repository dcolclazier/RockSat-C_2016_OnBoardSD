using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Abstract;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Work_Items
{

    enum PacketType : byte
    {
        StartByte = 0xFF,
        Geiger = 0x00,
        AccelDump = 0x01,
        BNODump = 0x02
    }

    public class AccelUpdater  {
        private static readonly AnalogInput XPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
        private static readonly AnalogInput YPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);
        private static readonly AnalogInput ZPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

        private readonly AccelData _accelData = new AccelData();
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _arraySize;
        //private int _frequency;
        private readonly int _offset;

        public AccelUpdater(int arraySize) {
            Debug.Print("Initializing Accelerometer data updater");
            _arraySize = arraySize;
            _dataArray = new byte[_arraySize + 10]; //3 bytes for each time stamp, 2 for size, 1 for type, 1 for start
            _workItem = new WorkItem(DumpAccelData, ref _dataArray, EventType.AccelDump, _accelData, persistent:true, pauseable:true);
            //_frequency = frequency;

            //start data packet w/ correct info
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.AccelDump;
            _dataArray[2] = (byte)(_arraySize & 0xFF);
            _dataArray[3] = (byte)((_arraySize >> 8) & 0xFF);
            _offset = 4;
        }

        private void DumpAccelData()
        {
            short x = 0;
            

            _dataArray[0 + _offset] = RTC.Hours();
            _dataArray[1 + _offset] = RTC.Minutes();
            _dataArray[2 + _offset] = RTC.Seconds();

            for (var i = 0; i < _arraySize; i+=2)
            {
                short raw = 0;
                switch (x++%3) {
                    case 0: raw = (short)(XPin.Read() * 1000);
                        break;
                    case 2: raw = (short)(YPin.Read() * 1000);
                        break;
                    case 1: raw = (short)(ZPin.Read() * 1000);
                        break;
                }

                _dataArray[i + _offset + 3] = (byte) (raw & 0xFF);
                _dataArray[i + +_offset + 4] = (byte) ((raw >> 8) & 0xFF);
                //var period = 1000*(1/_frequency);
                //if (period < 1) period = 1;
                //Thread.Sleep(period);
            }

            var time = RTC.CurrentTime();
            _dataArray[_arraySize + _offset + 3] = time[0]; //hours
            _dataArray[_arraySize + _offset + 4] = time[1]; //minutes
            _dataArray[_arraySize + _offset + 5] = time[2]; //seconds

            Debug.Print("Accel data dump complete - free mem: " + Debug.GC(true));
        }

        public void Start() {
            _workItem.Start();
        }
        
    }

    // ReSharper disable once InconsistentNaming
    internal class RTC
    {
        public static byte[] CurrentTime()
        {
            return new[]
            {
                Hours(),
                Minutes(),
                Seconds()
            };
        }

        public static byte Hours()
        {
            return 0x00;
        }
        public static byte Minutes()
        {
            return 0x00;
        }
        public static byte Seconds()
        {
            return 0x00;
        }
    }
}