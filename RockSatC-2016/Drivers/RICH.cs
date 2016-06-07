using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using RockSatC_2016.Work_Items;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Drivers
{
    public class Rich
    {

        public Rich()
        {
            _richPin = new OutputPort(Pins.GPIO_PIN_D6, false);
            _richPin.Write(false);
        }

        public void TurnOn()
        {
            Rebug.Print("Sending signal to power on RICH detector");
            _richPin.Write(true);
            Thread.Sleep(500);
            _richPin.Write(false);

        }

        public void TurnOff()
        {
            Rebug.Print("Sending signal to power off RICH detector");
            _richPin.Write(true);
            Thread.Sleep(4000);
            _richPin.Write(false);
        }

        private static OutputPort _richPin;
    }
}
