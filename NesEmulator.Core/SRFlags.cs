namespace NesEmulator.Core
{
    public enum SRFlags
    {
        Negative = 0b10000000,
        VOverflow = 0b01000000,

        Break = 0b00010000,
        Decimal = 0b00001000,
        Interrupt = 0b00000100,
        Zero = 0b00000010,
        Carry = 0b00000001
    }
}
