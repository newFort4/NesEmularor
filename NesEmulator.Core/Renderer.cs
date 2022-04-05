using System;
using System.Runtime.CompilerServices;

namespace NesEmulator.Core
{
    // ToDo: Refactor
    public static class Renderer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] GetBackgroundPallete(PPU ppu, Span<byte> attributeTable, int tileColumn, int tileRow)
        {
            var attributeTableIdx = tileRow / 4 * 8 + tileColumn / 4;
            var attributeByte = attributeTable[attributeTableIdx];
            var palleteIdx = (tileColumn % 4 / 2, tileRow % 4 / 2)
            switch
            {
                (0, 0) => attributeByte & 0b11,
                (1, 0) => (attributeByte >> 2) & 0b11,
                (0, 1) => (attributeByte >> 4) & 0b11,
                (1, 1) => (attributeByte >> 6) & 0b11,
                _ => throw new InvalidProgramException(),
            };
            var palleteStart = 1 + palleteIdx * 4;

            return new[] { ppu.PalletTable[0], ppu.PalletTable[palleteStart], ppu.PalletTable[palleteStart + 1], ppu.PalletTable[palleteStart + 2] };
        }

        public static void RenderNameTable(PPU ppu, Frame frame, Span<byte> nameTable, Rect viewPort, int shiftX, int shiftY)
        {
            var bank = ppu.Control.BackgroundPatternAddress();

            var span = ppu.ChrROM.AsSpan();

            var attributeTable = nameTable[0x3C0..0x400];

            for (var i = 0; i < 0x03C0; i++)
            {
                var tileIndex = ppu.VRAM[i];

                var tileX = i % 32;
                var tileY = i / 32;

                var tile = span.Slice(bank + tileIndex * 16, 16); //ppu.ChrROM[(bank + tileIndex * 16)..(bank + tileIndex * 16 + 16)];
                var pallete = GetBackgroundPallete(ppu, attributeTable, tileX, tileY);

                for (var y = 0; y < 8; y++)
                {
                    var upper = tile[y];
                    var lower = tile[y + 8];

                    for (var x = 7; x >= 0; x--)
                    {
                        var value = ((1 & lower) << 1) | (1 & upper);

                        upper >>= 1;
                        lower >>= 1;
                        var rgb = value switch
                        {
                            0 => Pallete.SystemPallete[ppu.PalletTable[0]],
                            1 => Pallete.SystemPallete[pallete[1]],
                            2 => Pallete.SystemPallete[pallete[2]],
                            3 => Pallete.SystemPallete[pallete[3]],
                            _ => throw new InvalidProgramException(),
                        };

                        var pixelX = tileX * 8 + x;
                        var pixelY = tileY * 8 + y;

                        if (pixelX >= viewPort.X1 && pixelX < viewPort.X2 && pixelY >= viewPort.Y1 && pixelY < viewPort.Y2)
                        {
                            frame.SetPixel(shiftX + pixelX, shiftY + pixelY, rgb);
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Render(PPU ppu, Frame frame)
        {
            var scrollX = ppu.Scroll.ScrollX;
            var scrollY = ppu.Scroll.ScrollY;

            var span = ppu.VRAM.AsSpan();

            const int tableLength = 0x400;

            var (mainNameTableOffset, secondNameTableOffset) = (ppu.Mirroring, ppu.Control.GetNametableAddress()) switch
            {
                (Mirroring.Vertical, 0x2000) or (Mirroring.Vertical, 0x2800) or (Mirroring.Horizontal, 0x2000) or (Mirroring.Horizontal, 0x2400) => (0, 0x400),
                (Mirroring.Vertical, 0x2400) or (Mirroring.Vertical, 0x2C00) or (Mirroring.Horizontal, 0x2800) or (Mirroring.Horizontal, 0x2c00) => (0x400, 0),
                _ => throw new InvalidProgramException()
            };

            RenderNameTable(ppu, frame, span.Slice(mainNameTableOffset, tableLength), new Rect(scrollX, scrollY, 256, 240), -scrollX, -scrollY);

            if (scrollX > 0)
            {
                RenderNameTable(ppu, frame, span.Slice(secondNameTableOffset, tableLength), new Rect(0, 0, scrollX, 240), 256 - scrollX, 0);
            } else if (scrollY > 0)
            {
                RenderNameTable(ppu, frame, span.Slice(secondNameTableOffset, tableLength), new Rect(0, 0, 256, scrollY), 0, 240 - scrollY);
            }

            for (var i = ppu.OAMData.Length - 4; i >= 0; i -= 4)
            {
                var tileIdx = ppu.OAMData[i + 1];
                var tileX = ppu.OAMData[i + 3];
                var tileY = ppu.OAMData[i];

                var flipVertical = ((ppu.OAMData[i + 2] >> 7) & 1) == 1;
                var flipHorizontal = ((ppu.OAMData[i + 2] >> 6) & 1) == 1;

                var palleteIdx = ppu.OAMData[i + 2] & 0b11;

                var spritePallete = GetSpritePallete(ppu, palleteIdx);
                var bank = ppu.Control.SpritePatternAddress();

                var tile = ppu.ChrROM.AsSpan().Slice(bank + tileIdx * 16, 16); ;

                for (var y = 0; y < 8; y++)
                {
                    var upper = tile[y];
                    var lower = tile[y + 8];

                    for (var x = 7; x >= 0; x--)
                    {
                        var value = ((1 & lower) << 1) | (1 & upper);

                        upper >>= 1;
                        lower >>= 1;

                        (byte, byte, byte) rgb;

                        switch (value)
                        {
                            case 0:
                                continue;
                            case 1:
                                rgb = Pallete.SystemPallete[spritePallete[1]];
                                break;
                            case 2:
                                rgb = Pallete.SystemPallete[spritePallete[2]];
                                break;
                            case 3:
                                rgb = Pallete.SystemPallete[spritePallete[3]];
                                break;
                            default:
                                throw new InvalidProgramException();
                        }

                        switch (flipHorizontal, flipVertical)
                        {
                            case (false, false):
                                frame.SetPixel(tileX + x, tileY + y, rgb);
                                break;
                            case (true, false):
                                frame.SetPixel(tileX + 7 - x, tileY + y, rgb);
                                break;
                            case (false, true):
                                frame.SetPixel(tileX + x, tileY + 7 - y, rgb);
                                break;
                            case (true, true):
                                frame.SetPixel(tileX + 7 - x, tileY + 7 - y, rgb);
                                break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] GetSpritePallete(PPU ppu, int palleteIdx)
        {
            var start = 0x11 + palleteIdx * 4;

            return new[] { (byte)0, ppu.PalletTable[start], ppu.PalletTable[start + 1], ppu.PalletTable[start + 2] };
        }

        public record Rect
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }

            public Rect(int x1, int y1, int x2, int y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }
        }
    }
}

