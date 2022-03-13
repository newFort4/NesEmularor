using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class LDYTests : CPUBaseTests
    {
        private const byte LDY = OpCode.LDY;

        [Fact]
        public void LDYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, SomeValue, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue);
        }

        [Fact]
        public void LDYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, AllZeroes, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void LDYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDY, AllOnes, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
