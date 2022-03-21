using System;

namespace NesEmulator.Core
{
    public class Bus
    {
        public byte[] CPUVRAM { get; } = new byte[2048];

        private const ushort MirrorsLength = 0x1FFF;

        private const ushort RAM = 0x0000;
        private const ushort RAMMirrorsEnd = RAM + MirrorsLength;
        private const ushort PPURegisters = 0x2000;
        private const ushort PPURegistersMirrorsEnd = PPURegisters + MirrorsLength;

        public byte ReadMemory(ushort address)
        {
            switch (address)
            {
                case >= RAM and <= RAMMirrorsEnd:
                    var mirrorDownAddress = (ushort)(address & 0b00000111_11111111);

                    // ToDo: Guy from the internet casts it to usize
                    return CPUVRAM[mirrorDownAddress];
                case >= PPURegisters and <= PPURegistersMirrorsEnd:
                    // ToDo: Implement PPU addresses
                    mirrorDownAddress = (ushort)(address & 0b00100000_00000111);
                    throw new NotImplementedException("PPU addresses are not implemented yet.");
                default:
                    return 0;
            }
        }

        public void WriteMemory(ushort address, byte data)
        {
            switch (address)
            {
                case >= RAM and <= RAMMirrorsEnd:
                    var mirrorDownAddress = (ushort)(address & 0b00000111_11111111);
                    // ToDo: Guy from the internet casts it to usize
                    CPUVRAM[mirrorDownAddress] = data;

                    break;
                case >= PPURegisters and <= PPURegistersMirrorsEnd:
                    // ToDo: Implement PPU addresses
                    mirrorDownAddress = (ushort)(address & 0b00100000_00000111);
                    throw new NotImplementedException("PPU addresses are not implemented yet.");
                default:
                    break;
            }
        }

        public ushort ReadMemoryUshort(ushort position)
        {
            var low = (ushort)ReadMemory(position);
            var high = (ushort)ReadMemory((ushort)(position + 1));

            return (ushort)((high << 8) | (low));
        }

        public void WriteMemoryUshort(ushort position, ushort data)
        {
            var high = (byte)(data >> 8);
            var low = (byte)(data & 0xFF);

            WriteMemory(position, low);
            WriteMemory((ushort)(position + 1), high);
        }
    }
}
