using Xunit;

namespace NesEmulator.Core.Tests
{
    public abstract class CPUBaseTests
    {
        protected readonly CPU cpu;

        protected readonly byte BRK = 0x00;

        protected const byte SomeValue = 0xAA;
        protected const ushort SomeAddress = 0x10;

        protected const byte AllOnes = 0xFF;
        protected const byte AllOnesExceptTheLast = 0xFE;
        protected const byte AllZeroes = 0x00;

        protected CPUBaseTests() => cpu = new CPU(0x0600);

        protected void AssertFlag(SRFlag flag, bool value)
        {
            var flagByte = cpu.Status & ((byte)flag);

            if (value)
            {
                Assert.Equal(flagByte, (byte)flag);
            } else
            {
                Assert.NotEqual(flagByte, (byte)flag);
            }
        }
    }
}
