using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class LDXTests : CPUBaseTests
    {
        private const byte LDX = OpCode.LDX;

        [Fact]
        public void LDXWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, SomeValue, StopCode });

            Assert.Equal(cpu.RegisterX, SomeValue);
        }

        [Fact]
        public void LDXWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, AllZeroes, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void LDXWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDX, AllOnes, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
