using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeInterpreter;
using CodeInterpreter.AST;

namespace DroneFlightPath {
  public class BasicWaveFrontAi {
    public void GenerateCode() {
      var sb = new StringBuilder();
      var rm = new RegisterMachine();

      var mapRows = AstNode.Variable("mapRows");
      var mapColumns = AstNode.Variable("mapColumns");
      var droneX = AstNode.Variable("droneX");
      var droneY = AstNode.Variable("droneY");
      var targetX = AstNode.Variable("targetX");
      var targetY = AstNode.Variable("targetY");
      var numberOfObstacles = AstNode.Variable("numberOfObstacles");
      var numberOfPeople = AstNode.Variable("numberOfPeople");
      var numberOfDrones = AstNode.Variable("numberOfDrones");


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
