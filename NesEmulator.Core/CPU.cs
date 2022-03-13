using System;
using System.Linq;

namespace NesEmulator.Core
{
    /// <summary>
    /// This class implements the processor 6502
    /// </summary>
    public class CPU
    {
        public byte Accumulator { get; private set; } = 0x00;
        public byte RegisterX { get; private set; } = 0x00;
        public byte RegisterY { get; private set; } = 0x00;

        public byte Status { get; set; } = 0b00000000;

        private ushort ProgramCounter { get; set; } = 0;
        private byte StackPointer { get; set; } = 0;

        private const ushort ResetVector = 0xFFFC;
        private const ushort ProgramOffset = 0x8000;

        private byte[] memory { get; set; } = new byte[ushort.MaxValue];

        public void LoadAndRun(byte[] program)
        {
            Load(program);
            Reset();
            Run();
        }

        public void Load(byte[] program)
        {
            // ToDo: Check ArraySegment<T>

            Array.Copy(program, 0, memory, ProgramOffset, program.Length);

            WriteMemoryUshort(ResetVector, ProgramOffset);
        }

        public void Run()
        {
            // ToDo: Implement All OpCodes

            while (true)
            {
                var opCode = ReadMemory(ProgramCounter);

                ProgramCounter++;

                var generalOpCode = OpCodes.Codes.SingleOrDefault(x => x.Code == opCode);

                switch (opCode)
                {
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "LDA").Select(x => x.Code).Contains(opCode):
                        LDA(generalOpCode.AddressingMode);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "LDX").Select(x => x.Code).Contains(opCode):
                        RegisterX = ReadMemory(ProgramCounter);
                        SetNZ(RegisterX);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "LDY").Select(x => x.Code).Contains(opCode):
                        RegisterY = ReadMemory(ProgramCounter);
                        SetNZ(RegisterY);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "TAX").Select(x => x.Code).Contains(opCode):
                        RegisterX = Accumulator;
                        SetNZ(RegisterX);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "TAY").Select(x => x.Code).Contains(opCode):
                        RegisterY = Accumulator;
                        SetNZ(RegisterY);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "STA").Select(x => x.Code).Contains(opCode):
                        STA(generalOpCode.AddressingMode);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "AND").Select(x => x.Code).Contains(opCode):
                        AND(generalOpCode.AddressingMode);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "EOR").Select(x => x.Code).Contains(opCode):
                        EOR(generalOpCode.AddressingMode);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "ORA").Select(x => x.Code).Contains(opCode):
                        ORA(generalOpCode.AddressingMode);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "INX").Select(x => x.Code).Contains(opCode):
                        RegisterX++;
                        SetNZ(RegisterX);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "INY").Select(x => x.Code).Contains(opCode):
                        RegisterY++;
                        SetNZ(RegisterY);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "SED").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Decimal);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "SEC").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Carry);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "SEI").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Interrupt);
                        break;

                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "CLD").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Decimal);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "CLV").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.VOverflow);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "CLC").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Carry);
                        break;
                    case var _ when OpCodes.Codes.Where(x => x.Mnemonic == "CLI").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Interrupt);
                        break;

                    case 0xFF:
                        return;
                    default:
                        throw new InvalidOperationException();
                }

                ProgramCounter = (ushort)(ProgramCounter + generalOpCode.Length - 1);
            }
        }

        private void LDA(AddressingMode addressingMode)
        {
            var address = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            Accumulator = value;
            SetNZ(Accumulator);
        }

        private void STA(AddressingMode addressingMode)
        {
            var address = GetOperandAddress(addressingMode);
            WriteMemory(address, Accumulator);
        }

        private void AND(AddressingMode addressingMode)
        {
            var address = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            Accumulator &= value;

            SetNZ(Accumulator);
        }

        private void EOR(AddressingMode addressingMode)
        {
            var address = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            Accumulator ^= value;

            SetNZ(Accumulator);
        }

        private void ORA(AddressingMode addressingMode)
        {
            var address = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            Accumulator |= value;

            SetNZ(Accumulator);
        }

        private void Reset()
        {
            Accumulator = 0;
            RegisterX = 0;
            RegisterY = 0;
            Status = 0;

            ProgramCounter = ReadMemoryUshort(ResetVector);
        }

        private ushort GetOperandAddress(AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.Immediate:
                    return ProgramCounter;
                case AddressingMode.ZeroPage:
                    return ReadMemory(ProgramCounter);
                case AddressingMode.Absolute:
                    return ReadMemoryUshort(ProgramCounter);
                case AddressingMode.ZeroPageX:
                    return (ushort)(ReadMemory(ProgramCounter) + RegisterX);
                case AddressingMode.ZeroPageY:
                    return (ushort)(ReadMemory(ProgramCounter) + RegisterY);
                case AddressingMode.AbsoluteX:
                    return (ushort)(ReadMemoryUshort(ProgramCounter) + RegisterX);
                case AddressingMode.AbsoluteY:
                    return (ushort)(ReadMemoryUshort(ProgramCounter) + RegisterY);
                case AddressingMode.IndirectX:
                    return GetIndirectOperandAddress(RegisterX);
                case AddressingMode.IndirectY:
                    // Guy from the internet did it in another way. Don't know why.
                    return GetIndirectOperandAddress(RegisterY);
                case AddressingMode.NoneAddressing:
                    throw new InvalidOperationException("Incorrect Addressing Mode.");
                default:
                    throw new InvalidOperationException("Incorrect Addressing Mode.");
            }

            ushort GetIndirectOperandAddress(byte register)
            {
                var position = ReadMemory(ProgramCounter);
                var pointer = (byte)(position + register);
                var low = ReadMemory(pointer);
                var high = ReadMemory((byte)(pointer + 1));

                return (ushort)((high) << 8 | (low));
            }
        }

        private byte SetFlag(SRFlag flag) => Status |= (byte)flag;
        private byte ClearFlag(SRFlag flag) => Status = (byte)(Status & ~(byte)flag);

        private void SetNZ(byte value)
        {
            _ = value == 0 ? SetFlag(SRFlag.Zero) : ClearFlag(SRFlag.Zero);
            _ = (value & ((byte)SRFlag.Negative)) != 0 ? SetFlag(SRFlag.Negative) : ClearFlag(SRFlag.Negative);
        }

        public byte ReadMemory(ushort address) => memory[address];
        public byte WriteMemory(ushort address, byte value) => memory[address] = value;

        private ushort ReadMemoryUshort(ushort position)
        {
            var low = (ushort) ReadMemory(position);
            var high = (ushort) ReadMemory((ushort)(position + 1));

            return (ushort)((high << 8) | (low));
        }

        private void WriteMemoryUshort(ushort position, ushort data)
        {
            var high = (byte) (data >> 8);
            var low = (byte) (data & 0xFF);

            WriteMemory(position, low);
            WriteMemory((ushort)(position + 1), high);
        }
    }
}
