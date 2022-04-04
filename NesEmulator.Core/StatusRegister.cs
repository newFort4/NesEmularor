namespace NesEmulator.Core
{
    enum StatusRegisterEnum
    {
        NotUsed = 0b00000001,
        NotUsed2 = 0b00000010,
        NotUsed3 = 0b00000100,
        NotUsed4 = 0b00001000,
        NotUsed5 = 0b00010000,
        SpriteOverflow = 0b00100000,
        SpriteZeroHit = 0b01000000,
        VBlankStarted = 0b10000000
    }

    // ToDo: Refactor
    public class StatusRegister
	{
        public byte Value { get; set; } = 0;

        public void SetVBlankStatus(bool status) => Value = status ? (byte)(Value | ((byte)StatusRegisterEnum.VBlankStarted)) : (byte)(Value & ~(byte)StatusRegisterEnum.VBlankStarted);

        public void SetSpriteZeroHit(bool status) => Value = status ? (byte)(Value | ((byte)StatusRegisterEnum.SpriteZeroHit)) : (byte)(Value & ~(byte)StatusRegisterEnum.SpriteZeroHit);

        public void SetSpriteOverflow(bool status) => Value = status ? (byte)(Value | ((byte)StatusRegisterEnum.SpriteOverflow)) : (byte)(Value & ~(byte)StatusRegisterEnum.SpriteOverflow);

        public void ResetVBlankStatus() => SetVBlankStatus(false);
        public bool IsInVBlank() => (Value & ((byte)StatusRegisterEnum.VBlankStarted)) != 0;
        public byte Snaphot => Value;
    }
}

