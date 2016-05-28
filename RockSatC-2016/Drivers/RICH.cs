using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Drivers
{
    public class RICH
    {
        public RICH()
        {
            Debug.Print("Initialing GoPro on pin D7...");
            _richPin = new OutputPort(Pins.GPIO_PIN_D7, false);
            _richPin.Write(false);
        }

        public void TurnOn()
        {
            Debug.Print("Turning ON GoPro!");
            _richPin.Write(true);
            Thread.Sleep(500);
            _richPin.Write(false);
            Debug.Print("GoPro is ON");

        }

        public void turnOff()
        {
            Debug.Print("Turning OFF GoPro");
            _richPin.Write(true);
            Thread.Sleep(4000);
            _richPin.Write(false);
            Debug.Print(("GoPro is OFF"));
        }

        private static OutputPort _richPin;
    }
}
