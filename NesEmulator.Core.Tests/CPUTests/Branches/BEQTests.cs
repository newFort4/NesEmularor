using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests.Branches
{
    public class BEQTests : CPUBaseTests
    {
        private readonly byte BEQ = OpCodes.Codes.Single(x => x.Mnemonic == nameof(BEQ)).Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(LDA) && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void BEQWorksCorrectlyForZero()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, BEQ, 0x02, LDA, 2, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BEQWorksCorrectlyForNonZerValue()
        {
            cpu.LoadAndRun(new byte[] { LDA, 0xFF, BEQ, 0x02, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BEQWorksCorrectlyForNegativeJump()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, BEQ, 3, LDA, SomeValue, BRK, BEQ, 0xFC, LDA, 0x32, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }
    }
}
