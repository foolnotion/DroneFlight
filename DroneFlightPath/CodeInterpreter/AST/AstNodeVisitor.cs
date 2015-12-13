using System;
using System.Collections.Generic;

namespace CodeInterpreter.AST {
  public abstract class AstNodeVisitor {
    public abstract void Visit(AstNode node);
  }

  public class MapSymbolsToMemoryVisitor : AstNodeVisitor {
    private uint memStart = 1000;
    private uint memEnd = 2000;
    private uint memPointer;
    public Dictionary<string, uint> MemMap { get; }

    public MapSymbolsToMemoryVisitor() {
      MemMap = new Dictionary<string, uint>();
      memPointer = memStart;
    }

    public MapSymbolsToMemoryVisitor(uint memStart, uint memEnd) : this() {
      if (!(memStart > memEnd))
        throw new ArgumentException($"Invalid memory limits.");
      this.memStart = memStart;
      this.memEnd = memEnd;
      memPointer = memStart;
    }

    public override void Visit(AstNode node) {
      var variableNode = node as VariableAstNode;
      if (variableNode == null) return;
      MemMap[variableNode.VariableName] = memPointer++;
    }
  }

  public class GenerateAsmVisitor : AstNodeVisitor {
    private Dictionary<string, int> jumpLocations;
    private Dictionary<string, uint> memoryMap;
    public List<Instruction> Code { get; private set; }
    public Dictionary<string, int> IntermediateResults; // stores the memory addresses of intermediate results
    private int intermediateResultsPointer = 2000;

    private GenerateAsmVisitor() { }

    public GenerateAsmVisitor(Dictionary<string, uint> mmap) {
      memoryMap = mmap;
      jumpLocations = new Dictionary<string, int>();
      Code = new List<Instruction>();
      IntermediateResults = new Dictionary<string, int>();
    }

    private int MapResult(string resultName) {
      int addr;
      if (IntermediateResults.TryGetValue(resultName, out addr))
        return addr;
      addr = intermediateResultsPointer;
      IntermediateResults[resultName] = addr;
      intermediateResultsPointer++;
      return addr;
    }

    private void UnmapResult(string resultName) {
      // do something without fragmenting the result memory section
    }

    public override void Visit(AstNode node) {
      if (node.Type == AstNodeType.Constant) return;
      var resultAddr = MapResult(node.Id);
      switch (node.Type) {
        case AstNodeType.StartNode: {
            Code.Add(new Instruction(OpCode.Hlt, new Arg(ArgType.Value, 0, indirect: false)));
            break;
          }
        case AstNodeType.Variable: {
            var variableNode = (VariableAstNode)node;
            Code.Add(new Instruction(OpCode.Lda, new Arg(ArgType.Value, variableNode.Value, false)));
            Code.Add(new Instruction(OpCode.Sta, new Arg(ArgType.Value, MapResult(node.Id), indirect: true)));
            break;
          }
        case AstNodeType.BinaryOp: {
            var binaryOpNode = (BinaryOperationAstNode)node;
            var left = binaryOpNode.Left;
            var right = binaryOpNode.Right;
            var leftArg = left.Type == AstNodeType.Constant ? new Arg(ArgType.Value, ((ConstantAstNode)left).Value, false) : new Arg(ArgType.Value, MapResult(left.Id), indirect: true);
            var rightArg = right.Type == AstNodeType.Constant ? new Arg(ArgType.Value, ((ConstantAstNode)right).Value, false) : new Arg(ArgType.Value, MapResult(right.Id), indirect: true);
            switch (binaryOpNode.Op) {
              case AstBinaryOp.Add: {
                  Code.Add(new Instruction(OpCode.Lda, leftArg));
                  Code.Add(new Instruction(OpCode.Adda, rightArg));
                  Code.Add(new Instruction(OpCode.Sta, new Arg(ArgType.Value, resultAddr, indirect: true)));
                  break;
                }
              case AstBinaryOp.Sub: {
                  Code.Add(new Instruction(OpCode.Lda, leftArg));
                  Code.Add(new Instruction(OpCode.Suba, rightArg));
                  Code.Add(new Instruction(OpCode.Sta, new Arg(ArgType.Value, resultAddr, indirect: true)));
                  break;
                }
              case AstBinaryOp.Mul: {
                  var resultAddrArg = new Arg(ArgType.Value, resultAddr, indirect: true);
                  var tmpAddrArg = new Arg(ArgType.Value, MapResult($"{binaryOpNode}_tmp"), indirect: true);
                  var count = Code.Count;
                  Code.Add(new Instruction(OpCode.Lda, new Arg(ArgType.Value, 0, indirect: false)));
                  Code.Add(new Instruction(OpCode.Sta, resultAddrArg));
                  Code.Add(new Instruction(OpCode.Lda, tmpAddrArg));
                  Code.Add(new Instruction(OpCode.Suba, leftArg));
                  Code.Add(new Instruction(OpCode.Jge, new Arg(ArgType.Value, count + 12, indirect: false)));
                  Code.Add(new Instruction(OpCode.Lda, resultAddrArg));
                  Code.Add(new Instruction(OpCode.Adda, rightArg));
                  Code.Add(new Instruction(OpCode.Sta, resultAddrArg));
                  Code.Add(new Instruction(OpCode.Lda, tmpAddrArg));
                  Code.Add(new Instruction(OpCode.Adda, new Arg(ArgType.Value, 1, indirect: false)));
                  Code.Add(new Instruction(OpCode.Sta, tmpAddrArg));
                  Code.Add(new Instruction(OpCode.Jge, new Arg(ArgType.Value, count + 2, indirect: false)));
                  // clean up memory at tmpAddr
                  Code.Add(new Instruction(OpCode.Lda, new Arg(ArgType.Value, 0, indirect: false)));
                  Code.Add(new Instruction(OpCode.Sta, tmpAddrArg));
                  break;
                }
              default:
                throw new Exception("Unknown binary op.");
            }
            break;
          }
        case AstNodeType.Condition:
          break;
        default:
          throw new Exception("Unknown AST node type.");
      }
      jumpLocations[node.Id] = Code.Count - 1;
    }
  }
}
