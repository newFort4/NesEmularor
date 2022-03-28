using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using NesEmulator.Core;

var cpu = new CPU();

// cpu.Load(game);
cpu.Reset();

var random = new Random();
var screenState = Enumerable
    .Range(0, 32 * 3 * 32)
    .Select(x => (byte) x)
    .ToArray();

cpu.RunWithCallback(cpu =>
{
    ReadInput();

    cpu.WriteMemory(0xFE, (byte)(1 + (random.Next() % 15)));

    if (ShouldUpdateScreen(cpu, screenState))
    {
        PresentScreen();
    }

    // Thread.Sleep(TimeSpan.FromMilliseconds(0.7));
});

void ReadInput()
{
    const ushort keyAddress = 0xFF;

    if (Console.KeyAvailable)
    {
        var consoleKey = Console.ReadKey().Key;

        switch (consoleKey)
        {
            case ConsoleKey.W:
                cpu.WriteMemory(keyAddress, 0x77);
                break;
            case ConsoleKey.A:
                cpu.WriteMemory(keyAddress, 0x61);
                break;
            case ConsoleKey.S:
                cpu.WriteMemory(keyAddress, 0x73);
                break;
            case ConsoleKey.D:
                cpu.WriteMemory(keyAddress, 0x64);
                break;
            default:
                break;
        }
    }
}

ConsoleColor GetColor(byte data)
{
    switch (data)
    {
        case 0:
            return ConsoleColor.Black;
        case 1:
            return ConsoleColor.White;
        case 2: case 9:
            return ConsoleColor.DarkGray;
        case 3: case 10:
            return ConsoleColor.Red;
        case 4:
        case 11:
            return ConsoleColor.Green;
        case 5:
        case 12:
            return ConsoleColor.Blue;
        case 6:
        case 13:
            return ConsoleColor.Magenta;
        case 7:
        case 14:
            return ConsoleColor.Yellow;
        default:
            return ConsoleColor.Cyan;
    }
}

(byte, byte, byte) ConsoleColorToRgb(ConsoleColor consoleColor)
{
    var color = Color.FromName(Enum.GetName(consoleColor));

    return (color.R, color.G, color.B);
}

ConsoleColor RgbToConsoleColor(byte r, byte g, byte b)
{
    var index = (r > 128 | g > 128 | b > 128) ? 8 : 0;
        index |= (r > 64) ? 4 : 0;
        index |= (g > 64) ? 2 : 0;
        index |= (b > 64) ? 1 : 0;

    return (ConsoleColor)index;
}

bool ShouldUpdateScreen(CPU cpu, byte[] frame)
{
    var frameIdx = 0;
    var shouldUpdate = false;

    for (var i = (ushort)0x0200; i < 0x0600; i++)
    {
        var colorIdx = cpu.ReadMemory(i);
        var (r, g, b) = ConsoleColorToRgb(GetColor(colorIdx));

        if (frame[frameIdx] != r || frame[frameIdx + 1] != g || frame[frameIdx + 2] != b)
        {
            frame[frameIdx] = r;
            frame[frameIdx + 1] = g;
            frame[frameIdx + 2] = b;

            shouldUpdate = true;
        }

        frameIdx += 3;
    }

    return shouldUpdate;
}

void PresentScreen()
{
    Console.Clear();

    for (var i = 0; i < screenState.Length; i += 3)
    {
        Console.ForegroundColor = RgbToConsoleColor(screenState[i], screenState[i + 1], screenState[i + 2]);
        Console.Write('x');

        if (((i / 3) + 1) % 32 == 0)
        {
            Console.WriteLine();
        }
    }

    Console.ResetColor();
}