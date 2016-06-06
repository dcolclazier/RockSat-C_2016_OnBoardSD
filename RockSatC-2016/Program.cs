using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016 {
    
    
    //debug packets instead of usb debug
    
    public static class Program {
       
        public static void Main() {

            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            var logger = new Logger();
            logger.Start();
            FlightComputer.Logger = logger;

            Debug.Print("Flight computer started successfully. Beginning INIT.");

            Debug.Print("Starting logger...");

            Debug.Print("Logger started, Rebugger initialized");

            Debug.Print("Starting stopwatch");
            Clock.Instance.Start();

            Debug.Print("Recording time-sync packet");
            var timeSync = new TimeSync(delay:30000);
            timeSync.Run();

            //Initializes the RICH on pin D7
            Debug.Print("Initializing RICH detector");
            var rich = new Rich();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            Debug.Print("Initializing BNO Sensor ");
            var bnoloop = new SerialBnoUpdater();

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(sleepInterval:40);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            var accelDumpSize = 18432;
            Debug.Print("Initializing fast accel dump collector with a size of " + accelDumpSize + "bytes.");
            var acceldumploop = new AccelUpdater(accelDumpSize);

            //Thread.Sleep(5000);
            Debug.Print("Flight computer INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Debug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);


            //THIS STARTS THE Accel dump update
            Debug.Print("Starting accel dumper...");
            acceldumploop.Start();

            //THIS STARTS THE BNO SENSOR UPDATE
            Debug.Print("Starting bno sensor updates...");
            bnoloop.Start();

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

