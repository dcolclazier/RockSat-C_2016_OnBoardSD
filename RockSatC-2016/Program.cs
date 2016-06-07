using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Work_Items;


namespace RockSatC_2016 {
    
    
    //debug packets instead of usb debug
    
    public static class Program {
       
        public static void Main() {

            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            var logger = new Logger();
            Debug.Print("Starting logger...");
            logger.Start();


            Rebug.Print("Starting stopwatch");
            Clock.Instance.Start();

            Rebug.Print("Recording time-sync packet");
            var timeSync = new TimeSync(delay:10000);
            timeSync.Run();

            //Initializes the RICH on pin D7
            Rebug.Print("Initializing RICH detector");
            var rich = new Rich();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            Rebug.Print("Initializing BNO Sensor ");
            var bnoloop = new SerialBnoUpdater(delay:1000);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Rebug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(delay:10, size:2048);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            var accel_dump_size = 18432;
            Rebug.Print("Initializing fast accel dump collector with a size of " + accel_dump_size + "bytes.");
            var acceldumploop = new AccelUpdater(accel_dump_size);

            //Thread.Sleep(5000);
            Rebug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Rebug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);

            //THIS STARTS THE Accel dump update
            Rebug.Print("Starting accel dumper...");
            acceldumploop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            Rebug.Print("Starting bno sensor updates...");
            bnoloop.Start();

            //THIS STARTS THE Geiger UPDATE.
            Rebug.Print("Starting geiger counter data collection...");
            geigerloop.Start();

            //Starts the RICH detector
            Rebug.Print("Starting RICH detector");
            rich.TurnOn();

            Rebug.Print("Flight computer boot successful.");
        }

    }
}

