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
      Func<int, AstNode> _ = AstNode.Constant;
      Func<int, AstNode> mem = AstNode.Pointer;

      var sb = new StringBuilder();

      var arr = (VariableAstNode)AstNode.Variable("arr", 10);

      var right = _(2);
      var left = _(4);
      var down = _(3);
      var up = _(1);
      var hold = _(0);

      var cx = AstNode.Variable("cx");
      var cy = AstNode.Variable("cy");
      var tx = AstNode.Variable("tx");
      var ty = AstNode.Variable("ty");
      var drone = mem(0);
      var ret = new AstReturnNode();
      var block = AstNode.Block(
        AstNode.Assign(cx, mem(3)),
        AstNode.Assign(cy, mem(4)),
        AstNode.Assign(tx, mem(5)),
        AstNode.Assign(ty, mem(6)),
        AstNode.IfThen(
          cy < ty,
          AstNode.Block(AstNode.Assign(drone, down), ret)
        ),
        AstNode.IfThen(
          cy > ty,
          AstNode.Block(AstNode.Assign(drone, up), ret)
        ),
        AstNode.IfThen(
          cx > tx,
          AstNode.Block(AstNode.Assign(drone, left), ret)
        ),
        AstNode.IfThen(
          cx < tx,
          AstNode.Block(AstNode.Assign(drone, right), ret)
        ),
        ret
      );

      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);
      //      genVisitor.Code.Add(Instruction.Hlt());

      sb.Clear();
      for (int i = 0; i < genVisitor.Code.Count; ++i) {
        sb.AppendLine($"{genVisitor.Code[i]}");
      }
      File.WriteAllText(@"C:\\Users\\Bogdan\\Projects\\TechOn2015\\DroneFlightPath\\DroneFlightPath\\TestFile\\07_intoTheDark.txt", sb.ToString());
      //      Console.WriteLine("Generated ASM:");
      //      Console.WriteLine(sb);
      //
      //      foreach (var p in genVisitor.NodeNames) {
      //        Console.WriteLine($"{genVisitor.JumpLocations[p.Key]} {p.Value}");
      //      }

      var rm = new RegisterMachine();
      rm.LoadIntructions(genVisitor.Code);
      rm.Run();
      //
      //      var addr = genVisitor.MemoryMap["arr"];
      //      Console.WriteLine("Arr addr: " + addr);
      //      Console.WriteLine("A addr: " + genVisitor.MemoryMap["a"]);
      //      for (int i = addr; i < addr + arr.Size; ++i)
      //        Console.WriteLine($"{arr.VariableName}[{i - addr}]: {rm.Memory[i]}");
      //
      //      var resultAddr = genVisitor.MemoryMap["result"];
      //      Console.WriteLine($"Result addr: {resultAddr}");
      //      Console.WriteLine($"Result: {rm.Memory[resultAddr]}");
      //      Console.Read();
    }
  }
}
