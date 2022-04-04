using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NesEmulator.Core
{
    // ToDo: Refactor
    public class Bus
    {
        public byte[] CPUVRAM { get; } = Enumerable.Repeat((byte)0, 2048).ToArray();
        public readonly ROM ROM;
        public readonly PPU PPU;
        public readonly Joypad Joypad;

        private readonly Action<PPU, object> callback;

        public int Cycles { get; private set; } = 0;

        private const ushort MirrorsLength = 0x1FFF;

        private const ushort RAM = 0x0000;
        private const ushort RAMMirrorsEnd = RAM + MirrorsLength;
        private const ushort PPURegisters = 0x2000;
        private const ushort PPURegistersMirrorsEnd = PPURegisters + MirrorsLength;

        public Bus(ROM rom, Action<PPU, object> callback, Joypad joypad)
        {
            ROM = rom;
            PPU = new(rom.CHRROM, rom.Mirroring);
            Joypad = joypad;

            this.callback = callback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(byte cycles)
        {
            Cycles += cycles;

            var nmiBefore = PPU.NMIInterrupt.HasValue;
            var a = PPU.Tick((byte)(3 * cycles));
            var nmiAfter = PPU.NMIInterrupt.HasValue;

            if (!nmiBefore && nmiAfter)
            {
                Task.Run(() => callback(PPU, null));
                //callback(PPU, null); // ToDo: joypad
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                case 0x2002:
                    return PPU.ReadStatus();
                case 0x2004:
                    return PPU.ReadOAMData();
                case 0x2007:
                    return PPU.ReadMemory();
                case >= 0x2008 and <= PPURegistersMirrorsEnd:
                    // ToDo: Implement PPU addresses
                    mirrorDownAddress = (ushort)(address & 0b00100000_00000111);
                    return ReadMemory(mirrorDownAddress);
                case 0x4016:
                    return Joypad.Read();
                case >= 0x8000 and <= 0xFFFF:
                    return ReadPRGROM(address);
                default:
                    return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? PollNMIStatus()
        {
            var result = PPU.NMIInterrupt;

            PPU.NMIInterrupt = null;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemory(ushort address, byte data)
        {
            switch (address)
            {
                case >= RAM and <= RAMMirrorsEnd:
                    var mirrorDownAddress = (ushort)(address & 0b00000111_11111111);
                    CPUVRAM[mirrorDownAddress] = data;

                    break;
                case 0x2000:
                    PPU.WriteToControl(data);
                    break;
                case 0x2001:
                    PPU.WriteToMask(data);
                    break;
                case 0x2002:
                    throw new InvalidOperationException("Attempt to write to PPU status register.");
                case 0x2003:
                    PPU.WriteToOAMAddress(data);
                    break;
                case 0x2004:
                    PPU.WriteToOAM(data);
                    break;
                case 0x2005:
                    PPU.WriteToScroll(data);
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
                case 0x4014:
                    var buffer = new byte[256];
                    var high = data << 8;
                    for (var i = 0; i < 256; i++)
                    {
                        buffer[i] = ReadMemory((ushort)(high + i));
                    }

                    PPU.WriteOAMDMA(buffer);
                    break;
                case 0x4016:
                    Joypad.Write(data);
                    break;
                case >= 0x8000 and <= 0xFFFF:
                    throw new InvalidOperationException("Attempt to write to Cartridge ROM space.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadMemoryUshort(ushort position)
        {
            var low = (ushort)ReadMemory(position);
            var high = (ushort)ReadMemory((ushort)(position + 1));

            return (ushort)((high << 8) | (low));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteMemoryUshort(ushort position, ushort data)
        {
            var high = (byte)(data >> 8);
            var low = (byte)(data & 0xFF);

            WriteMemory(position, low);
            WriteMemory((ushort)(position + 1), high);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
