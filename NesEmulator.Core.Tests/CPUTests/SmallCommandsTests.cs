using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class SmallCommandsTests : CPUBaseTests
    {
        [Theory]
        [InlineData(OpCodes.SED, SRFlags.Decimal)]
        [InlineData(OpCodes.SEC, SRFlags.Carry)]
        [InlineData(OpCodes.SEI, SRFlags.Interrupt)]
        public void SetsFlagCorrectly(byte opCode, SRFlags flag)
        {
            cpu.Interpret(new byte[] { opCode, StopCode });

            Assert.Equal(cpu.Status & ((byte)flag), (byte)flag);
        }

        [Theory]
        [InlineData(OpCodes.CLD, SRFlags.Decimal)]
        [InlineData(OpCodes.CLV, SRFlags.VOverflow)]
        [InlineData(OpCodes.CLC, SRFlags.Carry)]
        [InlineData(OpCodes.CLI, SRFlags.Interrupt)]
        public void CearsFlagCorrectly(byte opCode, SRFlags flag)
        {
            cpu.Interpret(new byte[] { opCode, StopCode });

            Assert.Equal(cpu.Status | (~(byte)flag), ~(byte)flag);
        }
    }
}
