using System;
using System.IO;
using System.Linq;

namespace NesEmulator.Core
{
    /// <summary>
    /// This class implements the processor 6502
    /// </summary>
    public class CPU
    {
        public byte RegisterA { get; private set; } = 0x00;
        public byte RegisterX { get; private set; } = 0x00;
        public byte RegisterY { get; private set; } = 0x00;

        public byte Status { get; set; } = 0b00000000;

        private ushort ProgramCounter { get; set; } = 0;
        private byte StackPointer { get; set; } = StackReset;

        // private const ushort ResetVector = 0xFFFC;
        private const byte StackReset = 0xFD;
        public readonly ushort ProgramOffset = 0x8600;

        private const ushort StackOffset = 0x0100;

        public readonly Bus Bus;

        public CPU() => Bus = new Bus(new ROM(File.ReadAllBytes("snake.nes")));
        public CPU(ushort programOffset) : this() => ProgramOffset = programOffset;

        public void LoadAndRun(byte[] program)
        {
            Load(program);
            Reset();
            Run();
        }

        public void Load(byte[] program)
        {
            // ToDo: Check ArraySegment<T>

            //Array.Copy(program, 0, memory, ProgramOffset, program.Length);

            for (var i = 0; i < program.Length; i++)
            {
                WriteMemory((ushort)(ProgramOffset + i), program[i]);
            }

            // WriteMemoryUshort(ResetVector, ProgramOffset);
        }

        public void Run()
        {
            RunWithCallback((cpu) => { });
        }

