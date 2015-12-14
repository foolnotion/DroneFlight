using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeInterpreter.AST {
  public abstract class AstNodeVisitor {
    public abstract void Visit(AstNode node);
  }

  // tries to simplify the tree, eg:
  // - Ast.Neg(Ast.Constant(3)) => Ast.Constant(-3)
  // - Ast.Add(Ast.Constant(1), Ast.Constant(2)) => Ast.Constant(3)
  // .. and so on

  public class SimplificationVisitor : AstNodeVisitor {
    public override void Visit(AstNode node) {
      throw new NotImplementedException();
    }
  }

  public class MemoryMap {
    public int Start { get; } = 1000;
    public int End { get; } = 10000;
    public int Pointer { get; private set; }

    private Dictionary<string, int> objects;
    public Dictionary<string, int> Objects { get { return objects; } }

    private MemoryMap() { }

    public MemoryMap(int start, int end) {
      if (!(start < end))
        throw new ArgumentException("Invalid memory limits.");
      Start = start;
      End = end;
      Pointer = Start;
    }

    public int MapObject(string objectId) {
      if (objects == null)
        objects = new Dictionary<string, int>();
      int addr;
      if (objects.TryGetValue(objectId, out addr))
        return addr;
      addr = Pointer++;
      objects[objectId] = addr;
      return addr;
    }

    public int MapObject(string objectId, int size) {
      if (objects == null)
        objects = new Dictionary<string, int>();
      int addr;
      if (objects.TryGetValue(objectId, out addr))
        return addr;
      addr = Pointer++;
      objects[objectId] = addr;
      Pointer += size;
      return addr;
    }

    public int this[string name] {
      get { return objects[name]; }
    }
  }

  public class MapObjectsToMemoryVisitor : AstNodeVisitor {
    public MemoryMap MemoryMap { get; }

    public MapObjectsToMemoryVisitor() {
      MemoryMap = new MemoryMap(1000, 10000);
    }

    public override void Visit(AstNode node) {
      if (node.Type == AstNodeType.Constant)
        return;
      var variable = node as VariableAstNode;
      var id = variable == null ? node.Id : variable.VariableName;
      MemoryMap.MapObject(id);
    }
  }

  public class GenerateAsmVisitor : AstNodeVisitor {
    public Dictionary<string, int> JumpLocations { get; private set; }
    public List<Instruction> Code { get; private set; }
    public MemoryMap MemoryMap { get; private set; } // stores the memory addresses of intermediate results

    public Dictionary<string, string> NodeNames { get; }

    private GenerateAsmVisitor() { }

    public GenerateAsmVisitor(MemoryMap mmap) {
      MemoryMap = mmap;
      JumpLocations = new Dictionary<string, int>();
      Code = new List<Instruction>();
      NodeNames = new Dictionary<string, string>();
    }

    private Arg LeafToArg(AstNode leaf) {
      if (!leaf.IsLeaf)
        throw new ArgumentException($"Provided argument {leaf.Name} is not a leaf node.");
      switch (leaf.Type) {
        case AstNodeType.Constant: {
            return Arg.Val(((ConstantAstNode)leaf).Value);
          }
        case AstNodeType.Variable: {
            var variableNode = (VariableAstNode)leaf;
            var addr = MemoryMap.MapObject(variableNode.VariableName);
            return Arg.Mem(addr);
          }
        case AstNodeType.Array: {
            var arrayNode = (ArrayAstNode)leaf;
            var addr = MemoryMap.MapObject(arrayNode.VariableName, arrayNode.Size);
            return Arg.Mem(addr);
          }
        default:
          throw new ArgumentException("Unknown leaf node type.");
      }
    }

    private int CalculateJumpLocation(AstNode node) {
      if (node.IsLeaf) return -1;
      var count = Code.Count;
      switch (node.Type) {
        case AstNodeType.BinaryOp: {
            var binaryOpNode = (BinaryAstNode)node;
            var left = binaryOpNode.Left;
            var right = binaryOpNode.Right;
            if (left.IsLeaf && right.IsLeaf) return count;
            if (left.IsLeaf)
              return JumpLocations[right.Id];
            return JumpLocations[left.Id];
          }
        case AstNodeType.UnaryOp: {
            var unaryOpNode = (UnaryAstNode)node;
            var arg = unaryOpNode.Arg;
            return arg.IsLeaf ? count : JumpLocations[arg.Id];
          }
        case AstNodeType.Conditional: {
            var conditionalNode = (ConditionalAstNode)node;
            var condition = conditionalNode.Condition;
            return condition.IsLeaf ? count : JumpLocations[condition.Id];
          }
        case AstNodeType.Loop: {
            var loopNode = (LoopAstNode)node;
            var condition = loopNode.Condition;
            var body = loopNode.Body;
            switch (loopNode.LoopType) {
              case AstLoopType.While: {
                  return condition.IsLeaf ? count : JumpLocations[condition.Id];
                }
              case AstLoopType.DoWhile: {
                  return body.IsLeaf ? (condition.IsLeaf ? count : JumpLocations[condition.Id]) : JumpLocations[body.Id];
                }
              default:
                throw new Exception("Unknown loop type.");
            }
          }
        case AstNodeType.Block: {
            var blockNode = (AstBlockNode)node;
            foreach (var child in blockNode.Children)
              if (!child.IsLeaf)
                return JumpLocations[child.Id];
            return count;
          }
        case AstNodeType.Constant:
        case AstNodeType.Variable:
        case AstNodeType.Array:
          return -1;
        default:
          throw new ArgumentException("Unknown loop type.");
      }
    }

    public void GenerateAsm(AstNode node) {
      var resultAddr = MemoryMap.MapObject(node.Id);
      var resultAddrArg = Arg.Mem(resultAddr);
      var count = Code.Count;

      switch (node.Type) {
        case AstNodeType.Array: {
            var arrayNode = (ArrayAstNode)node;
            Code.Add(Instruction.Lda(Arg.Val(arrayNode.Size)));
            Code.Add(Instruction.Sta(resultAddrArg));
            break;
          }
        case AstNodeType.Block: {
            Code.Add(Instruction.Hlt());
            break;
          }
        case AstNodeType.BinaryOp: {
            var binaryOpNode = (BinaryAstNode)node;
            var left = binaryOpNode.Left;
            var right = binaryOpNode.Right;
            var leftArg = left.IsLeaf ? LeafToArg(left) : Arg.Mem(MemoryMap[left.Id]);
            var rightArg = right.IsLeaf ? LeafToArg(right) : Arg.Mem(MemoryMap[right.Id]);
            #region binary operation switch
            switch (binaryOpNode.Op) {
              case AstBinaryOp.Assign: {
                  Code.AddRange(new[] {
                    Instruction.Lda(rightArg),
                    Instruction.Sta(leftArg),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Add: {
                  Code.AddRange(new[] {
                    Instruction.Lda(leftArg),
                    Instruction.Adda(rightArg),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Sub: {
                  Code.Add(Instruction.Lda(leftArg));
                  Code.Add(Instruction.Suba(rightArg));
                  Code.Add(Instruction.Sta(new Arg(ArgType.Value, resultAddr, indirect: true)));
                  break;
                }
              case AstBinaryOp.Mul: {
                  // mul is implemented as repeated addition
                  // a loop is used for incrementing the value at resultAddr
                  var tmpId = $"{binaryOpNode}_tmp";
                  var tmpAddrArg = Arg.Mem(MemoryMap.MapObject(tmpId));
                  Code.AddRange(new[] {
                    // store 0 in the result
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Sta(resultAddrArg),
                    // load tmp and subtract left (loop will run left times)
                    Instruction.Lda(tmpAddrArg),
                    Instruction.Suba(leftArg),
                    // branch: when tmpAddrArg == left, jump out
                    Instruction.Jge(Arg.Val(count + 12)),
                    // else add right to result and increment loop variable
                    Instruction.Lda(resultAddrArg),
                    Instruction.Adda(rightArg),
                    Instruction.Sta(resultAddrArg),
                    // increment loop variable
                    Instruction.Lda(tmpAddrArg),
                    Instruction.Adda(Arg.Val(1)),
                    Instruction.Sta(tmpAddrArg),
                    // jump to loop start
                    Instruction.Jge(Arg.Val(count + 2)),
                    // clean up memory at tmpAddr
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Sta(tmpAddrArg),
                  });
                  break;
                }
              case AstBinaryOp.Div: {
                  // div is implemented as repeated subtraction
                  // but the result will be given by the loop counter
                  // eg: for 2/2 the code will count how many times 2 can be subtracted from 2 with a >= 0 result
                  var tmpId = $"{binaryOpNode}_tmp";
                  var tmpAddrArg = new Arg(ArgType.Value, MemoryMap.MapObject(tmpId), indirect: true); // address for the counter (value will be initially zero)
                  Code.AddRange(new[] {
                    // initialize result value with left
                    Instruction.Lda(leftArg),
                    Instruction.Sta(resultAddrArg),
                    // loop: result = result - right
                    Instruction.Lda(resultAddrArg),
                    Instruction.Suba(rightArg),
                    Instruction.Sta(resultAddrArg),
                    // if result >= 0, increment counter, else we are done
                    Instruction.Jge(Arg.Val(count + 9)),
                    Instruction.Lda(tmpAddrArg), // load counter value
                    Instruction.Sta(resultAddrArg), // write to result
                    Instruction.Jge(Arg.Val(count + 13)), // jump away
                    // increment counter
                    Instruction.Lda(tmpAddrArg),
                    Instruction.Adda(Arg.Val(1)),
                    Instruction.Sta(tmpAddrArg),
                    // go back to loop start
                    Instruction.Jge(Arg.Val(count + 2)),
                    // clean up tmpAddr
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Sta(tmpAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Mod: {
                  // mod is implemented as repeated subtraction
                  // but the result will be given by the last result value before (result -= right) < 0
                  // eg: for 3/2 the code will count how many times 2 can be subtracted from 3 with a result >= 0
                  var tmpId = $"{binaryOpNode}_tmp";
                  var tmpAddrArg = Arg.Mem(MemoryMap.MapObject(tmpId)); // address for the counter (value will be initially zero)
                  Code.AddRange(new[] {
                    // initialize result value with left
                    Instruction.Lda(leftArg),
                    Instruction.Sta(tmpAddrArg),
                    // loop: result = result - right
                    Instruction.Lda(tmpAddrArg),
                    Instruction.Sta(resultAddrArg),
                    Instruction.Suba(rightArg),
                    Instruction.Sta(tmpAddrArg),
                    Instruction.Jge(Arg.Val(count + 2)),
                    // if A < 0, we are done, result contains the right value
                  });
                  break;
                }
              case AstBinaryOp.Pow: {
                  throw new NotImplementedException();
                }
              case AstBinaryOp.Eq: {
                  Code.AddRange(new[] {
                    Instruction.Lda(leftArg),
                    Instruction.Suba(rightArg),
                    // if left >= right, jump to the next test
                    Instruction.Jge(Arg.Val(count + 5)),
                    // if A < 0, return
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(Arg.Val(count + 8)),
                    // test right < left
                    Instruction.Lda(rightArg),
                    Instruction.Suba(leftArg),
                    // at this point A can only be 0 (left = right) or negative (left > right)
                    Instruction.Jge(Arg.Val(count + 9)), // if its 0 return 0
                    Instruction.Lda(Arg.Val(-1)),
                    Instruction.Sta(resultAddrArg),
                  });
                  break;
                }
              case AstBinaryOp.Neq: {
                  // this operation should return the exact oposite values compared to Eq
                  var end = Arg.Val(count + 11);
                  Code.AddRange(new[] {
                    Instruction.Lda(leftArg),
                    Instruction.Suba(rightArg),
                    // if left >= right, jump to the next test
                    Instruction.Jge(Arg.Val(count + 5)),
                    // if A < 0, return 0 (true)
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(end),
                    // test right <= left
                    Instruction.Lda(rightArg),
                    Instruction.Suba(leftArg),
                    // at this point A can only be 0 (left = right) or negative (left > right)
                    Instruction.Jge(Arg.Val(count + 10)), // if its 0 return -1 (since they are equal)
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(end),
                    Instruction.Lda(Arg.Val(-1)),
                    Instruction.Sta(resultAddrArg),
                  });
                  break;
                }
              case AstBinaryOp.Lt: {
                  Code.AddRange(new[] {
                    Instruction.Lda(leftArg),
                    Instruction.Suba(rightArg),
                    // if left < right, return 0
                    Instruction.Jge(Arg.Val(count + 5)),
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(Arg.Val(count + 6)),
                    Instruction.Lda(Arg.Val(-1)),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Gt: {
                  Code.AddRange(new[] {
                    Instruction.Lda(rightArg),
                    Instruction.Suba(leftArg),
                    // if left < right, return 0
                    Instruction.Jge(Arg.Val(count + 5)),
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(Arg.Val(count + 6)),
                    Instruction.Lda(Arg.Val(-1)),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.IdxGet: {
                  Code.AddRange(new[] {
                    Instruction.Lda(leftArg),
                    Instruction.Adda(rightArg),
                    Instruction.Adda(Arg.Val(1)),
                    Instruction.Sta(resultAddrArg),
                    Instruction.Ldn(resultAddrArg),
                    Instruction.Lda(Arg.N(true)),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Lte: {
                  throw new NotImplementedException();
                }
              case AstBinaryOp.Gte: {
                  throw new NotImplementedException();
                }
              default:
                throw new Exception("Unknown binary Op.");
            }
            #endregion
            break;
          }
        case AstNodeType.Conditional: {
            var childVisitor = new GenerateAsmVisitor(MemoryMap);
            var conditionalNode = (ConditionalAstNode)node;
            conditionalNode.TrueBranch.Accept(childVisitor);
            var trueBranchCode = childVisitor.Code.ToList();
            var conditionArg = Arg.Mem(MemoryMap.MapObject(conditionalNode.Condition.Id));
            switch (conditionalNode.Op) {
              case AstConditionalOp.IfThen: {
                  Code.AddRange(new[] {
                    Instruction.Lda(conditionArg),
                    Instruction.Jge(Arg.Val(count + 4)),
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(Arg.Val(count + 4 + trueBranchCode.Count)),
                  });
                  Code.AddRange(trueBranchCode);
                  break;
                }
              case AstConditionalOp.IfThenElse: {
                  childVisitor.Code.Clear();
                  conditionalNode.FalseBranch.Accept(childVisitor);
                  var falseBranchCode = childVisitor.Code.ToList();
                  Code.AddRange(new[] {
                    Instruction.Lda(conditionArg),
                    Instruction.Jge(Arg.Val(count + 4)),
                    Instruction.Lda(Arg.Val(0)),
                    // if condition false, jump over true branch code section
                    Instruction.Jge(Arg.Val(count + 4 + trueBranchCode.Count + 2)),
                  });
                  Code.AddRange(trueBranchCode);
                  // skip false branch section if condition true, at the end of the execution of the true branch code
                  Code.AddRange(new[] {
                    Instruction.Lda(Arg.Val(0)),
                    Instruction.Jge(Arg.Val(count + 4 + trueBranchCode.Count + 2 + falseBranchCode.Count)),
                  });
                  Code.AddRange(falseBranchCode);
                  break;
                }
              default:
                throw new Exception("Unknown conditional Op.");
            }
            break;
          }
        case AstNodeType.Loop: {
            var loopNode = (LoopAstNode)node;
            var body = loopNode.Body;
            var condition = loopNode.Condition;
            var conditionArg = Arg.Mem(MemoryMap.MapObject(condition.Id));

            switch (loopNode.LoopType) {
              case AstLoopType.While: {
                  throw new NotSupportedException("The while loop is currently not supported.");
                }
              case AstLoopType.DoWhile: {
                  Code.AddRange(new[] {
                    Instruction.Lda(conditionArg),
                    Instruction.Jge(Arg.Val(JumpLocations[body.Id]))
                  });
                  break;
                }
              default:
                throw new Exception("Unknown loop type.");
            }
            break;
          }
        case AstNodeType.UnaryOp: {
            var unaryOpNode = (UnaryAstNode)node;
            var unaryArg = unaryOpNode.Arg;
            var unaryArgAddr = unaryArg.IsLeaf ? LeafToArg(unaryArg) : Arg.Mem(MemoryMap[unaryArg.Id]);
            switch (unaryOpNode.Op) {
              case AstUnaryOp.Increment: {
                  Code.AddRange(new[] {
                    Instruction.Lda(unaryArgAddr),
                    Instruction.Adda(Arg.Val(1)),
                    Instruction.Sta(unaryArgAddr),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
              case AstUnaryOp.Decrement: {
                  Code.AddRange(new[] {
                    Instruction.Lda(unaryArgAddr),
                    Instruction.Suba(Arg.Val(1)),
                    Instruction.Sta(unaryArgAddr),
                    Instruction.Sta(resultAddrArg)
                  });
                  break;
                }
            }
          }
          break;
        default:
          throw new Exception("Unknown AST node type.");
      }
    }

    public override void Visit(AstNode node) {
      NodeNames[node.Id] = node.ToString();
      if (node.Type == AstNodeType.Variable || node.Type == AstNodeType.Constant) return;
      JumpLocations[node.Id] = CalculateJumpLocation(node);
      GenerateAsm(node);
    }
  }
}
