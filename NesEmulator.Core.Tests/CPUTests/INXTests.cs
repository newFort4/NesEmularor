using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class INXTests : CPUBaseTests
    {
        private readonly byte INX = OpCode
            .Codes
            .Single(x => x.Mnemonic == "INX" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;
        private readonly byte LDA = OpCode
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte TAX = OpCode
            .Codes
            .Single(x => x.Mnemonic == "TAX" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;

        [Fact]
        public void INXWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, TAX, INX, BRK });

            Assert.Equal(cpu.RegisterX, SomeValue + 1);
        }

        [Fact]
        public void INXWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, 0x01, TAX, INX, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void INXWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAX, INX, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void INXWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, TAX, INX, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void INXWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnesExceptTheLast, TAX, INX, BRK });

            AssertFlag(SRFlag.Negative, true);
        }
    }
}
