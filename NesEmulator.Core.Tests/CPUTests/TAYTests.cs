using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class TAYTests : CPUBaseTests
    {
        private readonly byte TAY = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "TAY" && x.AddressingMode == AddressingMode.NoneAddressing)
            .Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void TAYWithValueWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, SomeValue, TAY, BRK });

            Assert.Equal(cpu.RegisterY, SomeValue);
        }

        [Fact]
        public void TAYWithZeroWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllZeroes, TAY, BRK });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Zero), (byte)SRFlag.Zero);
        }

        [Fact]
        public void TAYWithNegativeWorks()
        {
            cpu.LoadAndRun(new byte[] { LDA, AllOnes, TAY, BRK });

            Assert.Equal(cpu.Status & ((byte)SRFlag.Negative), (byte)SRFlag.Negative);
        }
    }
}
