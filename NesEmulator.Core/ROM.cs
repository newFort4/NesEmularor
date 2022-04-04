using System;
using System.Linq;

namespace NesEmulator.Core
{
    public record ROM
    {
        public byte[] PRGROM { get; set; }
        public byte[] CHRROM { get; set; }
        public byte Mapper { get; set; }
        public Mirroring Mirroring { get; set; }

        private readonly byte[] NesTag = new byte[] { 0x4E, 0x45, 0x53, 0x1A };
        private const ushort PRGROMPageSize = 16384;
        private const ushort CHRROMPageSize = 8192;

        public ROM(byte[] raw)
        {
            if (!raw[0..4].SequenceEqual(NesTag))
            {
                throw new ArgumentException("File is not in iNES file format.");
            }

            var mapper = (byte)((raw[7] & 0b1111_0000) | (raw[6] >> 4));
            var iNesVersion = (raw[7] >> 2) & 0b11;

            if (iNesVersion != 0)
            {
                throw new ArgumentException("NES2.0 format is not supported");
            }

            var isFourScreen = (raw[6] & 0b1000) != 0;
            var isVerticalMirroring = (raw[6] & 0b1) != 0;

            var screenMirroring = isFourScreen ? Mirroring.FourScreen :
                isVerticalMirroring ? Mirroring.Vertical :
                Mirroring.Horizontal;

            var prgROMSize = raw[4] * PRGROMPageSize;
            var chrROMSize = raw[5] * CHRROMPageSize;

            var shouldSkipTrainer = (raw[6] & 0b100) != 0;

            var prgROMStart = 16 + (shouldSkipTrainer ? 512 : 0);
            var chrROMStart = prgROMStart + prgROMSize;

            PRGROM = raw[prgROMStart..(prgROMStart + prgROMSize)];
            CHRROM = raw[chrROMStart..(chrROMStart + chrROMSize)];
            Mapper = mapper;
            Mirroring = screenMirroring;
        }
    }
}
