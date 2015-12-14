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
      var rm = new RegisterMachine();

      var a = AstNode.Variable("a");
      var b = AstNode.Variable("b");
      var aa = AstNode.Assign(a, a + AstNode.Constant(5));
      var ab = AstNode.Assign(a, a + AstNode.Constant(3));
      var array = AstNode.Array("v", 30);
      var result = AstNode.Variable("result");

      var zero = AstNode.Constant(0);
      var one = AstNode.Constant(1);
      var two = AstNode.Constant(2);

      var startNode = new AstBlockNode(
          //                          AstNode.Assign(a, AstNode.Constant(2)),
          //                                    AstNode.Assign(b, AstNode.Constant(10)),
          AstNode.While(
            a < AstNode.Constant(10),
            AstNode.IfThenElse(
              a % two | zero,
              //              AstNode.Assign(a, a + AstNode.Constant(10)),
              aa,
              ab)
            ),
          AstNode.Assign(result, a)
        );
      //      var startNode = new AstBlockNode(AstNode.While(one < two, AstNode.Assign(a, a + AstNode.Constant(1))));

      var mmapVisitor = new MapObjectsToMemoryVisitor();
      startNode.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      startNode.Accept(genVisitor);

      sb.Clear();
      for (int i = 0; i < genVisitor.Code.Count; ++i) {
        sb.AppendLine($"{i}: {genVisitor.Code[i]}");
      }
      Console.WriteLine("Generated ASM:");
      Console.WriteLine(sb);

      foreach (var o in genVisitor.MemoryMap.Objects) {
        if (genVisitor.NodeNames.ContainsKey(o.Key))
          Console.WriteLine("{00} {1}", genVisitor.JumpLocations[o.Key], genVisitor.NodeNames[o.Key]);
      }

      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      var resultAddr = genVisitor.MemoryMap["result"];
      Console.WriteLine($"Result addr: {resultAddr}");
      Console.WriteLine($"Result: {rm.Memory[resultAddr]}");
      Console.Read();
    }
  }
}
