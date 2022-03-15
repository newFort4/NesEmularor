using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class ADCTests : CPUBaseTests
    {
        private readonly byte ADC = OpCodes
            .Codes
            .Single(x => x.Mnemonic == nameof(ADC) && x.AddressingMode == AddressingMode.Immediate)
            .Code;
        private readonly byte CLC = OpCodes.Codes.Single(x => x.Mnemonic == nameof(CLC)).Code;
        private readonly byte SEC = OpCodes.Codes.Single(x => x.Mnemonic == nameof(SEC)).Code;
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        [Fact]
        public void ADCWorksCorrectlyWithoutCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 1, ADC, 2, BRK });

            Assert.Equal(3, cpu.RegisterA);
        }

        [Fact]
        public void ADCWorksCorrectlyWithCarryFlag()
        {
            cpu.LoadAndRun(new byte[] { LDA, 1, SEC, ADC, 0x02, BRK });

            Assert.Equal(4, cpu.RegisterA);
        }

        [Fact]
        public void ADCSetsNegativeFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 127, ADC, 1, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void ADCSetsNegativeFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 127, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Negative, true);
        }

        [Fact]
        public void ADCClearsNegativeFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 1, ADC, 1, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void ADCClearsNegativeFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 1, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Negative, false);
        }

        [Fact]
        public void ADCSetsZeroFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 0, ADC, 0, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void ADCSetsZeroFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 255, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Zero, true);
        }

        [Fact]
        public void ADCClearsZeroFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 1, ADC, 1, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void ADCClearsZeroFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 1, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Zero, false);
        }

        [Fact]
        public void ADCSetsCarryFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 255, ADC, 1, BRK });

            AssertFlag(SRFlag.Carry, true);
        }

        [Fact]
        public void ADCSetsCarryFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 255, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Carry, true);
        }

        [Fact]
        public void ADCClearsCarryFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 254, ADC, 1, BRK });

            AssertFlag(SRFlag.Carry, false);
        }

        [Fact]
        public void ADCClearsCarryFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 254, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.Carry, false);
        }

        [Fact]
        public void ADCSetsOverflowFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 127, ADC, 1, BRK });

            AssertFlag(SRFlag.VOverflow, true);
        }

        [Fact]
        public void ADCSetsOverflowFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 127, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.VOverflow, true);
        }

        [Fact]
        public void ADCClearsOverflowFlagWithoutCarry()
        {
            cpu.LoadAndRun(new byte[] { CLC, LDA, 126, ADC, 1, BRK });

            AssertFlag(SRFlag.VOverflow, false);
        }

        [Fact]
        public void ADCClearsOverflowFlagWithCarry()
        {
            cpu.LoadAndRun(new byte[] { LDA, 126, SEC, ADC, 0, BRK });

            AssertFlag(SRFlag.VOverflow, false);
        }
    }
}
