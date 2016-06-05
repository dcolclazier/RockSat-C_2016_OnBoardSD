using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Listeners;
using RockSatC_2016.Utility;
using RockSatC_2016.Work_Items;


namespace RockSatC_2016 {
    //accel time is off - fixed
    //geiger counter isn't logging shielded data? - fixed
    //timewatcher data skewed
    //need event trigger for launch
    //need listener for launch trigger - 
    //debug?
    //fix data file creation

    public static class Program {
       
        public static void Main() {
            //Configure local time


            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            var logger = new Logger();
            Debug.Print("Starting logger...");
            logger.Start();

            Debug.Print("Starting stopwatch");
            Stopwatch.Instance.Start();

            Debug.Print("Recording time-sync packet");
            var timeSync = new TimeSync();
            timeSync.RunOnce();

            //RTC.Adjust(0,59,0,4,6,2016);

            //Initializes the RICH on pin D7
            Debug.Print("Initializing RICH detector");
            var rich = new Rich();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            //Debug.Print("Initializing BNO Sensor on Serial Port COM4, 1 stop bit, 0 parity, 8 data bits");
            //var bnoloop = new SerialBnoUpdater(sigFigs: 4);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(sleepInterval:40);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            var accel_dump_size = 18432;
            Debug.Print("Initializing fast accel dump collector with a size of " + accel_dump_size + "bytes.");
            var acceldumploop = new AccelUpdater(accel_dump_size);

            Thread.Sleep(5000);
            Debug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Debug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);

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
}

