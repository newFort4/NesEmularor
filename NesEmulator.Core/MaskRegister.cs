using System.Collections.Generic;

namespace NesEmulator.Core
{
    enum MaskRegisterEnum
    {
		GrayScale = 0b00000001,
		LeftMost8PixelBackground = 0b00000010,
		LeftMost8PixelSprite = 0b00000100,
		ShowBackground = 0b00001000,
		ShowSprites = 0b00010000,
		EmphasiseRed = 0b00100000,
		EmphasiseGreen = 0b01000000,
		EmphasiseBlue = 0b10000000
    }

	public enum ColorEnum
    {
		Red,
		Green,
		Blue
    }

    public class MaskRegister
	{
		public byte Value { get; set; } = 0;

		public bool IsGrayScale => (Value & ((byte)MaskRegisterEnum.GrayScale)) != 0;
		public bool LeftMost8PixelBackground => (Value & ((byte)MaskRegisterEnum.LeftMost8PixelBackground)) != 0;
		public bool LeftMost8PixelSprite => (Value & ((byte)MaskRegisterEnum.LeftMost8PixelSprite)) != 0;
		public bool ShouldShowBackground => (Value & ((byte)MaskRegisterEnum.ShowBackground)) != 0;
		public bool ShoukdShowSprites => (Value & ((byte)MaskRegisterEnum.ShowSprites)) != 0;

		public void Update(byte data) => Value = data;

		// ToDo: Refactor
		public ColorEnum[] Emphasise()
        {
			var colors = new List<ColorEnum>();

			if ((Value & ((byte)MaskRegisterEnum.EmphasiseRed)) != 0)
            {
				colors.Add(ColorEnum.Red);
            }

			if ((Value & ((byte)MaskRegisterEnum.EmphasiseGreen)) != 0)
            {
				colors.Add(ColorEnum.Green);
            }

			if ((Value & ((byte)MaskRegisterEnum.EmphasiseBlue)) != 0)
            {
				colors.Add(ColorEnum.Blue);
            }

			return colors.ToArray();
        }
	}
}

