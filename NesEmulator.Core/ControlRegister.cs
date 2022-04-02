namespace NesEmulator.Core
{
    public enum ControlRegisterEnum
    {
        Nametable1 = 0b00000001,
        Nametable2 = 0b00000010,
        VRAMAddIncrement = 0b00000100,
        SpritePatternAddress = 0b00001000,
        BackgroundPatternAddress = 0b00010000,
        SpriteSize = 0b00100000,
        MasterSlaveSelect = 0b01000000,
        GenerateNMI = 0b10000000
    }

    public class ControlRegister
    {
        public byte Bits { get; set; } = 0;

        public byte VRAMAddIncrement() => (byte)((Bits & (byte)ControlRegisterEnum.VRAMAddIncrement) == 0 ? 1 : 32);
        public void Update(byte data) => Bits = data;
    }
}
