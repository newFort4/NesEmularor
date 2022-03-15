using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class STATests : CPUBaseTests
    {
        private readonly byte LDA = OpCodes
            .Codes
            .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
            .Code;

        public class STAZeroPageTests : STATests
        {
            private readonly byte STA = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "STA" && x.AddressingMode == AddressingMode.ZeroPage)
                .Code;

            [Fact]
            public void STAWorksCorrectly()
            {
                cpu.LoadAndRun(new byte[] { LDA, SomeValue, STA, (byte)SomeAddress, BRK });

                Assert.Equal(cpu.ReadMemory(SomeAddress), SomeValue);
            }
        }

        public class STAZeroPageXTests : STATests
        {
            private readonly byte STA = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "STA" && x.AddressingMode == AddressingMode.ZeroPageX)
                .Code;

            private readonly byte LDX = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "LDX" && x.AddressingMode == AddressingMode.Immediate)
                .Code;

            [Fact]
            public void STAWorksCorrectly()
            {
                cpu.LoadAndRun(new byte[]
                {
                    LDA, SomeValue,
                    LDX, 4,
                    STA, (byte)SomeAddress,
                    BRK
                });

                Assert.Equal(cpu.ReadMemory(SomeAddress + 4), SomeValue);
            }
        }
    }
}
