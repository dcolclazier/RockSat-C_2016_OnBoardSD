using System;
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
        TimeSync = 0x03,
        DebugMessage = 0x04
    }

    public class AccelUpdater  {


        private static readonly AnalogInput XPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);
        private static readonly AnalogInput YPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A1);
        private static readonly AnalogInput ZPin = new AnalogInput(AnalogChannels.ANALOG_PIN_A2);

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount;
        //private readonly int _offset;
        private readonly double _zLaunchThreshold;

        private const int MetaDataCount = 2;
        private const int TimeDataCount = 6;

        public AccelUpdater(int dataCount, float zLaunchThreshold = 2.5f)
        {
            _zLaunchThreshold = zLaunchThreshold;
            Rebug.Print("Initializing Accelerometer data updater");
            _dataCount = dataCount;
            _dataArray = new byte[dataCount + MetaDataCount + TimeDataCount]; //3 bytes for each time stamp, 2 for size, 1 for type, 1 for start
            _workItem = new WorkItem(DumpAccelData, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            //start data packet w/ correct info
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.AccelDump;


        }

        //This is the method that gets run by the threadpool persistently... notice its name is 
        //listed above as a parameter...
        private void DumpAccelData()
        {
            //keeps track of the current index in the packet... notice how it increments
            //    as we continue adding data to it...
            var currentDataIndex = MetaDataCount;

            //get our stopwatch's elapsed ms, stick it in our packet
            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex++] = time[2];
          
            //anything beyond simply filling RAM with data is S-L-O-W on the .NetMF due to the 
            // overhead associated with it. 
            //
            //The switch statement decides whether we're on x, y, or z... our first index is
            //0, and 0 % 3 is 0, so it makes sense to store x here...
            //       1 % 3 is 1, so it makes sense to store y here... and so on.
            for (var i = 0; i < _dataCount/2; i++)
            {
                short raw = 0;
                switch (i % 3) {
                    case 0:
                        raw = (short)(XPin.Read() * 1000);
                        break;
                    case 1:
                        raw = (short)(YPin.Read() * 1000);
                        break;
                    case 2:
                        raw = (short)(ZPin.Read() * 1000);
                        break;
                }
                var msb = (byte) ((raw >> 8) & 0xFF);
                var lsb = (byte) (raw & 0xff);                                    
                                                                                  
                _dataArray[currentDataIndex++] = msb;
                _dataArray[currentDataIndex++] = lsb;
            }

            time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            //last 8 bytes store end time stamp
            _dataArray[currentDataIndex++] = time[0];
            _dataArray[currentDataIndex++] = time[1];
            _dataArray[currentDataIndex] = time[2];
          
            //pass this off to the packet so it gets recorded to the sd card.
            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);
        }

        //called in program.cs to start up the accelerometer sensor logger
        public void Start() {
            _workItem.Start();
        }
        
    }
    
}