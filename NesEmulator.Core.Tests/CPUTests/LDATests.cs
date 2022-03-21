using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class LDATests : CPUBaseTests
    {
        public class LDAImmediateTests : LDATests
        {
            private readonly byte LDA = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Immediate)
                .Code;

            [Fact]
            public void LDAWithValueWorks()
            {
                cpu.LoadAndRun(new byte[] { LDA, SomeValue, BRK });

                Assert.Equal(cpu.RegisterA, SomeValue);
            }

            [Fact]
            public void LDAWithNonZeroWorks()
            {
                cpu.LoadAndRun(new byte[] { LDA, 0x01, BRK });

                AssertFlag(SRFlag.Zero, false);
            }

            [Fact]
            public void LDAWithPositiveWorks()
            {
                cpu.LoadAndRun(new byte[] { LDA, 0x01, BRK });

                AssertFlag(SRFlag.Negative, false);
            }

            [Fact]
            public void LDAWithZeroWorks()
            {
                cpu.LoadAndRun(new byte[] { LDA, AllZeroes, BRK });

                AssertFlag(SRFlag.Zero, true);
            }

            [Fact]
            public void LDAWithNegativeWorks()
            {
                cpu.LoadAndRun(new byte[] { LDA, AllOnes, BRK });

                AssertFlag(SRFlag.Negative, true);
            }
        }

        // ToDo: Check flags
        public class LDAZeroPageTests : LDATests
        {
            private readonly byte LDA = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.ZeroPage)
                .Code;

            [Fact]
            public void LDAWithValueWorks()
            {
                cpu.WriteMemory(SomeAddress, SomeValue);

                cpu.LoadAndRun(new byte[] { LDA, (byte)SomeAddress, BRK });

                Assert.Equal(cpu.RegisterA, SomeValue);
            }
        }

        // ToDo: Check flags
        public class LDAAbsoluteTests : LDATests
        {
            private readonly byte LDA = OpCodes
                .Codes
                .Single(x => x.Mnemonic == "LDA" && x.AddressingMode == AddressingMode.Absolute)
                .Code;

            [Fact]
            public void LDAWithValueWorks()
            {
                cpu.WriteMemory(SomeAddress, SomeValue);

                cpu.LoadAndRun(new byte[] { LDA, (byte)SomeAddress, SomeAddress >> 8, BRK });

                Assert.Equal(cpu.RegisterA, SomeValue);
            }
        }

        // ToDo: Add all addressing modes
    }
}
