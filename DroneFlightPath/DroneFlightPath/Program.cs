using System;
using System.Collections.Generic;
using System.Text;
using CodeInterpreter;
using CodeInterpreter.AST;

namespace DroneFlightPath {
  class Program {
    private static readonly Dictionary<int, string> int2Dir = new Dictionary<int, string> {
      {0, "HOLD"},
      {1, "UP"},
      {2, "RIGHT"},
      {3, "DOWN"},
      {4, "LEFT"}
    };

    static void Main(string[] args) {
      var sb = new StringBuilder();
      var block = Strategy.Test();
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);
      sb.Clear();
      for (int i = 0; i < genVisitor.Code.Count; ++i) {
        sb.AppendLine($"{genVisitor.Code[i]}");
      }

      //      Console.WriteLine($"i: {genVisitor.MemoryMap["i"]}");
      //      Console.WriteLine($"j: {genVisitor.MemoryMap["j"]}");
      //      File.WriteAllText(@"C:\\Users\\Bogdan\\Projects\\TechOn2015\\DroneFlightPath\\DroneFlightPath\\TestFile\\07_intoTheDark.txt", sb.ToString());
      var rm = new RegisterMachine();
      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      Console.WriteLine($"3002: {rm.Memory[3002]}");
      Console.WriteLine($"3003: {rm.Memory[3003]}");
      Console.WriteLine($"3004: {rm.Memory[3004]}");

      for (int i = 0; i < 7; ++i) {
        Console.WriteLine($"Mem[{i}]: {rm.Memory[i]}");
      }
    }
  }
}
