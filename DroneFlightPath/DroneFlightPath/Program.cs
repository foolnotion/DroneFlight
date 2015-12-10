using System;
using System.Linq;
using CodeInterpreter;

namespace DroneFlightPath {
  class Program {
    static void Main(string[] args) {
      //      var interpreter = new Interpreter(@"G:\work\TechOn2015\DroneFlightPath\DroneFlightPath\TestFile\01_letsGetToKnowEachOther_s3.txt", 100);
      //
      //      interpreter.Read();      
      var code = StackMachineUtil.LoadSource(@"C:\Users\Bogdan\Projects\TechOn2015\DroneFlightPath\DroneFlightPath\TestFile\01_letsGetToKnowEachOther_s3.txt").ToArray();
      var sm = new StackMachine(1001);
      var result = sm.Execute(code);

      Console.WriteLine("Result: {0}", result);
    }
  }
}
