using System;
using System.Diagnostics;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
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
        BnoDump = 0x02,
        TimeSync = 0x03
    }

    public class AccelUpdater  {


        private static readonly AnalogInput XPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
        private static readonly AnalogInput YPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);
        private static readonly AnalogInput ZPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount;
        private readonly int _offset;

        private const int MetaDataCount = 4;
        private const int TimeDataCount = 16;

        public AccelUpdater(int dataCount) {
            Debug.Print("Initializing Accelerometer data updater");
            _dataCount = dataCount;
            _dataArray = new byte[dataCount + MetaDataCount + TimeDataCount]; //3 bytes for each time stamp, 2 for size, 1 for type, 1 for start
            _workItem = new WorkItem(DumpAccelData, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            //start data packet w/ correct info
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.AccelDump;

            var dataSize = dataCount + TimeDataCount;
            _dataArray[2] = (byte)((dataSize >> 8) & 0xFF);
            _dataArray[3] = (byte)(dataSize & 0xFF);
            _offset = 4;
        }

        private void DumpAccelData()
        {
            var currentDataIndex = _offset;
            //Debug.Print("Accel start millis: " + Stopwatch.Instance.ElapsedMilliseconds);
            var time = BitConverter.GetBytes(Stopwatch.Instance.ElapsedMilliseconds);
            Debug.Print("Accel Time start: " + BitConverter.ToInt64(time, 0) + ":" + Debug.GC(false));
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];
            _dataArray[currentDataIndex++] = time[3];
            _dataArray[currentDataIndex++] = time[4];
            _dataArray[currentDataIndex++] = time[5];
            _dataArray[currentDataIndex++] = time[6];
            _dataArray[currentDataIndex++] = time[7];

            for (var i = 0; i < _dataCount/2; i++)
            {
                short raw = 0;
                switch (i % 3) {
                    
                    case 0:
                        raw = (short)(XPin.Read() * 1000);
                        //Debug.Print("0: " +  raw);
                        break;
                    case 1:
                        raw = (short)(YPin.Read() * 1000);
                        //Debug.Print("2: " + raw);
                        break;
                    case 2:
                        raw = (short)(ZPin.Read() * 1000);
                        ////Debug.Print("1: " + raw);
                        break;
                }
                var msb = (byte) ((raw >> 8) & 0xFF);
                var lsb = (byte) (raw & 0xff);

                _dataArray[currentDataIndex++] = msb;
                _dataArray[currentDataIndex++] = lsb;
            }
            //Debug.Print("Accel stop millis: " + Stopwatch.Instance.ElapsedMilliseconds);
            time = BitConverter.GetBytes(Stopwatch.Instance.ElapsedMilliseconds);
            //Debug.Print("Accel Time stop: " + BitConverter.ToInt64(time, 0));
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];
            _dataArray[currentDataIndex++] = time[3];
            _dataArray[currentDataIndex++] = time[4];
            _dataArray[currentDataIndex++] = time[5];
            _dataArray[currentDataIndex++] = time[6];
            _dataArray[currentDataIndex] = time[7];

            _workItem.PacketData = _dataArray;
            //Debug.Print("Accel.");
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
    
}