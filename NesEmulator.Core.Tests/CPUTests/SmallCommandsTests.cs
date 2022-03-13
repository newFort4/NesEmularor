using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NesEmulator.Core.Tests.CPUTests
{
    public class SmallCommandsTests : CPUBaseTests
    {
        [Fact]
        public void SetsFlagCorrectly()
        {
            var tuples = new List<Tuple<byte, SRFlag>>
            {
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "SED" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Decimal),
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "SEC" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Carry),
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "SEI" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Interrupt)
            };

            foreach (var tuple in tuples)
            {
                cpu.LoadAndRun(new byte[] { tuple.Item1, StopCode });

                Assert.Equal(cpu.Status & ((byte)tuple.Item2), (byte)tuple.Item2);
            }
        }

        [Fact]
        public void CearsFlagCorrectly()
        {
            var tuples = new List<Tuple<byte, SRFlag>>
            {
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "CLD" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Decimal),
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "CLV" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.VOverflow),
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "CLC" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Carry),
                new Tuple<byte, SRFlag>(OpCodes
                .Codes
                .Single(x => x.Mnemonic == "CLI" && x.AddressingMode == AddressingMode.NoneAddressing)
                .Code, SRFlag.Interrupt)
            };

            foreach (var tuple in tuples)
            {
                cpu.LoadAndRun(new byte[] { tuple.Item1, StopCode });

                Assert.Equal(cpu.Status | (~(byte)tuple.Item2), ~(byte)tuple.Item2);
            }
        }
    }
}
