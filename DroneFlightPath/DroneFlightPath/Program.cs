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
      Test.TestCit();
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
      File.WriteAllText("02_dontGetShot.txt", sb.ToString());
      File.WriteAllText("03_shortestPath.txt", sb.ToString());
      File.WriteAllText("04_gottaCircleAround.txt", sb.ToString());
      File.WriteAllText("05_thinkAhead.txt", sb.ToString());
      File.WriteAllText("06_beOnYourToes.txt", sb.ToString());
      File.WriteAllText("07_intoTheDark.txt", sb.ToString());
      File.WriteAllText("08_mazeOfDrones.txt", sb.ToString());
      File.WriteAllText("09_theyJustKeepOnComing.txt", sb.ToString());
      File.WriteAllText("10_labyrinth.txt", sb.ToString());
      File.WriteAllText("11_whatsTheName.txt", sb.ToString());
      File.WriteAllText("12_noWayToTarget.txt", sb.ToString());

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
      rm.Run();
      for (int row = 0; row < 10; row++) {
        for (int col = 0; col < 10; col++) {
          System.Console.Write(rm.Memory[genVisitor.MemoryMap["currentMap"] + col * 10 + row] + "\t");
        }
        System.Console.WriteLine();
      }

      //      rm.Memory[1] = 3; // 3 rows
      //      rm.Memory[2] = 3; // 3 columns
      //      rm.Memory[3] = 0; // dx = 0
      //      rm.Memory[4] = 0; // dy = 0
      //      rm.Memory[5] = 2; // tx = 0
      //      rm.Memory[6] = 2; // ty = 0
      //      rm.Memory[7] = 1; // 1 obstacle
      //      rm.Memory[8] = 1;
      //      rm.Memory[9] = 0;
      //
      //      rm.LoadIntructions(genVisitor.Code);
      //      int step = 0;
      //      while (true) {
      //        rm.Run();
      //        ++step;
      //        if (step > 10)
      //          break;
      //      }
      //      Console.WriteLine($"Test result: {rm.Memory[0]}");
      //      Console.Read();
    }
  }
}
