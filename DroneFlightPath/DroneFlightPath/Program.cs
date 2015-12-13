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
      //      var binaryOp = new BinaryOperationAstNode(
      //        AstBinaryOp.Add,
      //        new BinaryOperationAstNode(AstBinaryOp.Mul, new ConstantAstNode(2), new VariableAstNode("a", 3)),
      //        new ConstantAstNode(4));
      var binaryOp = new BinaryOperationAstNode(AstBinaryOp.Mul, new ConstantAstNode(3), new VariableAstNode("a", 5));
      var startNode = new AstStartNode { Child = binaryOp };
      var mmapVisitor = new MapSymbolsToMemoryVisitor();
      startNode.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemMap);
      startNode.Accept(genVisitor);

      sb.Clear();
      for (int i = 0; i < genVisitor.Code.Count; ++i) {
        sb.AppendLine($"{i}: {genVisitor.Code[i]}");
      }
      Console.WriteLine("Generated ASM:");
      Console.WriteLine(sb);

      rm.LoadIntructions(genVisitor.Code.ToArray());
      rm.Run();
      var resultAddr = genVisitor.IntermediateResults[binaryOp.Id];
      Console.WriteLine($"Result addr: {resultAddr}");
      Console.WriteLine($"Result: {rm.Memory[resultAddr]}");
      //      Console.Read();
    }
  }
}
