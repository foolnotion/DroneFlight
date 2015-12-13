﻿using System;
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
      var aa = AstNode.Assign(a, AstNode.Constant(1));
      var ab = AstNode.Assign(b, AstNode.Constant(2));
      var result = AstNode.Variable("result");
      var startNode = new AstStartNode(aa, ab, AstNode.IfThen(a > b, AstNode.Assign(result, AstNode.Constant(10)))
      //     AstNode.IfThen(a < b, AstNode.Assign(result, AstNode.Constant(100)))
      );

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

      rm.LoadIntructions(genVisitor.Code);
      rm.Run();

      var resultAddr = genVisitor.MemoryMap["result"];
      Console.WriteLine($"Result addr: {resultAddr}");
      Console.WriteLine($"Result: {rm.Memory[resultAddr]}");
      //      Console.Read();
    }
  }
}
