using System;
using System.Diagnostics;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Listeners;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;
using RockSatC_2016.Work_Items;


namespace RockSatC_2016 {


    public static class Program {
       
        public static void Main() {
            //Configure local time


            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            var logger = new Logger();

            //Initializes the RICH on pin D7
            Debug.Print("Initializing RICH detector");
            var rich = new Rich();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            //Debug.Print("Initializing BNO Sensor on Serial Port COM4, 1 stop bit, 0 parity, 8 data bits");
            //var bnoloop = new SerialBnoUpdater(sigFigs: 4);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(sleepInterval:50);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            var accel_dump_size = 12288;
            Debug.Print("Initializing fast accel dump collector with a size of " + accel_dump_size + "bytes.");
            var acceldumploop = new AccelUpdater(accel_dump_size);

            Debug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Debug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);
            

            //THIS STARTS THE LOGGER
            Debug.Print("Starting logger...");
            logger.Start();

            Debug.Print("Starting stopwatch");
            Stopwatch.Instance.Start();

            //Debug.Print("Recording time-sync packet");
            //var timeSync = new TimeSync();
            //timeSync.RunOnce();


            //THIS STARTS THE Accel dump update
            Debug.Print("Starting accel dumper...");
            acceldumploop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            //Debug.Print("Starting bno sensor updates...");
            //bnoloop.Start();

            //THIS STARTS THE Geiger UPDATE.
            Debug.Print("Starting geiger counter data collection...");
            geigerloop.Start();

            //Starts the RICH detector
            Debug.Print("Starting RICH detector");
            rich.TurnOn();

            Debug.Print("Flight computer boot successful.");
        }

    }

    public class TimeSync
    {
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _offset;

        public TimeSync()
        {
            _dataArray = new byte[18]; // start, type, size, size, hours, minutes, seconds, 8 bytes for millis()
            _dataArray[0] = (byte) PacketType.StartByte;
            _dataArray[1] = (byte) PacketType.TimeSync;
            _dataArray[2] = (byte) 0; //msb for '8' as short- static size 
            _dataArray[3] = (byte) 8; //lsb for '8' as short- static size
            _offset = 4;

            _workItem = new WorkItem(SyncTime, ref _dataArray, loggable:true);
        }

        private void SyncTime()
        {
            var dataIndex = 0;
            var time = RTC.CurrentTime();
            var millis = BitConverter.GetBytes(Stopwatch.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++ + _offset] = time[0];
            _dataArray[dataIndex++ + _offset] = time[1];
            _dataArray[dataIndex++ + _offset] = time[2];

            _dataArray[dataIndex++ + _offset] = millis[0];
            _dataArray[dataIndex++ + _offset] = millis[1];
            _dataArray[dataIndex++ + _offset] = millis[2];
            _dataArray[dataIndex++ + _offset] = millis[3];
            _dataArray[dataIndex++ + _offset] = millis[4];
            _dataArray[dataIndex++ + _offset] = millis[5];
            _dataArray[dataIndex++ + _offset] = millis[6];
            _dataArray[dataIndex + _offset] = millis[7];

            time = RTC.CurrentTime();
            _dataArray[dataIndex++ + _offset] = time[0];
            _dataArray[dataIndex++ + _offset] = time[1];
            _dataArray[dataIndex + _offset] = time[2];

            _workItem.PacketData = _dataArray;
        }

        public void RunOnce()
        {
            FlightComputer.Instance.Execute(_workItem);
        }
    }
}

