using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class BITTests : CPUBaseTests
    {
        private readonly byte BIT = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(BIT) && x.AddressingMode == AddressingMode.Absolute)
            .Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(LDA) && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void SetsZeroFlag()
        {
            cpu.WriteMemory(0x1234, 0b11110000);
            cpu.LoadAndRun(new byte[] { LDA, 0b00001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void ClearsZeroFlag()
        {
            cpu.WriteMemory(0x1234, 0b11111000);
            cpu.LoadAndRun(new byte[] { LDA, 0b00001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void SetsNegativeFlag()
        {
            cpu.WriteMemory(0x1234, 0b11111000);
            cpu.LoadAndRun(new byte[] { LDA, 0b00001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void ClearsNegativeFlag()
        {
            cpu.WriteMemory(0x1234, 0b01111000);
            cpu.LoadAndRun(new byte[] { LDA, 0b00001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void SetsVOverflowFlag()
        {
            cpu.WriteMemory(0x1234, 0b01111000);
            cpu.LoadAndRun(new byte[] { LDA, 0b01001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.VOverflow, true);
        }

        [Fact]
        public void ClearsVOverflowFlag()
        {
            cpu.WriteMemory(0x1234, 0b00111000);
            cpu.LoadAndRun(new byte[] { LDA, 0b00001111, BIT, 0x34, 0x12, BRK });

            AssertFlag(SRFlag.VOverflow, false);
        }
    }
}
