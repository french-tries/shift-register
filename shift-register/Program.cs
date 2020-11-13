using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace shiftregister
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Pi.Init<BootstrapWiringPi>();

            IPin serialPin = new RaspberryIoPin(23);
            IPin clockPin = new RaspberryIoPin(8);
            IPin latchPin = new RaspberryIoPin(25);
            IPin outputEnablePin = new RaspberryIoPin(24, false);

            var register = new ShiftRegister(serialPin, clockPin, latchPin, outputEnablePin, 16);

            int i = 0;
            while (true)
            {
                register.ShiftByte((byte)i++);
                Thread.Sleep(1000);
            }
        }
    }
}
