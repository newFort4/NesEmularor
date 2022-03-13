using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class TAYTests : CPUBaseTests
    {
        private const byte TAY = OpCodes.TAY;

        [Fact]
        public void TAYWithValueWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, SomeValue, TAY, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue);
        }

        [Fact]
        public void TAYWithZeroWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, AllZeroes, TAY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlags.Zero), (byte)SRFlags.Zero);
        }

        [Fact]
        public void TAYWithNegativeWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, AllOnes, TAY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlags.Negative), (byte)SRFlags.Negative);
        }
    }
}
