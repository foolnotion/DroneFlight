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

      var rm = new RegisterMachine();
      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      Console.WriteLine($"Test result: {rm.Memory[0]}");

      int n = 0;
      int i = 0, j = 0, k = 0;
      while (i < 3) {
        while (j < 3) {
          while (k < 3) {
            n++;
            ++k;
          }
          ++j;
        }
        ++i;
      }
      Console.WriteLine($"While: {n}");
      i = j = k = n = 0;
      do {
        do {
          do {
            ++n;
            ++k;
          } while (k < 3);
          ++j;
        } while (j < 3);
        ++i;
      } while (i < 3);
      Console.WriteLine($"Do...While: {n}");
    }
  }
}
