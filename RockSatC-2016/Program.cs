using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Listeners;
using RockSatC_2016.Work_Items;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace RockSatC_2016 {


    //to decode...

    //start a loop, reading each byte...
    //if the first byte is not 0xFF, there's a corruption problem with the file
    //the first byte is the start byte
    //the second byte is the type byte
    //the third and fourth bytes hold the size of the data in the packet (add 10 for total size)
    //next 3 bytes include start time stamp, 1 for hour, 1 for minutes, 1 for seconds
    //now comes data
    //final 3 bytes include end time stamp, 1 for hour, 1 for minutes, 1 for seconds


    
    public static class Program {
       
        public static void Main() {
            //Configure local time


            //THIS SECTION CREATES / INITIALIZES THE SERIAL LOGGER
            Debug.Print("Flight computer started successfully. Beginning INIT.");

            Debug.Print("Initializing Serial logger on COM1 with baudrate of 115200bps.  Max log buffer = 4096b");
            var logger = new Logger(SerialPorts.COM1, 115200);

            //Initializes the RICH on pin D7
            var rich = new RICH();

            //THIS SECTION CREATES/INITIALIZES THE SERIAL BNO 100HZ UPDATER
            //Debug.Print("Initializing BNO Sensor on Serial Port COM4, 1 stop bit, 0 parity, 8 data bits");
            //var bnoloop = new SerialBnoUpdater(sigFigs: 4);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing geiger counter collection data");
            var geigerloop = new GeigerUpdater(sleepInterval:30);

            //THIS SECTION CREATES/INITIALIZES THE GEIGER COUNTER UPDATER
            Debug.Print("Initializing fast accel dump collector with a size of 12kb");
            var acceldumploop = new AccelUpdater(12000);

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

        //public static void custom_delay_usec(uint microseconds) {
        //    var delayStart = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks;
        //    while (Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks - delayStart < microseconds*10) ;

        //}
    }


}

