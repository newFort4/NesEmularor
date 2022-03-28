using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests.Branches
{
    public class BPLTests : CPUBaseTests
    {
        private readonly byte BPL = OpCode.Codes.Single(x => x.Mnemonic == nameof(BPL)).Code;
        private readonly byte LDA = OpCode.Codes.Single(x => x.Mnemonic == nameof(LDA) && x.AddressingMode == AddressingMode.Immediate).Code;

        [Fact]
        public void BPLWorksCorrectlyForPositiveValue()
        {
            cpu.LoadAndRun(new byte[] { LDA, 0x01, BPL, 0x02, LDA, 2, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BPLWorksCorrectlyForNegativeValue()
        {
            cpu.LoadAndRun(new byte[] { LDA, 0xFF, BPL, 0x02, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BPLWorksCorrectlyForNegativeJump()
        {
            cpu.LoadAndRun(new byte[] { LDA, 0x12, BPL, 3, LDA, SomeValue, BRK, BPL, 0xFB, LDA, 0x32, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }
    }
}
