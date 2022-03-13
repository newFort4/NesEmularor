using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class INYTests : CPUBaseTests
    {
        private readonly byte INY = OpCodes.INY;

        [Fact]
        public void INYWithValueWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, SomeValue, OpCodes.TAY, INY, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue + 1);
        }

        [Fact]
        public void INYWithZeroWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, AllOnes, OpCodes.TAY, INY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlags.Zero), (byte)SRFlags.Zero);
        }

        [Fact]
        public void INYWithNegativeWorks()
        {
            cpu.Interpret(new byte[] { OpCodes.LDA, AllOnesExceptTheLast, OpCodes.TAY, INY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlags.Negative), (byte)SRFlags.Negative);
        }
    }
}
