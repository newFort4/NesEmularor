using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class TAYTests : CPUBaseTests
    {
        private const byte TAY = OpCode.TAY;

        [Fact]
        public void TAYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, SomeValue, TAY, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue);
        }

        [Fact]
        public void TAYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, AllZeroes, TAY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void TAYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, AllOnes, TAY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
