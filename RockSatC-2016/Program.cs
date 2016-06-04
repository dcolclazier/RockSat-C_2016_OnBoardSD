using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Listeners;
using RockSatC_2016.Work_Items;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace RockSatC_2016 {


    public static class Program {
       
        public static void Main() {
            //Configure local time


            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            var logger = new Logger();

            //Initializes the RICH on pin D7
            var rich = new RICH();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            //Debug.Print("Initializing BNO Sensor on Serial Port COM4, 1 stop bit, 0 parity, 8 data bits");
            var bnoloop = new SerialBnoUpdater(sigFigs: 4);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(sleepInterval:50);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing fast accel dump collector with a size of 12kb");
            var acceldumploop = new AccelUpdater(arraySize:12000);

            Debug.Print("INIT Complete. Continuing with boot.");

            //THIS SECTION INITIALIZES AND STARTS THE MEMORY MONITOR
            Debug.Print("Starting memory monitor...");
            MemoryMonitor.Instance.Start(ref logger);
            

            //THIS STARTS THE LOGGER
            Debug.Print("Starting logger...");
            logger.Start();

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
            rich.TurnOn();
            Debug.Print("RICH detector start signal sent.");

            Debug.Print("Flight computer boot successful.");
        }

    }


}

