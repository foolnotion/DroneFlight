using System;
using System.Collections.Generic;
using System.IO;
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
      File.WriteAllText(@"C:\\Users\\P40913\\Projects\\TechOn2015\\DroneFlightPath\\DroneFlightPath\\TestFile\\07_intoTheDark.txt", sb.ToString());
      var rm = new RegisterMachine();
      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      for (int i = 0; i < 30; ++i) {
        Console.WriteLine($"Mem[{i}]: {rm.Memory[i]}");
      }
    }
  }
}
