using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class STATests : CPUBaseTests
    {
        public class STAZeroPageTests : STATests
        {
            private const byte STA = 0x85;

            [Fact]
            public void STAWorksCorrectly()
            {
                cpu.LoadAndRun(new byte[] { OpCode.LDA, SomeValue, STA, (byte)SomeAddress, StopCode });

                Assert.Equal(cpu.ReadMemory(SomeAddress), SomeValue);
            }
        }

        public class STAZeroPageXTests : STATests
        {
            private const byte STA = 0x95;

            [Fact]
            public void STAWorksCorrectly()
            {
                cpu.LoadAndRun(new byte[]
                {
                    OpCode.LDA, SomeValue,
                    OpCode.LDX, 4,
                    STA, (byte)SomeAddress,
                    StopCode
                });

                Assert.Equal(cpu.ReadMemory(SomeAddress + 4), SomeValue);
            }
        }
    }
}
