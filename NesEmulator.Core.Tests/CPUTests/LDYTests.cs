using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class LDYTests : CPUBaseTests
    {
        private readonly byte LDY = OpCode
            .Codes
            .Single(x => x.Mnemonic == "LDY" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void LDYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, SomeValue, BRK });

            Assert.Equal(cpu.RegisterY, SomeValue);
        }

        [Fact]
        public void LDYWithNonZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, 0x01, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void LDYWithPositiveWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, 0x01, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void LDYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, AllZeroes, BRK });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void LDYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, AllOnes, BRK });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
