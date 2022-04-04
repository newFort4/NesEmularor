using System.Runtime.CompilerServices;

namespace NesEmulator.Core
{
    // ToDo: Refactor
    public class Joypad
    {
        public bool Strobe { get; set; } = false;
        public byte ButtonIndex { get; set; } = 0;
        public byte ButtonStatus { get; set; } = 0b00000000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte data)
        {
            Strobe = (data & 1) == 1;

            if (Strobe)
            {
                ButtonIndex = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Read()
        {
            if (ButtonIndex > 7)
            {
                return 1;
            }

            var response = (byte)((ButtonStatus & (1 << ButtonIndex)) >> ButtonIndex);

            if (!Strobe && ButtonIndex <= 7)
            {
                ButtonIndex++;
            }

            return response;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetButtonPressedStatus(JoypadButton joypadButton, bool isPressed) => ButtonStatus = isPressed ? (byte)(ButtonStatus | ((byte)joypadButton)) : (byte)(ButtonStatus & ~((byte)joypadButton));
    }
}
