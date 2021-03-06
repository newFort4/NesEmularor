using System;
using NesEmulator.Core;
using NesEmulator.Example;

var cpu = new CPU("nestest.nes", (a, b) => { });

cpu.Reset();

// ToDo: Implement full trace
cpu.RunWithCallback(cpu =>
{
    Console.WriteLine(cpu.Trace());
});