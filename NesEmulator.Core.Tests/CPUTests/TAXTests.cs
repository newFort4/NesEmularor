using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class TAXTests : CPUBaseTests
    {
        private readonly byte TAX = OpCode
            .Codes
            .Single(x => x.Mnemonic == "TAX" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;
        private readonly byte LDA = OpCode
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void TAXWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, TAX, BRK });

            Assert.Equal(cpu.RegisterX, SomeValue);
        }

        [Fact]
        public void TAXWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes + 1, TAX, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void TAXWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAX, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void TAXWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes + 1, TAX, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void TAXWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, TAX, BRK });

            AssertFlag(SRFlag.Negative, true);
        }
    }
}
