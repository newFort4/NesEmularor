namespace NesEmulator.Core
{
    public static class OpCodes
    {
        public const byte LDA = 0xA9;

        public const byte TAX = 0xAA;
        public const byte TAY = 0xA8;

        public const byte INX = 0xE8;
        public const byte INY = 0xC8;

        public const byte SED = 0xF8;
        public const byte SEC = 0x38;
        public const byte SEI = 0x78;

        public const byte CLD = 0xD8;
        public const byte CLV = 0xB8;
        public const byte CLC = 0x18;
        public const byte CLI = 0x58;
    }
}
