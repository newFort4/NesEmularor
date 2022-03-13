namespace NesEmulator.Core
{
    /// <summary>
    /// This class implements CPU of the processor 6502
    /// </summary>
    public class CPU
    {
        public byte Accumulator { get; private set; }
        public byte RegisterX { get; private set; }
        public byte RegisterY { get; private set; }

        public byte Status { get; set; }

        private ushort ProgramCounter { get; set; }
        private byte StackPointer { get; set; }

        public CPU()
        {
            Accumulator = 0;
            Status = 0;
            ProgramCounter = 0;
        }

        public void Interpret(byte[] program)
        {
            ProgramCounter = 0;

            while (true)
            {
                var opCode = program[ProgramCounter];

                ProgramCounter++;

                switch (opCode)
                {
                    case OpCodes.LDA:
                        Accumulator = program[ProgramCounter];
                        ProgramCounter++;
                        SetNZ(Accumulator);

                        break;
                    case OpCodes.TAX:
                        RegisterX = Accumulator;
                        SetNZ(RegisterX);

                        break;
                    case OpCodes.TAY:
                        RegisterY = Accumulator;
                        SetNZ(RegisterY);

                        break;
                    case OpCodes.INX:
                        RegisterX++;
                        SetNZ(RegisterX);

                        break;
                    case OpCodes.INY:
                        RegisterY++;
                        SetNZ(RegisterY);

                        break;
                    default:
                        return;
                }
            }
        }

        private void SetNZ(byte value)
        {
            if (value == 0)
            {
                Status |= (byte)SRFlags.Zero;
            }
            else
            {
                Status = (byte)(Status | ~(byte)SRFlags.Zero);
            }

            if ((value & ((byte)SRFlags.Negative)) != 0)
            {
                Status |= (byte)SRFlags.Negative;
            }
            else
            {
                Status = (byte)(Status & ~(byte)SRFlags.Negative);
            }
        }
    }
}
