using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class SmallCommandsTests : CPUBaseTests
    {
        [Theory]
        [InlineData(OpCode.SED, SRFlag.Decimal)]
        [InlineData(OpCode.SEC, SRFlag.Carry)]
        [InlineData(OpCode.SEI, SRFlag.Interrupt)]
        public void SetsFlagCorrectly(byte opCode, SRFlag flag)
        {
            cpu.LoadAndRun(new byte[] { opCode, StopCode });

            Assert.Equal(cpu.Status & ((byte)flag), (byte)flag);
        }

        [Theory]
        [InlineData(OpCode.CLD, SRFlag.Decimal)]
        [InlineData(OpCode.CLV, SRFlag.VOverflow)]
        [InlineData(OpCode.CLC, SRFlag.Carry)]
        [InlineData(OpCode.CLI, SRFlag.Interrupt)]
        public void CearsFlagCorrectly(byte opCode, SRFlag flag)
        {
            cpu.LoadAndRun(new byte[] { opCode, StopCode });

            Assert.Equal(cpu.Status | (~(byte)flag), ~(byte)flag);
        }
    }
}
