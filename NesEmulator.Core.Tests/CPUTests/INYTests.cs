using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class INYTests : CPUBaseTests
    {
        private readonly byte INY = OpCode.INY;

        [Fact]
        public void INYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, SomeValue, OpCode.TAY, INY, StopCode });

            Assert.Equal(cpu.RegisterY, SomeValue + 1);
        }

        [Fact]
        public void INYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, AllOnes, OpCode.TAY, INY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void INYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { OpCode.LDA, AllOnesExceptTheLast, OpCode.TAY, INY, StopCode });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
