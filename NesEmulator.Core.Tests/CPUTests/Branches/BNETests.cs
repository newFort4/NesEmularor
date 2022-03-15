using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests.Branches
{
    public class BNETests : CPUBaseTests
    {
        private readonly byte BNE = OpCodes.Codes.Single(x => x.Mnemonic == nameof(BNE)).Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(LDA) && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void BNEWorksCorrectlyForNonZero()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnesExceptTheLast, BNE, 0x02, LDA, 2, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BNEWorksCorrectlyForZero()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, BNE, 0x02, LDA, SomeValue, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }

        [Fact]
        public void BNEWorksCorrectlyForNegativeJump()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, BNE, 3, LDA, SomeValue, BRK, BNE, 252, LDA, 0x32, BRK });

            Assert.Equal(cpu.RegisterA, SomeValue);
        }
    }
}
