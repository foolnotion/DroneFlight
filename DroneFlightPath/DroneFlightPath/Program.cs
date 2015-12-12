using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeInterpreter;

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
      var nl = Environment.NewLine;
      var srcPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestFile\05_thinkAhead_Generated.txt"));
      //      var code = RegisterMachineUtil.LoadPath(srcPath).ToArray();
      var moveInstructions = new[] {
        new MapUtil.MoveInstr(Direction.Right, 4),
        new MapUtil.MoveInstr(Direction.Up, 6),
        new MapUtil.MoveInstr(Direction.Left, 4),
        new MapUtil.MoveInstr(Direction.Up, 1),
        new MapUtil.MoveInstr(Direction.Hold, 7),
        new MapUtil.MoveInstr(Direction.Up, 5),
        new MapUtil.MoveInstr(Direction.Left, 1),
        new MapUtil.MoveInstr(Direction.Up, 9),
        new MapUtil.MoveInstr(Direction.Right, 2),
      };
      var steps = moveInstructions.Sum(x => x.S);
      var code = MapUtil.GenerateMachineCode(moveInstructions).ToArray();
      var sb = new StringBuilder();
      foreach (var instr in code) {
        sb.AppendLine(instr.ToString());
      }
      File.WriteAllText(srcPath, sb.ToString());

      var sm = new RegisterMachine();
      sm.LoadIntructions(code);

      const double mapWeight = 0.05;
      for (int i = 0; i < steps; ++i) {
        try {
          sm.Run();
          Console.WriteLine("A: {0}, N: {1}, M[0]: {2}, M[1000]: {3}, M[1001]: {4}",
            sm.A, sm.N, int2Dir[sm.Memory[0]], sm.Memory[1000], sm.Memory[1001]);
        }
        catch (Exception e) {
          Console.WriteLine($"Error running code: {e.Message}");
        }
      }
      Console.WriteLine("Cpu cycles: {0}", sm.Cycles);

      var score = mapWeight * 1e6 / Math.Log(steps * steps * sm.Cycles);

      Console.WriteLine("Score: {0:0.00}", score);

      //      Console.Read();
    }
  }
}
