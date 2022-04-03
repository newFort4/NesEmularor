using System;
using System.Linq;

namespace NesEmulator.Core
{
    public class Frame
	{
        public byte[] Data { get; set; }

        private const int Width = 256;
        private const int Height = 240;

        public Frame() => Data = Enumerable
            .Repeat((byte)0, Width * Height * 3)
            .ToArray();

        public void SetPixel(int x, int y, (byte, byte, byte) rgb)
        {
            var @base = y * 3 * Width + x * 3;

            if (@base + 2 < Data.Length)
            {
                Data[@base] = rgb.Item1;
                Data[@base + 1] = rgb.Item2;
                Data[@base + 2] = rgb.Item3;
            }
        }

        public static Frame ShowFrame(byte[] chrROM, int bank, int tileN)
        {
            var frame = new Frame();
            bank *= 0x1000;
            var tile = chrROM[(bank + tileN * 16)..(bank + tileN * 16 + 16)];

            for (var y = 0; y < 8; y++)
            {
                var upper = tile[y];
                var lower = tile[y + 8];

                for (var x = 7; x >= 0; x--)
                {
                    var value = ((1 & upper) << 1) | (1 & lower);
                    upper >>= 1;
                    lower >>= 1;

                    (byte, byte, byte) rgb;

                    switch (value)
                    {
                        case 0:
                            rgb = Pallete.SystemPallete[0x01];
                            break;
                        case 1:
                            rgb = Pallete.SystemPallete[0x23];
                            break;
                        case 2:
                            rgb = Pallete.SystemPallete[0x27];
                            break;
                        case 3:
                            rgb = Pallete.SystemPallete[0x30];
                            break;
                        default:
                            throw new InvalidProgramException();
                    }

                    frame.SetPixel(x, y, rgb);
                }
            }

            return frame;
        }
    }
}

