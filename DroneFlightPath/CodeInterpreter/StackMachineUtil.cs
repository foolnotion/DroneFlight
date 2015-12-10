using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeInterpreter {
  public static class StackMachineUtil {
    private static Dictionary<string, OpCode> instructionOpCodes = new Dictionary<string, OpCode> {
      { "STA", OpCode.Sta},
      { "LDA", OpCode.Lda},
      { "LDN", OpCode.Ldn},
      { "ADDA", OpCode.Adda},
      { "SUBA", OpCode.Suba},
      { "JGE", OpCode.Jge},
      { "HLT", OpCode.Hlt},
    };

    public static IEnumerable<Instruction> LoadSource(string path) {
      var lines = File.ReadAllLines(path).Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("//")).ToList();
      foreach (var line in lines) {
        var tokens = line.Split();
        var instrToken = tokens[0];
        var opCode = instructionOpCodes[instrToken];
        if (opCode != OpCode.Hlt && tokens.Length < 2)
          continue; // garbage. maybe throw exception
        var arg = opCode == OpCode.Hlt ? new Arg(ArgType.Value, 0) : ParseArg(tokens[1]);
        yield return new Instruction(opCode, arg);
      }
    }

    public static Arg ParseArg(string arg) {
      if (arg.StartsWith("[")) {
        var innerArg = arg.Replace("[", "").Replace("]", "");
        if (innerArg == "A")
          return new Arg(ArgType.RefA, 0);
        if (innerArg == "N")
          return new Arg(ArgType.RefN, 0);
        int addr;
        if (int.TryParse(innerArg, out addr)) {
          return new Arg(ArgType.RefMem, addr);
        }
      } else {
        int value;
        if (int.TryParse(arg, out value)) {
          return new Arg(ArgType.Value, value);
        }
      }
      throw new Exception(string.Format("Could not parse arg \"{0}\"", arg));
    }
  }
}
