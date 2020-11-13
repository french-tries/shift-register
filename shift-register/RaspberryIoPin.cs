using System;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace shiftregister
{
    public class RaspberryIoPin : IPin
    {
        public RaspberryIoPin(int id, bool activeHigh = true)
        {
            Id = id;
            ActiveHigh = activeHigh;
            pin = Pi.Gpio[id];
            pin.PinMode = GpioPinDriveMode.Output;
        }

        public int Id { get; }

        public bool ActiveHigh { get; }

        public void Write(bool value)
        {
            pin.Write(value == ActiveHigh);
        }

        private IGpioPin pin;
    }
}
