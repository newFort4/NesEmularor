using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class INYTests : CPUBaseTests
    {
        private readonly byte INY = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "INY" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte TAY = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "TAY" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;

        [Fact]
        public void INYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, TAY, INY, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue + 1);
        }

        [Fact]
        public void INYWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAY, INY, StopCode });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void INYWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAY, INY, StopCode });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void INYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, TAY, INY, StopCode });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void INYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnesExceptTheLast, TAY, INY, StopCode });

            AssertFlag(SRFlag.Negative, true);
        }
    }
}
