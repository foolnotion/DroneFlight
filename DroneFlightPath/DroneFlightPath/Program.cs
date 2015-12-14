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
      Func<int, AstNode> _ = AstNode.Constant;

      var sb = new StringBuilder();

      var a = AstNode.Variable("a");
      var result = AstNode.Variable("result");
      var arr = (VariableAstNode)AstNode.Variable("arr", 10);
      var block = AstNode.Block(
        AstNode.DoWhile(
          a < AstNode.Constant(arr.Size),
          AstNode.Block(
            AstNode.ArraySet(arr, a, a),
            AstNode.Increment(a)
          )
        ),
        AstNode.Assign(result, a + AstNode.ArrayGet(arr, a - _(1)))
      );
      //      var startNode = new AstBlockNode(AstNode.While(one < two, AstNode.Assign(a, a + AstNode.Constant(1))));
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);

      sb.Clear();
      for (int i = 0; i < genVisitor.Code.Count; ++i) {
        sb.AppendLine($"{i}: {genVisitor.Code[i]}");
      }
      Console.WriteLine("Generated ASM:");
      Console.WriteLine(sb);

      var rm = new RegisterMachine();
      genVisitor.Code.Add(Instruction.Hlt());
      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      var addr = genVisitor.MemoryMap["arr"];
      Console.WriteLine("Arr addr: " + addr);
      Console.WriteLine("A addr: " + genVisitor.MemoryMap["a"]);
      for (int i = addr; i < addr + arr.Size; ++i)
        Console.WriteLine($"{arr.VariableName}[{i - addr}]: {rm.Memory[i]}");

      var resultAddr = genVisitor.MemoryMap["result"];
      Console.WriteLine($"Result addr: {resultAddr}");
      Console.WriteLine($"Result: {rm.Memory[resultAddr]}");
      Console.Read();
    }
  }
}
