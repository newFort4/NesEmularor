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

        public ushort ProgramCounter { get; set; } = 0;
        public byte StackPointer { get; set; } = StackReset;

        private const ushort ResetVector = 0xFFFC;
        private const ushort NMIVector = 0xFFFA;
        private const byte StackReset = 0xFD;
        public readonly ushort? ProgramOffset = null;

        private const ushort StackOffset = 0x0100;

        public readonly Bus Bus;

        public CPU() : this("snake.nes", (p, c) => { }) { }
        public CPU(string romFile, Action<PPU, object> ppuCallback) => Bus = new Bus(new ROM(File.ReadAllBytes(romFile)), ppuCallback);
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
        }

        public void Run()
        {
            RunWithCallback((cpu) => { });
        }

        // ToDo: Implement unofficial instructions
        public void RunWithCallback(Action<CPU> action)
        {
            while (true)
            {
                if (Bus.PollNMIStatus() != null)
                {
                    InterruptNMI();
                }

                var opCode = ReadMemory(ProgramCounter);

                OpCode generalOpCode = null;

                for (int i = 0; i < OpCode.Codes.Length; i++)
                {
                    if (OpCode.Codes[i].Code == opCode)
                    {
                        generalOpCode = OpCode.Codes[i];
                        break;
                    }
                }
                //var generalOpCode = OpCode.Codes.FirstOrDefault(x => x.Code == opCode);

                action(this);

                ProgramCounter++;
                var programCounterState = ProgramCounter;

                switch (opCode)
                {
                    case 0x00:
                        return;

                    case 0xEA:
                        //var (address, pageCross) = GetOperandAddress(generalOpCode.AddressingMode);
                        //var data = ReadMemory(address);

                        //if (pageCross)
                        //{
                        //    Bus.Tick(1);
                        //}
                        break;

                    case 0XA9:
                    case 0XA5:
                    case 0XB5:
                    case 0XAD:
                    case 0XBD:
                    case 0XB9:
                    case 0XA1:
                    case 0XB1:
                        LDA(generalOpCode.AddressingMode);
                        break;
                    case 0XA2:
                    case 0XA6:
                    case 0XB6:
                    case 0XAE:
                    case 0XBE:
                        LDX(generalOpCode.AddressingMode);
                        break;
                    case 0XA0:
                    case 0XA4:
                    case 0XB4:
                    case 0XAC:
                    case 0XBC:
                        LDY(generalOpCode.AddressingMode);
                        break;

                    case 0XAA:
                        RegisterX = RegisterA;
                        SetNZ(RegisterX);
                        break;
                    case 0XA8:
                        RegisterY = RegisterA;
                        SetNZ(RegisterY);
                        break;
                    case 0XBA:
                        RegisterX = StackPointer;
                        SetNZ(RegisterX);
                        break;
                    case 0X8A:
                        RegisterA = RegisterX;
                        SetNZ(RegisterA);
                        break;
                    case 0X98:
                        RegisterA = RegisterY;
                        SetNZ(RegisterA);
                        break;
                    case 0X9A:
                        StackPointer = RegisterX;
                        break;

                    case 0X85:
                    case 0X95:
                    case 0X8D:
                    case 0X9D:
                    case 0X99:
                    case 0X81:
                    case 0X91:
                        STA(generalOpCode.AddressingMode);
                        break;
                    case 0X86:
                    case 0X96:
                    case 0X8E:
                        STX(generalOpCode.AddressingMode);
                        break;
                    case 0X84:
                    case 0X94:
                    case 0X8C:
                        STY(generalOpCode.AddressingMode);
                        break;

                    case 0X29:
                    case 0X25:
                    case 0X35:
                    case 0X2D:
                    case 0X3D:
                    case 0X39:
                    case 0X21:
                    case 0X31:
                        AND(generalOpCode.AddressingMode);
                        break;
                    case 0X49:
                    case 0X45:
                    case 0X55:
                    case 0X4D:
                    case 0X5D:
                    case 0X59:
                    case 0X41:
                    case 0X51:
                        EOR(generalOpCode.AddressingMode);
                        break;
                    case 0X09:
                    case 0X05:
                    case 0X15:
                    case 0X0D:
                    case 0X1D:
                    case 0X19:
                    case 0X01:
                    case 0X11:
                        ORA(generalOpCode.AddressingMode);
                        break;

                    case 0XE6:
                    case 0XF6:
                    case 0XEE:
                    case 0XFE:
                        INC(generalOpCode.AddressingMode);
                        break;
                    case 0XE8:
                        RegisterX++;
                        SetNZ(RegisterX);
                        break;
                    case 0XC8:
                        RegisterY++;
                        SetNZ(RegisterY);
                        break;

                    case 0XC6:
                    case 0XD6:
                    case 0XCE:
                    case 0XDE:
                        DEC(generalOpCode.AddressingMode);
                        break;
                    case 0XCA:
                        RegisterX--;
                        SetNZ(RegisterX);
                        break;
                    case 0X88:
                        RegisterY--;
                        SetNZ(RegisterY);
                        break;

                    case 0XC9:
                    case 0XC5:
                    case 0XD5:
                    case 0XCD:
                    case 0XDD:
                    case 0XD9:
                    case 0XC1:
                    case 0XD1:
                        Compare(generalOpCode.AddressingMode, RegisterA);
                        break;
                    case 0XE0:
                    case 0XE4:
                    case 0XEC:
                        Compare(generalOpCode.AddressingMode, RegisterX);
                        break;
                    case 0XC0:
                    case 0XC4:
                    case 0XCC:
                        Compare(generalOpCode.AddressingMode, RegisterY);
                        break;

                        // TODO: OPTIMALIZATION
                    case 0X4C:
                    case 0X6C:
                        JMP(isAbsolute: generalOpCode.Code == 0x4C);
                        break;
                    case 0X20:
                        StackPushUshort((ushort)(ProgramCounter + 1));
                        ProgramCounter = ReadMemoryUshort(ProgramCounter);
                        break;

                    case 0X60:
                        ProgramCounter = (ushort)(StackPopUshort() + 1);
                        break;
                    case 0X40:
                        // ToDo: Guy from the internet uses not used bit from Status Register;
                        Status = StackPop();
                        ClearFlag(SRFlag.Break);
                        ProgramCounter = StackPopUshort();
                        break;

                    case 0XD0:
                        Branch(!IsSet(SRFlag.Zero));
                        break;
                    case 0X70:
                        Branch(IsSet(SRFlag.VOverflow));
                        break;
                    case 0X50:
                        Branch(!IsSet(SRFlag.VOverflow));
                        break;
                    case 0X10:
                        Branch(!IsSet(SRFlag.Negative));
                        break;
                    case 0X30:
                        Branch(IsSet(SRFlag.Negative));
                        break;
                    case 0XF0:
                        Branch(IsSet(SRFlag.Zero));
                        break;
                    case 0XB0:
                        Branch(IsSet(SRFlag.Carry));
                        break;
                    case 0X90:
                        Branch(!IsSet(SRFlag.Carry));
                        break;

                    case 0X24:
                    case 0X2C:
                        BIT(generalOpCode.AddressingMode);
                        break;

                    case 0X48:
                        PHA();
                        break;
                    case 0X08:
                        PHP();
                        break;
                    case 0X68:
                        PLA();
                        break;
                    case 0X28:
                        PLP();
                        break;

                    case 0x69:
                    case 0x65:
                    case 0x75:
                    case 0x6D:
                    case 0x7D:
                    case 0x79:
                    case 0x61:
                    case 0x71:
                        ADC(generalOpCode.AddressingMode);
                        break;
                    case 0XE9:
                    case 0XE5:
                    case 0XF5:
                    case 0XED:
                    case 0XFD:
                    case 0XF9:
                    case 0XE1:
                    case 0XF1:
                        SBC(generalOpCode.AddressingMode);
                        break;

                    case 0X4A:
                    case 0X46:
                    case 0X56:
                    case 0X4E:
                    case 0X5E:
                        LSR(generalOpCode.AddressingMode);
                        break;
                    case 0X0A:
                    case 0X06:
                    case 0X16:
                    case 0X0E:
                    case 0X1E:
                        ASL(generalOpCode.AddressingMode);
                        break;

                    case 0X2A:
                    case 0X26:
                    case 0X36:
                    case 0X2E:
                    case 0X3E:
                        ROL(generalOpCode.AddressingMode);
                        break;
                    case 0X6A:
                    case 0X66:
                    case 0X76:
                    case 0X6E:
                    case 0X7E:
                        ROR(generalOpCode.AddressingMode);
                        break;

                    case 0XF8:
                        SetFlag(SRFlag.Decimal);
                        break;
                    case 0X38:
                        SetFlag(SRFlag.Carry);
                        break;
                    case 0X78:
                        SetFlag(SRFlag.Interrupt);
                        break;

                    case 0XD8:
                        ClearFlag(SRFlag.Decimal);
                        break;
                    case 0XB8:
                        ClearFlag(SRFlag.VOverflow);
                        break;
                    case 0X18:
                        ClearFlag(SRFlag.Carry);
                        break;
                    case 0X58:
                        ClearFlag(SRFlag.Interrupt);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown mnemonic.");
                }

                Bus.Tick(generalOpCode.Cycles);

                if (programCounterState == ProgramCounter)
                {
                    ProgramCounter = (ushort)(ProgramCounter + generalOpCode.Length - 1);
                }
            }
        }

        private void InterruptNMI()
        {
            StackPushUshort(ProgramCounter);
            var statusClone = Status;

            ClearFlag(SRFlag.Break);
            SetFlag(SRFlag.Break2);
            StackPush(Status);

            Status = statusClone;
            SetFlag(SRFlag.Interrupt);
            Bus.Tick(2); // ToDo: Correct cycles
            ProgramCounter = ReadMemoryUshort(NMIVector);
        }

        private void LDA(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterA = value;
            SetNZ(RegisterA);

            if (pageCross)
            {
                Bus.Tick(1);
            }
        }

        private void LDX(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterX = value;
            SetNZ(RegisterX);

            if (pageCross)
            {
                Bus.Tick(1);
            }
        }

        private void LDY(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            RegisterY = value;
            SetNZ(RegisterY);

            if (pageCross)
            {
                Bus.Tick(1);
            }
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
                Bus.Tick(1);
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
                Bus.Tick(1);
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
                Bus.Tick(1);
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
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var data = ReadMemory(address);

            var difference = registerData - data;

            _ = difference >= 0 ? SetFlag(SRFlag.Carry) : ClearFlag(SRFlag.Carry);

            SetNZ((byte)difference);

            if (pageCross)
            {
                Bus.Tick(1);
            }
        }

        private void Branch(bool condition)
        {
            if (condition)
            {
                Bus.Tick(1);

                var jump = (sbyte) ReadMemory(ProgramCounter);

                var jumpAddress = (ushort)(ProgramCounter + jump + 1);

                if (((ProgramCounter + 1) & 0xFF00) != (jumpAddress & 0xFF00))
                {
                    Bus.Tick(1);
                }

                ProgramCounter = jumpAddress;
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
                // ToDo: Bug with 0x6C.
                ushort indirectReference;

                if ((address & 0x00FF) == 0x00FF)
                {
                    var low = ReadMemory(address);
                    var high = ReadMemory((ushort)(address & 0xFF00));

                    indirectReference = (ushort)((high << 8) | (low));
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
                Bus.Tick(1);
            }
        }

        private void SBC(AddressingMode addressingMode)
        {
            var (address, pageCross) = GetOperandAddress(addressingMode);
            var value = ReadMemory(address);

            AddToAccumulator((byte)~value);

            if (pageCross)
            {
                Bus.Tick(1);
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

            ProgramCounter = ProgramOffset ?? ReadMemoryUshort(ResetVector);
            //ProgramCounter = ProgramOffset ?? ReadMemoryUshort(ResetVector);
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

                    return ((ushort)(((high) << 8) | (low)), false);
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
            return addressingMode switch
            {
                AddressingMode.Immediate => (ProgramCounter, false),
                _ => GetAbsoluteAddress(addressingMode),
            };
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
