using System.Linq;
using NesEmulator.Core;

namespace NesEmulator.Example
{
    public static class CPUExtensions
	{
		public static string Trace(this CPU cpu)
        {
            return $"{cpu.ProgramCounter:X4}\t" +
                $"{cpu.ReadMemory(cpu.ProgramCounter):X4}\t" +
                $"{OpCode.Codes.FirstOrDefault(x => x.Code == cpu.ReadMemory(cpu.ProgramCounter)).Mnemonic}" +
                $"A:{cpu.RegisterA:X4} X:{cpu.RegisterX:X4} Y:{cpu.RegisterY:X4} P:{cpu.Status:X4} SP:{cpu.StackPointer:X4}";
        }
	}
}

