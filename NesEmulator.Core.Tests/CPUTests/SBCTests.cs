using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class SBCTests : CPUBaseTests
    {
        private readonly byte SBC = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(SBC) && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte CLC = OpCodes.Codes.Single(x => x.Mnemonic == nameof(CLC)).Code;
        private readonly byte SEC = OpCodes.Codes.Single(x => x.Mnemonic == nameof(SEC)).Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void SBCWorksCorrectlyWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 14, SBC, 12, BRK });

            Assert.Equal(2, cpu.RegisterA);
        }

        [Fact]
        public void SBCWorksCorrectlyWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 255, SBC, 1, BRK });

            Assert.Equal(255, cpu.RegisterA);
        }
    }
}
