// based from https://github.com/dotnet/iot/blob/master/src/devices/ShiftRegister/ShiftRegister.cs
// which is licensed to the .NET Foundation under the MIT license.

// @todo reset?
using System;
namespace shiftregister
{
    /// <summary>
    /// Generic shift register implementation. Supports multiple register lengths.
    /// Compatible with SN74HC595, MBI5027 and MBI5168, for example.
    /// </summary>
    public class ShiftRegister
    {
        // Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5168.pdf
        // Tutorial: https://www.youtube.com/watch?v=6fVbJbNPrEU
        private readonly IPin _serial;
        private readonly IPin _clock;
        private readonly IPin _latch;
        private readonly IPin _outputEnable;

        /// <summary>
        /// Initialize a new shift register connected through GPIO.
        /// </summary>
        /// <param name="serialPin">The pin mapping to use by the binding.</param>
        /// <param name="clockPin">The pin mapping to use by the binding.</param>
        /// <param name="latchPin">The pin mapping to use by the binding.</param>
        /// <param name="outputEnablePin">The pin mapping to use by the binding. Active on low value. </param>
        /// <param name="bitLength">Bit length of register, including chained registers.</param>
        public ShiftRegister(IPin serialPin, IPin clockPin, IPin latchPin, IPin outputEnablePin, int bitLength)
        {
            _serial = serialPin;
            _clock = clockPin;
            _latch = latchPin;
            _outputEnable = outputEnablePin;
            BitLength = bitLength;

            // these three pins are required
            if (_serial == null || _clock == null || _latch == null)
            {
                throw new Exception($"{nameof(ShiftRegister)} -- values must be non-zero; Values: {nameof(_serial)}: {serialPin}; {nameof(_clock)}: {clockPin}; {nameof(_latch)}: {latchPin};.");
            }

            _serial.Write(false);
            _latch.Write(false);
            _clock.Write(false);

            // this pin assignment is optional
            // if not assigned, must be tied to ground
            _outputEnable?.Write(true);
        }

        /// <summary>
        /// Bit length across all connected registers.
        /// </summary>
        public int BitLength { get; }

        /// <summary>
        /// Shifts zeros.
        /// Will dim all connected LEDs, for example.
        /// Assumes register bit length evenly divisible by 8.
        /// </summary>
        public void ShiftClear()
        {
            if (BitLength % 8 > 0)
            {
                throw new ArgumentNullException(nameof(ShiftClear), "Only supported for registers with bit lengths evenly divisible by 8.");
            }

            for (int i = 0; i < BitLength / 8; i++)
            {
                ShiftByte(0b00000000);
            }
        }

        /// <summary>
        /// Writes bool value to storage register.
        /// This will shift existing values to the next storage slot.
        /// Does not latch.
        /// </summary>
        public void ShiftBit(bool value)
        {
            // writes value to serial data pin
            _serial.Write(value);
            // data is written to the storage register on the rising edge of the storage register clock
            _clock.Write(true);
            // values are reset to low
            _serial.Write(false);
            _clock.Write(false);
        }

        /// <summary>
        /// Shifts a byte -- 8 bits -- to the storage register.
        /// Assumes register bit length evenly divisible by 8.
        /// Pushes / overwrites any existing values.
        /// Latches by default.
        /// </summary>
        public void ShiftByte(byte value, bool latch = true)
        {
            for (int i = 0; i < 8; i++)
            {
                // 0b_1000_0000 (same as integer 128) used as input to create mask
                // determines value of i bit in byte value
                // logical equivalent of value[i] (which isn't supported for byte type in C#)
                // starts left-most and ends up right-most
                bool data = ((0b10000000 >> i) & value) != 0;
                // writes value to storage register
                ShiftBit(data);
            }

            if (latch)
            {
                Latch();
            }
        }

        /// <summary>
        /// Latches values in data register to output pi.
        /// </summary>
        public void Latch()
        {
            // latches value on rising edge of register clock (LE)
            _latch.Write(true);
            // value reset to low in preparation for next use.
            _latch.Write(false);
        }

        /// <summary>
        /// Switch output register to high or low-impedance state.
        /// Enables or disables register outputs, but does not delete values.
        /// </summary>
        public bool EnableOutput
        {
            set
            {
                _outputEnable?.Write(value);
            }
        }
    }
}                  
