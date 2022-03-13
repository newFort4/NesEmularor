using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class LDXTests : CPUBaseTests
    {
        private readonly byte LDX = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDX" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void LDXWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, SomeValue, StopCode });

            Assert.Equal(cpu.RegisterX, SomeValue);
        }

        [Fact]
        public void LDXWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, 0x01, StopCode });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void LDXWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, 0x01, StopCode });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void LDXWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, AllZeroes, StopCode });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void LDXWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, AllOnes, StopCode });

            AssertFlag(SRFlag.Negative, true);
        }
    }
}
