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
      var block = Strategy.Manhattan();
      //      var block = Strategy.Test();
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);

      var sb = new StringBuilder();
      foreach (var c in genVisitor.Code) {
        sb.AppendLine(c.ToString());
      }
      File.WriteAllText("07_intoTheDark.txt", sb.ToString());

      var rm = new RegisterMachine();
      rm.Memory[1] = 3; // 3 rows
      rm.Memory[2] = 3; // 3 columns
      rm.Memory[3] = 0; // dx = 0
      rm.Memory[4] = 0; // dy = 0
      rm.Memory[5] = 2; // tx = 0
      rm.Memory[6] = 2; // ty = 0
      rm.Memory[7] = 1; // 1 obstacle
      rm.Memory[8] = 1;
      rm.Memory[9] = 0;

      rm.LoadIntructions(genVisitor.Code);
      int step = 0;
      while (true) {
        rm.Run();
        ++step;
        if (step > 10)
          break;
      }
      //      Console.WriteLine($"Test result: {rm.Memory[0]}");
      //      Console.Read();
    }
  }
}
