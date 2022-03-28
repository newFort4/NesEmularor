using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class INYTests : CPUBaseTests
    {
        private readonly byte INY = OpCode
            .Codes
            .Single(x => x.Mnemonic == "INY" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;
        private readonly byte LDA = OpCode
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte TAY = OpCode
            .Codes
            .Single(x => x.Mnemonic == "TAY" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;

        [Fact]
        public void INYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, TAY, INY, BRK });

            Assert.Equal(cpu.RegisterY, SomeValue + 1);
        }

        [Fact]
        public void INYWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAY, INY, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void INYWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAY, INY, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void INYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, TAY, INY, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void INYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnesExceptTheLast, TAY, INY, BRK });

            AssertFlag(SRFlag.Negative, true);
        }
    }
}
