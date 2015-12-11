using System;
using System.IO;
using System.Linq;
using CodeInterpreter;

namespace DroneFlightPath {
  class Program {
    static void Main(string[] args) {
      var srcPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\TestFile\01_letsGetToKnowEachOther_s3.txt"));
      var code = RegisterMachineUtil.LoadSource(srcPath).ToArray();
      var sm = new RegisterMachine(1001);

      const int steps = 12;
      const double mapWeight = 0.01;
      for (int i = 0; i < steps; ++i) {
        sm.Execute(code);
        Console.WriteLine("A: {0}, N: {1}, M[0]: {2}, M[1000]: {3}", sm.A, sm.N, sm.Memory[0], sm.Memory[1000]);
      }
      Console.WriteLine("Cpu cycles: {0}", sm.Cycles);

      var score = mapWeight * 1e6 / Math.Log(steps * steps * sm.Cycles);
      Console.WriteLine("Score: {0:0.00}", score);

      Console.Read();
    }
  }
}
