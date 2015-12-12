using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeInterpreter {
  public static class RegisterMachineUtil {
    private static readonly Dictionary<string, OpCode> InstructionOpCodes = new Dictionary<string, OpCode> {
      { "STA", OpCode.Sta},
      { "LDA", OpCode.Lda},
      { "LDN", OpCode.Ldn},
      { "ADDA", OpCode.Adda},
      { "SUBA", OpCode.Suba},
      { "JGE", OpCode.Jge},
      { "HLT", OpCode.Hlt},
    };

    public static IEnumerable<Instruction> LoadSource(string source) {
      foreach (var line in source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
        var tokens = line.ToUpper().Split();
        var instrToken = tokens[0];
        var opCode = InstructionOpCodes[instrToken];
        if (opCode != OpCode.Hlt && tokens.Length < 2)
          continue; // garbage. maybe throw exception
        var arg = opCode == OpCode.Hlt ? new Arg(ArgType.Value, 0, false) : ParseArg(tokens[1]);
        yield return new Instruction(opCode, arg);
      }
    }

    public static IEnumerable<Instruction> LoadPath(string path) {
      var lines = File.ReadAllLines(path).Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("//")).ToList();
      foreach (var line in lines) {
        var tokens = line.ToUpper().Split();
        var instrToken = tokens[0];
        var opCode = InstructionOpCodes[instrToken];
        if (opCode != OpCode.Hlt && tokens.Length < 2)
          continue; // garbage. maybe throw exception
        var arg = opCode == OpCode.Hlt ? new Arg(ArgType.Value, 0, false) : ParseArg(tokens[1]);
        yield return new Instruction(opCode, arg);
      }
    }

    public static Arg ParseArg(string arg) {
      string innerArg;
      bool indirect;
      if (arg.StartsWith("[")) {
        innerArg = arg.Replace("[", "").Replace("]", "");
        indirect = true;
      } else {
        innerArg = arg;
        indirect = false;
      }
      if (innerArg == "A")
        return new Arg(ArgType.RefA, 0, indirect);
      if (innerArg == "N")
        return new Arg(ArgType.RefN, 0, indirect);
      int addr;
      if (int.TryParse(innerArg, out addr))
        return new Arg(ArgType.Value, addr, indirect);
      throw new Exception(string.Format("Could not parse arg \"{0}\"", arg));
    }
  }
}
