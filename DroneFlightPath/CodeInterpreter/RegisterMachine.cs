using System;
using System.Collections.Generic;
using System.Text;
using CodeInterpreter.AST;

namespace CodeInterpreter {
  public enum OpCode : byte { Sta, Lda, Ldn, Adda, Suba, Jge, Hlt }
  public enum ArgType : byte { RefA, RefN, Value }

  public class Arg {
    public ArgType Type;
    public int Value;
    public bool Indirect; // specifies if the arg value represents a memory address (indirection)

    private Arg() { }

    public Arg(ArgType type, int value, bool indirect) {
      Type = type;
      Value = value;
      Indirect = indirect;
    }

    public override string ToString() {
      string label;
      switch (Type) {
        case ArgType.RefA:
          label = "A";
          break;
        case ArgType.RefN:
          label = "N";
          break;
        case ArgType.Value:
          label = Value.ToString();
          break;
        default:
          throw new ArgumentException("Unknown arg type");
      }
      return Indirect ? $"[{label}]" : label;
    }

    public static Arg Val(int value) {
      return new Arg(ArgType.Value, value, false);
    }
    public static Arg Mem(int addr) {
      return new Arg(ArgType.Value, addr, true);
    }
    public static Arg A(bool indirect) {
      return new Arg(ArgType.RefA, 0, indirect);
    }
    public static Arg N(bool indirect) {
      return new Arg(ArgType.RefN, 0, indirect);
    }
  }

  public class Instruction {
    public OpCode OpCode { get; }
    public Arg Arg { get; }

    private Instruction() { }

    public Instruction(OpCode opCode, Arg arg) {
      OpCode = opCode;
      Arg = arg;
    }

    public static Instruction Lda(Arg arg) {
      return new Instruction(OpCode.Lda, arg);
    }
    public static Instruction Ldn(Arg arg) {
      return new Instruction(OpCode.Ldn, arg);
    }
    public static Instruction Sta(Arg arg) {
      if (!arg.Indirect)
        throw new ArgumentException("The STA instruction accepts a memory address");
      return new Instruction(OpCode.Sta, arg);
    }
    public static Instruction Adda(Arg arg) {
      return new Instruction(OpCode.Adda, arg);
    }
    public static Instruction Suba(Arg arg) {
      return new Instruction(OpCode.Suba, arg);
    }
    public static Instruction Jge(Arg arg) {
      return new Instruction(OpCode.Jge, arg);
    }
    public static Instruction Hlt() {
      return new Instruction(OpCode.Hlt, new Arg(ArgType.Value, 0, false));
    }
    public override string ToString() {
      switch (OpCode) {
        case OpCode.Lda: {
            return string.Format("LDA {0}", Arg);
          }
        case OpCode.Sta: {
            return string.Format("STA {0}", Arg);
          }
        case OpCode.Ldn: {
            return string.Format("LDN {0}", Arg);
          }
        case OpCode.Adda: {
            return string.Format("ADDA {0}", Arg);
          }
        case OpCode.Suba: {
            return string.Format("SUBA {0}", Arg);
          }
        case OpCode.Jge: {
            return string.Format("JGE {0}", Arg);
          }
        case OpCode.Hlt: {
            return "HLT";
          }
        default:
          return "Unknown";
      }
    }
  }

  public class RegisterMachineState {
    private RegisterMachineState() { }
    private readonly IList<Instruction> code;

    public RegisterMachineState(IList<Instruction> instructions) {
      InstructionPointer = 0;
      this.code = instructions;
    }

    public int InstructionPointer { get; set; }

    public void Reset() {
      InstructionPointer = 0;
    }

    public Instruction NextInstruction() {
      return code[InstructionPointer++];
    }

    public void Jump(int ip) {
      InstructionPointer = ip;
    }

    public string PrintInstructions() {
      var sb = new StringBuilder();
      foreach (var instr in code) {
        sb.AppendLine(instr.ToString());
      }
      return sb.ToString();
    }
  }

  public class RegisterMachine {
    public int[] Memory { get; set; }
    // registers
    public int A;
    public int N;

    public MemoryMap MemoryMap { get; set; }
    private RegisterMachineState state;

    public int Cycles { get; private set; }

    public RegisterMachine() {
      Memory = new int[1000000];
    }

    public RegisterMachine(uint memoryCapacity) {
      Memory = new int[memoryCapacity];
    }

    public void LoadIntructions(IList<Instruction> instructions) {
      state = new RegisterMachineState(instructions);
    }

    public void Step() {
      if (state == null)
        throw new InvalidOperationException(
          $"State is null. Please use the LoadInstructions method to load some instructions first.");

      ExecuteInstruction(state.NextInstruction());
    }

    public void ExecuteInstruction(Instruction instr) {
      switch (instr.OpCode) {
        case OpCode.Sta: {
            var arg = instr.Arg;
            if (!arg.Indirect)
              throw new ArgumentException("Argument must be indirect");
            switch (arg.Type) {
              case ArgType.Value: {
                  Memory[arg.Value] = A;
                  break;
                }
              case ArgType.RefN: {
                  Memory[N] = A;
                  break;
                }
              case ArgType.RefA: {
                  Memory[A] = A;
                  break;
                }
            }
          }
          break;
        case OpCode.Lda: {
            A = EvaluateArg(instr.Arg);
            break;
          }
        case OpCode.Ldn: {
            N = EvaluateArg(instr.Arg);
            break;
          }
        case OpCode.Adda: {
            A += EvaluateArg(instr.Arg);
            break;
          }
        case OpCode.Suba: {
            A -= EvaluateArg(instr.Arg);
            break;
          }
        case OpCode.Jge: {
            if (A >= 0)
              state.Jump(EvaluateArg(instr.Arg));
            break;
          }
        case OpCode.Hlt: {
            break;
          }
        default:
          throw new ArgumentException(string.Format("Invalid OpCode {0}", instr.OpCode));
      }
    }

    public int Run(bool reset = false) {
      if (state == null)
        throw new InvalidOperationException(
          $"State is null. Please use the LoadInstructions method to load some instructions first.");
      state.Reset(); // rewind the instruction pointer

      if (reset) {
        A = 0;
        N = 0;
        Memory = new int[Memory.Length];
        Cycles = 0;
      }
      Instruction instr;
      do {
        Cycles++;
        instr = state.NextInstruction();
        ExecuteInstruction(instr);
      } while (instr.OpCode != OpCode.Hlt);
      return A;
    }

    private int EvaluateArg(Arg arg) {
      switch (arg.Type) {
        case ArgType.RefA:
          return arg.Indirect ? Memory[A] : A;
        case ArgType.RefN:
          return arg.Indirect ? Memory[N] : N;
        case ArgType.Value:
          return arg.Indirect ? Memory[arg.Value] : arg.Value;
        default:
          throw new Exception(string.Format("Unknown arg type {0}", arg.Type));
      }
    }
  }
}
