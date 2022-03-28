using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class SBCTests : CPUBaseTests
    {
        private readonly byte SBC = OpCode
            .Codes
            .Single(x => x.Mnemonic == nameof(SBC) && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte CLC = OpCode.Codes.Single(x => x.Mnemonic == nameof(CLC)).Code;
        private readonly byte SEC = OpCode.Codes.Single(x => x.Mnemonic == nameof(SEC)).Code;
        private readonly byte LDA = OpCode
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void SBCWorksCorrectlyWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 14, SBC, 12, BRK });

            Assert.Equal(1, cpu.RegisterA);
        }

        [Fact]
        public void SBCWorksCorrectlyWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 255, SBC, 1, BRK });

            Assert.Equal(254, cpu.RegisterA);
        }

        [Fact]
        public void SBCSetsNegativeFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 12, SBC, 13, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void SBCSetsNegativeFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 12, SBC, 14, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void SBCClearsNegativeFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 14, SBC, 13, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void SBCClearsNegativeFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 13, SBC, 13, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void SBCSetsZeroFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 12, SBC, 11, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void SBCSetsZeroFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 12, SBC, 12, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void SBCClearsZeroFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 15, SBC, 13, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void SBCClearsZeroFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 13, SBC, 12, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void SBCSetsCarryFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 13, SBC, 12, BRK });

            AssertFlag(SRFlag.Carry, true);
        }

        [Fact]
        public void SBCSetsCarryFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 12, SBC, 12, BRK });

            AssertFlag(SRFlag.Carry, true);
        }

        [Fact]
        public void SBCClearsCarryFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 12, SBC, 12, BRK });

            AssertFlag(SRFlag.Carry, false);
        }

        [Fact]
        public void SBCClearsCarryFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 13, SBC, 14, BRK });

            AssertFlag(SRFlag.Carry, false);
        }

        [Fact]
        public void SBCSetsOverflowFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 128, SBC, 0, BRK });

            AssertFlag(SRFlag.VOverflow, true);
        }

        [Fact]
        public void SBCSetsOverflowFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 128, SBC, 1, BRK });

            AssertFlag(SRFlag.VOverflow, true);
        }

        [Fact]
        public void SBCClearsOverflowFlagWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 129, SBC, 0, BRK });

            AssertFlag(SRFlag.VOverflow, false);
        }

        [Fact]
        public void SBCClearsOverflowFlagWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { SEC, LDA, 129, SBC, 1, BRK });

            AssertFlag(SRFlag.VOverflow, false);
        }
    }
}
