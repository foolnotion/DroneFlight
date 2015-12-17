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
      var block = Strategy.NaiveGradientDescent();
      //      var block = Strategy.Test();
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);

      var sb = new StringBuilder();
      foreach (var c in genVisitor.Code) {
        sb.AppendLine(c.ToString());
      }
      File.WriteAllText("01_letsGetToKnowEachOther.txt", sb.ToString());

      var rm = new RegisterMachine();
      rm.Memory[1] = 10; // rows
      rm.Memory[2] = 10; // columns
      rm.Memory[3] = 1; // dx = 0
      rm.Memory[4] = 0; // dy = 0
      rm.Memory[5] = 5; // tx
      rm.Memory[6] = 7; // ty
      rm.Memory[7] = 3; 
      rm.Memory[8] = 0;
      rm.Memory[9] = 2;
      rm.Memory[10] = 1;
      rm.Memory[11] = 2;
      rm.Memory[12] = 2;
      rm.Memory[13] = 2;

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
