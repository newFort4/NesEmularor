using System;
using System.Linq;

namespace NesEmulator.Core
{
    public class Bus
    {
        public byte[] CPUVRAM { get; } = Enumerable.Repeat((byte)0, 2048).ToArray();
        public readonly ROM ROM;
        public readonly PPU PPU;

        private const ushort MirrorsLength = 0x1FFF;

        private const ushort RAM = 0x0000;
        private const ushort RAMMirrorsEnd = RAM + MirrorsLength;
        private const ushort PPURegisters = 0x2000;
        private const ushort PPURegistersMirrorsEnd = PPURegisters + MirrorsLength;

        public Bus(ROM rom)
        {
            ROM = rom;
            PPU = new(rom.CHRROM, rom.Mirroring);
        }

        public byte ReadMemory(ushort address)
        {
            switch (address)
            {
                case >= RAM and <= RAMMirrorsEnd:
                    var mirrorDownAddress = (ushort)(address & 0b00000111_11111111);

                    // ToDo: Guy from the internet casts it to usize
                    return CPUVRAM[mirrorDownAddress];
                case 0x2000:
                case 0x2001:
                case 0x2003:
                case 0x2005:
                case 0x2006:
                case 0x4014:
                    throw new InvalidOperationException($"Attempt to read from write-only PPU address {address:x}");
                case 0x2007:
                    return PPU.ReadMemory();
                case >= 0x2008 and <= PPURegistersMirrorsEnd:
                    // ToDo: Implement PPU addresses
                    mirrorDownAddress = (ushort)(address & 0b00100000_00000111);
                    return ReadMemory(mirrorDownAddress);
                case >= 0x8000 and <= 0xFFFF:
                    return ReadPRGROM(address);
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
                case 0x2000:
                    PPU.WriteToControl(data);
                    break;
                case 0x2006:
                    PPU.WriteToAddress(data);
                    break;
                case 0x2007:
                    PPU.WriteData(data);
                    break;
                case >= 0x2008 and <= PPURegistersMirrorsEnd:
                    mirrorDownAddress = (ushort)(address & 0b00100000_00000111);
                    WriteMemory(mirrorDownAddress, data);
                    break;
                case >= 0x8000 and <= 0xFFFF:
                    throw new InvalidOperationException("Attempt to write to Cartridge ROM space.");
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

        private byte ReadPRGROM(ushort address)
        {
            address -= 0x8000;

            if (ROM.PRGROM.Length == 0x4000 && address >= 0x4000)
            {
                address %= 0x4000;
            }

            return ROM.PRGROM[address];
        }
    }
}