        public void RunWithCallback(Action<CPU> action)
        {
            var opCodes = OpCodes.Codes;

            while (true)
            {
                var opCode = ReadMemory(ProgramCounter);

                ProgramCounter++;
                var programCounterState = ProgramCounter;

                var generalOpCode = opCodes.SingleOrDefault(x => x.Code == opCode);

                switch (opCode)
                {
                    case var _ when opCodes.Where(x => x.Mnemonic == "BRK").Select(x => x.Code).Contains(opCode):
                        return;

                    case var _ when opCodes.Where(x => x.Mnemonic == "NOP").Select(x => x.Code).Contains(opCode):
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "LDA").Select(x => x.Code).Contains(opCode):
                        LDA(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "LDX").Select(x => x.Code).Contains(opCode):
                        LDX(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "LDY").Select(x => x.Code).Contains(opCode):
                        LDY(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "TAX").Select(x => x.Code).Contains(opCode):
                        RegisterX = RegisterA;
                        SetNZ(RegisterX);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "TAY").Select(x => x.Code).Contains(opCode):
                        RegisterY = RegisterA;
                        SetNZ(RegisterY);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "TSX").Select(x => x.Code).Contains(opCode):
                        RegisterX = StackPointer;
                        SetNZ(RegisterX);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "TXA").Select(x => x.Code).Contains(opCode):
                        RegisterA = RegisterX;
                        SetNZ(RegisterA);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "TYA").Select(x => x.Code).Contains(opCode):
                        RegisterA = RegisterY;
                        SetNZ(RegisterA);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "TXS").Select(x => x.Code).Contains(opCode):
                        StackPointer = RegisterX;
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "STA").Select(x => x.Code).Contains(opCode):
                        STA(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "STX").Select(x => x.Code).Contains(opCode):
                        STX(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "STY").Select(x => x.Code).Contains(opCode):
                        STY(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "AND").Select(x => x.Code).Contains(opCode):
                        AND(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "EOR").Select(x => x.Code).Contains(opCode):
                        EOR(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "ORA").Select(x => x.Code).Contains(opCode):
                        ORA(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "INC").Select(x => x.Code).Contains(opCode):
                        INC(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "INX").Select(x => x.Code).Contains(opCode):
                        RegisterX++;
                        SetNZ(RegisterX);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "INY").Select(x => x.Code).Contains(opCode):
                        RegisterY++;
                        SetNZ(RegisterY);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "DEC").Select(x => x.Code).Contains(opCode):
                        DEC(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "DEX").Select(x => x.Code).Contains(opCode):
                        RegisterX--;
                        SetNZ(RegisterX);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "DEY").Select(x => x.Code).Contains(opCode):
                        RegisterY--;
                        SetNZ(RegisterY);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "CMP").Select(x => x.Code).Contains(opCode):
                        Compare(generalOpCode.AddressingMode, RegisterA);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "CPX").Select(x => x.Code).Contains(opCode):
                        Compare(generalOpCode.AddressingMode, RegisterX);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "CPY").Select(x => x.Code).Contains(opCode):
                        Compare(generalOpCode.AddressingMode, RegisterY);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "JMP").Select(x => x.Code).Contains(opCode):
                        JMP(isAbsolute: generalOpCode.Code == 0x4C);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "JSR").Select(x => x.Code).Contains(opCode):
                        StackPushUshort((ushort)(ProgramCounter + 1));
                        ProgramCounter = ReadMemoryUshort(ProgramCounter);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "RTS").Select(x => x.Code).Contains(opCode):
                        ProgramCounter = (ushort)(StackPopUshort() + 1);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "RTI").Select(x => x.Code).Contains(opCode):
                        // ToDo: Guy from the internet uses not used bit from Status Register;
                        Status = StackPop();
                        ClearFlag(SRFlag.Break);
                        ProgramCounter = StackPopUshort();
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "BNE").Select(x => x.Code).Contains(opCode):
                        Branch(!IsSet(SRFlag.Zero));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BVS").Select(x => x.Code).Contains(opCode):
                        Branch(IsSet(SRFlag.VOverflow));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BVC").Select(x => x.Code).Contains(opCode):
                        Branch(!IsSet(SRFlag.VOverflow));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BPL").Select(x => x.Code).Contains(opCode):
                        Branch(!IsSet(SRFlag.Negative));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BMI").Select(x => x.Code).Contains(opCode):
                        Branch(IsSet(SRFlag.Negative));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BEQ").Select(x => x.Code).Contains(opCode):
                        Branch(IsSet(SRFlag.Zero));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BCS").Select(x => x.Code).Contains(opCode):
                        Branch(IsSet(SRFlag.Carry));
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "BCC").Select(x => x.Code).Contains(opCode):
                        Branch(!IsSet(SRFlag.Carry));
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "BIT").Select(x => x.Code).Contains(opCode):
                        BIT(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "PHA").Select(x => x.Code).Contains(opCode):
                        PHA();
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "PHA").Select(x => x.Code).Contains(opCode):
                        PHP();
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "PLA").Select(x => x.Code).Contains(opCode):
                        PLA();
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "PLP").Select(x => x.Code).Contains(opCode):
                        PLP();
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "ADC").Select(x => x.Code).Contains(opCode):
                        ADC(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "SBC").Select(x => x.Code).Contains(opCode):
                        SBC(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "LSR").Select(x => x.Code).Contains(opCode):
                        LSR(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "ASL").Select(x => x.Code).Contains(opCode):
                        ASL(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "ROL").Select(x => x.Code).Contains(opCode):
                        ROL(generalOpCode.AddressingMode);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "ROR").Select(x => x.Code).Contains(opCode):
                        ROR(generalOpCode.AddressingMode);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "SED").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Decimal);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "SEC").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Carry);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "SEI").Select(x => x.Code).Contains(opCode):
                        SetFlag(SRFlag.Interrupt);
                        break;

                    case var _ when opCodes.Where(x => x.Mnemonic == "CLD").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Decimal);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "CLV").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.VOverflow);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "CLC").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Carry);
                        break;
                    case var _ when opCodes.Where(x => x.Mnemonic == "CLI").Select(x => x.Code).Contains(opCode):
                        ClearFlag(SRFlag.Interrupt);
                        break;

                    case 0xFF:
                        return;
                    default:
                        throw new InvalidOperationException();
                }

                if (programCounterState == ProgramCounter)
                {
                    ProgramCounter = (ushort)(ProgramCounter + generalOpCode.Length - 1);
                }

                action(this);
            }
        }

        private void LDA(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterA = value;
            SetNZ(RegisterA);
        }

        private void LDX(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterX = value;
            SetNZ(RegisterX);
        }

        private void LDY(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterY = value;
            SetNZ(RegisterY);
        }

        private void STA(AddressingMode addressingMode) => StoreRegister(addressingMode, RegisterA);
        private void STX(AddressingMode addressingMode) => StoreRegister(addressingMode, RegisterX);
        private void STY(AddressingMode addressingMode) => StoreRegister(addressingMode, RegisterY);

        private void StoreRegister(AddressingMode addressingMode, byte register)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            WriteMemory(address, register);
        }

        private void AND(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterA &= value;

            SetNZ(RegisterA);

            if (pageCross)
            {
                // ToDo: Add bus tick
            }
        }

        private void EOR(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterA ^= value;

            SetNZ(RegisterA);

            if (pageCross)
            {
                // ToDo: Add bus tick
            }
        }

        private void ORA(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterA |= value;

            SetNZ(RegisterA);

            if (pageCross)
            {
                // ToDo: Add bus tick
            }
        }

        private void INC(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var data = ReadMemory(address);

            data++;
            WriteMemory(address, data);
            SetNZ(data);
        }

        private void DEC(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var data = ReadMemory(address);

            data--;
            WriteMemory(address, data);
            SetNZ(data);
        }

        // A.k.A. This implements CMP, CPX, CPY.
        private void Compare(AddressingMode addressingMode, byte registerData)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var data = ReadMemory(address);

            var difference = registerData - data;

            _ = difference >= 0 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);

            SetNZ((byte)difference);
        }

        private void Branch(bool condition)
        {
            if (condition)
            {
                var jump = ReadMemory(ProgramCounter);
                //if (((jump >> 7) & 0x1) == 0)
                if (jump <= 127)
                {
                    ProgramCounter = (ushort)(ProgramCounter + 1 + jump);
                } else
                {
                    // ToDo: Should 255 or 256?
                    ProgramCounter = (ushort)(ProgramCounter - 255 + jump);
                }
            }
        }

        // Could be absolute or indirect, but in spec for all is NoneAddressing
        // ToDo: Maybe spec could be edited.
        private void JMP(bool isAbsolute)
        {
            var address = ReadMemoryUshort(ProgramCounter);

            if (isAbsolute)
            {
                ProgramCounter = address;
            } else
            {
                ushort indirectReference;

                if ((address & 0x00FF) == 0x00FF)
                {
                    var low = ReadMemory(address);
                    var high = ReadMemory((ushort)(address & 0xFF00));

                    indirectReference = (ushort)((high << 8) & (low));
                } else
                {
                    indirectReference = ReadMemoryUshort(address);
                }

                ProgramCounter = indirectReference;
            }
        }

        private void BIT(AddressingMode addressingMode)
        {
            var (address, _) = GetOperandAddress(addressingMode);
            var data = ReadMemory(address);

            var and = (byte)(RegisterA & data);

            _ = and == 0 ? SetFlag(SRFlag.Zero) : ClearFlag(SRFlag.Zero);
            _ = (data & ((byte)SRFlag.Negative)) > 0 ? SetFlag(SRFlag.Negative) : ClearFlag(SRFlag.Negative);
            _ = (data & ((byte)SRFlag.VOverflow)) > 0 ? SetFlag(SRFlag.VOverflow) : ClearFlag(SRFlag.VOverflow);
        }

        private void PHA() => StackPush(RegisterA);
        private void PHP()
        {
            var statusCopy = Status;
            SetFlag(SRFlag.Break);
            ClearFlag(SRFlag.Break2);
            StackPush(Status);
            Status = statusCopy;
        }

        private void PLA() => RegisterA = StackPop();

        private void PLP()
        {
            Status = StackPop();
            ClearFlag(SRFlag.Break);
            SetFlag(SRFlag.Break2);
        }

        private void ADC(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            AddToAccumulator(value);

            if (pageCross)
            {
                // ToDo: Add bus tick
            }
        }

        private void SBC(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            AddToAccumulator((byte)~value);

            if (pageCross)
            {
                // ToDo: Add bus tick
            }
        }

        // ToDo: SetNZ is not needed here, since the first bit always be 0. Only Z flag should be checked.
        private void LSR(AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.NoneAddressing)
            {
                var data = RegisterA;
                _ = (data & 1) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data >>= 1;

                RegisterA = data;
                SetNZ(RegisterA);
            } else
            {
                var (address, _) = GetOperandAddress(addressingMode);
                var data = ReadMemory(address);

                _ = (data & 1) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data >>= 1;
                WriteMemory(address, data);
                SetNZ(data);
            }
        }

        private void ASL(AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.NoneAddressing)
            {
                var data = RegisterA;
                _ = (data >> 7) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data <<= 1;

                RegisterA = data;
                SetNZ(RegisterA);
            } else
            {
                var (address, _) = GetOperandAddress(addressingMode);
                var data = ReadMemory(address);

                _ = (data >> 7) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data <<= 1;

                WriteMemory(address, data);
                SetNZ(data);
            }
        }

        private void ROL(AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.NoneAddressing)
            {
                var data = RegisterA;
                var previousCarry = IsSet(SRFlag.Carry);

                _ = (data >> 7) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data <<= 1;
                data = (byte)(previousCarry ? (data | 0b00000001) : data);

                RegisterA = data;
                SetNZ(RegisterA);
            } else
            {
                var (address, _) = GetOperandAddress(addressingMode);
                var data = ReadMemory(address);
                var previousCarry = IsSet(SRFlag.Carry);

                _ = (data >> 7) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data <<= 1;
                data = (byte)(previousCarry ? (data | 0b00000001) : data);

                WriteMemory(address, data);
                SetNZ(data);
            }
        }

        private void ROR(AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.NoneAddressing)
            {
                var data = RegisterA;
                var previousCarry = IsSet(SRFlag.Carry);

                _ = (data & 1) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data >>= 1;
                data = (byte)(previousCarry ? (data | 0b10000000) : data);

                RegisterA = data;
                SetNZ(RegisterA);
            }
            else
            {
                var (address, _) = GetOperandAddress(addressingMode);
                var data = ReadMemory(address);
                var previousCarry = IsSet(SRFlag.Carry);

                _ = (data & 1) == 1 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);
                data >>= 1;
                data = (byte)(previousCarry ? (data | 0b10000000) : data);

                WriteMemory(address, data);
                SetNZ(data);
            }
        }

        private void AddToAccumulator(byte data)
        {
            var sum = RegisterA + data + (IsSet(SRFlag.Carry) ? 1 : 0);

            _ = sum > 0xFF ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);

            var result = (byte)sum;

            _ = ((data ^ sum) & (result ^ RegisterA) & 0x80) != 0 ? SetFlag(SRFlag.VOverflow) : ClearFlag(SRFlag.VOverflow);

            RegisterA = result;
            SetNZ(RegisterA);
        }

        private void StackPush(byte data)
        {
            WriteMemory((ushort)(StackOffset + StackPointer), data);
            StackPointer--;
        }

        private byte StackPop()
        {
            StackPointer++;

            var value = ReadMemory((ushort)(StackOffset + StackPointer));
            SetNZ(value);

            return value;
        }

        private void StackPushUshort(ushort data)
        {
            StackPush((byte)(data >> 8));
            StackPush((byte)(data & 0xFF));
        }

        private ushort StackPopUshort()
        {
            var low = StackPop();
            var high = StackPop();

            return (ushort)((high << 8) | low);
        }

        public void Reset()
        {
            RegisterA = 0;
            RegisterX = 0;
            RegisterY = 0;

            // ToDo: Should read ResetVector, but with the current implementation of bus, this is not possible.
            ProgramCounter = ProgramOffset;//ReadMemoryUshort(ResetVector);
            StackPointer = StackReset;
            Status = 0b00100100;
        }

        private (ushort, bool) GetAbsoluteAddress(AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.ZeroPage:
                    return (ReadMemory(ProgramCounter), false);
                case AddressingMode.Absolute:
                    return (ReadMemoryUshort(ProgramCounter), false);
                case AddressingMode.ZeroPageX:
                    return ((ushort)(ReadMemory(ProgramCounter) + RegisterX), false);
                case AddressingMode.ZeroPageY:
                    return ((ushort)(ReadMemory(ProgramCounter) + RegisterY), false);
                case AddressingMode.AbsoluteX:
                    var @base = ReadMemoryUshort(ProgramCounter);
                    var address = (ushort)(@base + RegisterX);
                    return (address, PageCross(@base, address));
                case AddressingMode.AbsoluteY:
                    @base = ReadMemoryUshort(ProgramCounter);
                    address = (ushort)(@base + RegisterY);
                    return (address, PageCross(@base, address));
                case AddressingMode.IndirectX:
                    var position = ReadMemory(ProgramCounter);
                    var pointer = (byte)(position + RegisterX);
                    var low = ReadMemory(pointer);
                    var high = ReadMemory((byte)(pointer + 1));

                    return ((ushort)((high) << 8 | (low)), false);
                case AddressingMode.IndirectY:
                    position = ReadMemory(ProgramCounter);
                    low = ReadMemory(position);
                    high = ReadMemory((byte)(position + 1));

                    var derefBase = (ushort)(high << 8 | low);
                    var deref = (ushort)(derefBase + RegisterY);

                    return (deref, PageCross(deref, derefBase));
                case AddressingMode.NoneAddressing:
                    throw new InvalidOperationException("Incorrect Addressing Mode.");
                default:
                    throw new InvalidOperationException("Incorrect Addressing Mode.");
            }
        }

        private bool PageCross(ushort address1, ushort address2)
        {
            return (address1 & 0xFF00) != (address2 & 0xFF00);
        }

        private (ushort, bool) GetOperandAddress(AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.Immediate:
                    return (ProgramCounter, false);
                default:
                    return GetAbsoluteAddress(addressingMode);
            }
        }

        private byte SetFlag(SRFlag flag) => Status |= (byte)flag;
        private byte ClearFlag(SRFlag flag) => Status = (byte)(Status & ~(byte)flag);
        private bool IsSet(SRFlag flag) => (Status & ((byte)flag)) != 0;

        private void SetNZ(byte value)
        {
            _ = value == 0 ? SetFlag(SRFlag.Zero) : ClearFlag(SRFlag.Zero);
            _ = value > 127 ? SetFlag(SRFlag.Negative) : ClearFlag(SRFlag.Negative);
        }

        public byte ReadMemory(ushort address) => Bus.ReadMemory(address);
        public void WriteMemory(ushort address, byte data) => Bus.WriteMemory(address, data);

        private ushort ReadMemoryUshort(ushort position) => Bus.ReadMemoryUshort(position);
        private void WriteMemoryUshort(ushort position, ushort data) => Bus.WriteMemoryUshort(position, data);
    }
}
