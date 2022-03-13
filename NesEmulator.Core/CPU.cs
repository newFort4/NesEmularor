using System;

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
                    case OpCodes.SED:
                        SetFlag(SRFlags.Decimal);

                        break;
                    case OpCodes.SEC:
                        SetFlag(SRFlags.Carry);

                        break;
                    case OpCodes.SEI:
                        SetFlag(SRFlags.Interrupt);

                        break;
                    case OpCodes.CLD:
                        ClearFlag(SRFlags.Decimal);

                        break;
                    case OpCodes.CLV:
                        ClearFlag(SRFlags.VOverflow);

                        break;
                    case OpCodes.CLC:
                        ClearFlag(SRFlags.Carry);

                        break;
                    case OpCodes.CLI:
                        ClearFlag(SRFlags.Interrupt);

                        break;
                    case 0xFF:
                        return;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }


        private byte SetFlag(SRFlags flag) => Status |= (byte)flag;
        private byte ClearFlag(SRFlags flag) => Status = (byte)(Status | ~(byte)flag);

        private void SetNZ(byte value)
        {
            _ = value == 0 ? SetFlag(SRFlags.Zero) : ClearFlag(SRFlags.Zero);
            _ = (value & ((byte)SRFlags.Negative)) != 0 ? SetFlag(SRFlags.Negative) : ClearFlag(SRFlags.Negative);
        }
    }
}
