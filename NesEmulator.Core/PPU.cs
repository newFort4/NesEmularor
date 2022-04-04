using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NesEmulator.Core
{
    // ToDo: Refactor
    public class PPU
	{
        public byte[] ChrROM { get; set; }
        public byte[] PalletTable { get; set; }
        public byte[] VRAM { get; set; }
        public byte[] OAMData { get; set; }

        public byte OAMAddress = 0;

        public int Cycles = 0;
        public ushort Scanline = 0;
        public int? NMIInterrupt = null;

        public AddressRegister Address { get; set; } = new();
        public ControlRegister Control { get; set; } = new();
        public MaskRegister Mask { get; set; } = new();
        public StatusRegister Status { get; set; } = new();
        public ScrollRegister Scroll { get; set; } = new();

        public Mirroring Mirroring { get; set; }

        private byte internalDataBuffer = 0;

        public PPU(byte[] chrROM, Mirroring mirroring)
        {
            ChrROM = chrROM;
            Mirroring = mirroring;

            VRAM = Enumerable.Repeat((byte)0, 2048).ToArray();
            OAMData = Enumerable.Repeat((byte)0, 64 * 4).ToArray();
            PalletTable = Enumerable.Repeat((byte)0, 32).ToArray();
        }

        internal bool Tick(byte cycles)
        {
            Cycles += cycles;
            if (Cycles >= 341)
            {
                Cycles -= 341;
                Scanline++;

                if (Scanline == 241)
                {
                    Status.SetVBlankStatus(true);
                    Status.SetSpriteZeroHit(false);

                    if (Control.GeneraeVBlankNMI())
                    {
                        NMIInterrupt = 1;
                    }
                }

                if (Scanline >= 262)
                {
                    Scanline = 0;
                    NMIInterrupt = null;
                    Status.SetSpriteZeroHit(false);
                    Status.ResetVBlankStatus();
                    return true;
                }
            }
            return false;
        }

        public byte ReadStatus()
        {
            var data = Status.Snaphot;
            Status.ResetVBlankStatus();
            Address.ResetLatch();
            Scroll.ResetLatch();

            return data;
        }

        public byte ReadOAMData() => OAMData[OAMAddress];

        public void WriteToAddress(byte value) => Address.Update(value);

        public void WriteToControl(byte value)
        {
            var beforeNMIInterrupt = Control.GeneraeVBlankNMI();
            Control.Update(value);
            if (!beforeNMIInterrupt && Control.GeneraeVBlankNMI() && Status.IsInVBlank())
            {
                NMIInterrupt = 1;
            }    
        }

        public void WriteToMask(byte value) => Mask.Update(value);
        public void WriteToOAMAddress(byte value) => OAMAddress = value;
        public void WriteToOAM(byte value)
        {
            OAMData[OAMAddress] = value;
            OAMAddress++;
        }
        public void WriteToScroll(byte value) => Scroll.Write(value);
        public void WriteOAMDMA(byte[] bytes)
        {
            foreach (var @byte in bytes)
            {
                OAMData[OAMAddress] = @byte;
                OAMAddress++;
            }
        }

        public void IncrementVRAMAddress() => Address.Increment(Control.VRAMAddIncrement());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadMemory()
        {
            var address = Address.Get();
            IncrementVRAMAddress();

            switch (address)
            {
                case >= 0 and <= 0x1FFF:
                    var result = internalDataBuffer;

                    internalDataBuffer = ChrROM[address];

                    return result;
                case >= 0x2000 and <= 0x2FFF:
                    result = internalDataBuffer;

                    internalDataBuffer = VRAM[MirrorVRAMAddress(address)];

                    return result;
                case >= 0x3000 and <= 0x3EFF:
                    throw new InvalidOperationException($"Address space 0x3000..0x3eff is not expected to be used, requested = { address }");
                case 0x3F10:
                case 0x3F14:
                case 0x3F18:
                case 0x3F1C:
                    address -= 10;
                    return PalletTable[address - 0x3F00];
                case >= 0x3F00 and <= 0x3FFF:
                    return PalletTable[address - 0x3F00];
                default:
                    throw new InvalidOperationException($"Unexpected access to mirrored space { address }");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteData(byte data)
        {
            var address = Address.Get();

            switch (address)
            {
                case >= 0x0000 and <= 0x1FFF:
                    break;
                    throw new InvalidOperationException($"Attempt to write to chr rom space { address }");
                case >= 0x2000 and <= 0x2FFF:
                    VRAM[MirrorVRAMAddress(address)] = data;
                    break;
                case >= 0x3000 and <= 0x3EFF:
                    throw new InvalidOperationException($"Address space 0x3000..0x3eff is not expected to be used, requested = { address }");
                case 0x3F10:
                case 0x3F14:
                case 0x3F18:
                case 0x3F1C:
                    address -= 10;
                    PalletTable[address - 0x3F00] = data;
                    break;
                case >= 0x3F00 and <= 0x3FFF:
                    PalletTable[address - 0x3F00] = data;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected access to mirrored space { address }");
            }

            IncrementVRAMAddress();
        }

        public ushort MirrorVRAMAddress(ushort address)
        {
            var mirroredVRAM = address & 0b10111111111111;
            var vramIndex = mirroredVRAM - 0x2000;
            var nameTable = vramIndex / 0x400;

            switch ((Mirroring, nameTable))
            {
                case (Mirroring.Vertical, 2):
                case (Mirroring.Vertical, 3):
                case (Mirroring.Horizontal, 3):
                    return (ushort)(vramIndex - 0x800);
                case (Mirroring.Horizontal, 1):
                case (Mirroring.Horizontal, 2):
                    return (ushort)(vramIndex - 0x400);
                default:
                    return (ushort)vramIndex;
            }
        }
    }

    public record AddressRegister
    {
        public (byte, byte) Value { get; set; }
        public bool HighPointer { get; set; }

        public AddressRegister()
        {
            Value = (0, 0);
            HighPointer = true;
        }

        public void Set(ushort data)
        {
            var high = (byte)(data >> 8);
            var low = (byte)(data & 0xFF);

            Value = (high, low);
        }

        public void Update(byte data)
        {
            Value = HighPointer ? (data, Value.Item2) : (Value.Item1, data);

            if (Get() > 0x3FFF)
            {
                Set((ushort)(Get() & 0b11111111111111));
            }

            HighPointer = !HighPointer;
        }

        public void ResetLatch() => HighPointer = true;

        public ushort Get() => (ushort)((Value.Item1 << 8) | Value.Item2);

        internal void Increment(byte increment)
        {
            var low = Value.Item2;

            Value = (Value.Item1, (byte)(Value.Item2 + increment));

            if (low > Value.Item2)
            {
                Value = ((byte)(Value.Item1 + 1), Value.Item2);
            }

            if (Get() > 0x3FFF)
            {
                Set((ushort)(Get() & 0b11111111111111));
            }
        }
    }
}

