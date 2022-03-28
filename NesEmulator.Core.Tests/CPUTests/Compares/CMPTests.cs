using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests.Compares
{
    public class CMPTests : CPUBaseTests
    {
        private readonly byte CMP = OpCode
            .Codes
            .Single(x => x.Mnemonic == nameof(CMP) && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte LDA = OpCode
            .Codes
            .Single(x => x.Mnemonic == nameof(LDA) && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void SetsCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, CMP, SomeValue, BRK });

            AssertFlag(SRFlag.Carry, true);
        }

        [Fact]
        public void ClearsCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, CMP, SomeValue + 1, BRK });

            AssertFlag(SRFlag.Carry, false);
        }

        [Fact]
        public void SetsNegativeFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, 43, CMP, 55, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void ClearsNegativeFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, 55, CMP, 32, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void SetsZeroFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, CMP, SomeValue, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void ClearsZeroFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue + 1, CMP, SomeValue, BRK });

            AssertFlag(SRFlag.Zero, false);
        }
    }
}
