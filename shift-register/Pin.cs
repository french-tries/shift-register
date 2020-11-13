using System;
namespace shiftregister
{
    // <todo> dummy pin
    public interface IPin
    {
        int Id { get; }
        bool ActiveHigh { get; }

        void Write(bool value);
    }
}
