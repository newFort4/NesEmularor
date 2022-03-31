using System;
using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests
{
    public class ROMTests
	{
		private const ushort PRGROMPageSize = 16384;
		private const ushort CHRROMPageSize = 8192;

		private record TestROM(byte[] Header, byte[] Trainer, byte[] PRGROM, byte[] CHRROM);

		private byte[] CreateROM(TestROM testROM) => testROM
				.Header
				.Concat(testROM.Trainer ?? Array.Empty<byte>())
				.Concat(testROM.PRGROM)
				.Concat(testROM.CHRROM)
				.ToArray();

		[Fact]
		public void ROMLoadsSuccessfully()
        {
			var prgROM = Enumerable.Repeat((byte)1, 2 * PRGROMPageSize).ToArray();
			var chrROM = Enumerable.Repeat((byte)2, 1 * CHRROMPageSize).ToArray();

			var testROM = CreateROM(new TestROM(
				new byte[] { 0x4E, 0x45, 0x53, 0x1A, 0x02, 0x01, 0x31, 00, 00, 00, 00, 00, 00, 00, 00, 00 },
				null,
                prgROM,
				chrROM
			));

            var rom = new ROM(testROM);

			Assert.Equal(rom.CHRROM, chrROM);
			Assert.Equal(rom.PRGROM, prgROM);
			Assert.Equal(3, rom.Mapper);
			Assert.Equal(Mirroring.Vertical, rom.Mirroring);
		}

		[Fact]
		public void ROMLoadsSuccessfullyWithTrainer()
        {
			var prgROM = Enumerable.Repeat((byte)1, 2 * PRGROMPageSize).ToArray();
			var chrROM = Enumerable.Repeat((byte)2, 1 * CHRROMPageSize).ToArray();

			var testROM = CreateROM(new TestROM(
				new byte[] {
					0x4E,
					0x45,
					0x53,
					0x1A,
					0x02,
					0x01,
					0x31 | 0b100,
					00,
					00,
					00,
					00,
					00,
					00,
					00,
					00,
					00,
				},
				Enumerable.Repeat((byte)0, 512).ToArray(),
				prgROM,
				chrROM
			));

			var rom = new ROM(testROM);

			Assert.Equal(rom.CHRROM, chrROM);
			Assert.Equal(rom.PRGROM, prgROM);
			Assert.Equal(3, rom.Mapper);
			Assert.Equal(Mirroring.Vertical, rom.Mirroring);
		}
	}
}

