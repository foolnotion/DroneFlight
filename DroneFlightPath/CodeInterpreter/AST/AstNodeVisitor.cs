using System;
using System.Collections.Generic;
using System.Linq;
using static CodeInterpreter.Instruction;

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
    public int Start { get; } = 3000;
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

    public int MapObject(string objectId, int size = 1) {
      if (objects == null)
        objects = new Dictionary<string, int>();
      int addr;
      if (objects.TryGetValue(objectId, out addr))
        return addr;
      addr = Pointer;
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
      MemoryMap = new MemoryMap(3000, 1000000);
    }

    public override void Visit(AstNode node) {
      if (node.Type == AstNodeType.Constant)
        return;
      var variable = node as AstVariableNode;
      if (object.Equals(variable, null)) {
        MemoryMap.MapObject(node.Id);
      } else {
        MemoryMap.MapObject(variable.VariableName, variable.Size);
      }
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
            return Arg.Val(((AstConstantNode)leaf).Value);
          }
        case AstNodeType.Variable: {
            var variableNode = (AstVariableNode)leaf;
            var addr = MemoryMap.MapObject(variableNode.VariableName);
            return Arg.Mem(addr);
          }
        default:
          throw new ArgumentException("Unknown leaf node type.");
      }
    }

    public void GenerateAsm(AstNode node) {
      var resultAddr = MemoryMap.MapObject(node.Id);
      var resultAddrArg = Arg.Mem(resultAddr);
      var count = Code.Count;

      switch (node.Type) {
        case AstNodeType.Block: {
            break;
          }
        case AstNodeType.Return: {
            Code.Add(Hlt());
            break;
          }
        case AstNodeType.MemoryAccess: {
            var memoryAccessNode = (AstMemoryAccessNode)node;
            var addr = memoryAccessNode.Address;
            var addrArg = addr.IsLeaf ? LeafToArg(addr) : Arg.Mem(MemoryMap[addr.Id]);
            switch (memoryAccessNode.Op) {
              case AstMemoryOp.Read: {
                  Code.AddRange(new[] {
                    Lda(addrArg),       // load address into accumulator
                    Ldn(Arg.A(false)),  // set N register
                    Lda(Arg.N(true)),   // load value from memory
                    Sta(resultAddrArg)  // store result
                  });
                  break;
                }
              case AstMemoryOp.Write: {
                  var value = memoryAccessNode.Value;
                  var valueArg = value.IsLeaf ? LeafToArg(value) : Arg.Mem(MemoryMap[value.Id]);
                  Code.AddRange(new[] {
                    Lda(addrArg),       // load address into accumulator
                    Ldn(Arg.A(false)),  // set N register
                    Lda(valueArg),      // load value into accumulator
                    Sta(Arg.N(true)),   // set new value in memory
                    Sta(resultAddrArg)  // store result
                  });
                  break;
                }
              default:
                throw new Exception("Unknown memory operation.");
            }
            break;
          }
        case AstNodeType.ArrayAccess: {
            var arrayAccessNode = (AstArrayAccessNode)node;
            var array = arrayAccessNode.Array;
            var index = arrayAccessNode.Index;
            var arrayArg = Arg.Val(LeafToArg(array).Value); // take the address and convert it into a value so we can add the offset
            var indexArg = index.IsLeaf ? LeafToArg(index) : Arg.Mem(MemoryMap[index.Id]);
            switch (arrayAccessNode.Op) {
              case AstArrayOp.GetIndex: {
                  Code.AddRange(new[] {
                    Lda(arrayArg),
                    Adda(indexArg),
                    Ldn(Arg.A(false)),
                    Lda(Arg.N(true)),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstArrayOp.SetIndex: {
                  var value = arrayAccessNode.Value;
                  var valueArg = value.IsLeaf ? LeafToArg(value) : Arg.Mem(MemoryMap[value.Id]);
                  Code.AddRange(new[] {
                    Lda(arrayArg),
                    Adda(indexArg),
                    Ldn(Arg.A(false)),
                    Lda(valueArg),
                    Sta(Arg.N(true))
                  });
                  break;
                }
              default:
                throw new Exception("Unknown array access op.");
            }
            break;
          }
        case AstNodeType.UnaryOp: {
            var unaryOpNode = (AstUnaryNode)node;
            var unaryArg = unaryOpNode.Arg;
            var unaryArgAddr = unaryArg.IsLeaf ? LeafToArg(unaryArg) : Arg.Mem(MemoryMap[unaryArg.Id]);
            switch (unaryOpNode.Op) {
              case AstUnaryOp.Increment: {
                  Code.AddRange(new[] {
                    Lda(unaryArgAddr),
                    Adda(Arg.Val(1)),
                    Sta(unaryArgAddr),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstUnaryOp.Decrement: {
                  Code.AddRange(new[] {
                    Lda(unaryArgAddr),
                    Suba(Arg.Val(1)),
                    Sta(unaryArgAddr),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstUnaryOp.True: {
                  // arg should be a logical operator that evaluates to 0 if true and -1 if false
                  Code.AddRange(new[] {
                    Lda(unaryArgAddr),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstUnaryOp.False: {
                  Code.AddRange(new[] {
                    Lda(Arg.Val(-1)),
                    Suba(unaryArgAddr),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstUnaryOp.Negate: {
                  throw new NotImplementedException();
                }
              default:
                throw new ArgumentOutOfRangeException();
            }
          }
          break;
        case AstNodeType.BinaryOp: {
            var binaryOpNode = (AstBinaryNode)node;
            var left = binaryOpNode.Left;
            var right = binaryOpNode.Right;
            var leftArg = left.IsLeaf ? LeafToArg(left) : Arg.Mem(MemoryMap[left.Id]);
            var rightArg = right.IsLeaf ? LeafToArg(right) : Arg.Mem(MemoryMap[right.Id]);

            #region binary operation switch

            switch (binaryOpNode.Op) {
              case AstBinaryOp.Assign: {
                  Code.Add(Lda(rightArg));
                  Code.Add(Sta(leftArg));
                  Code.Add(Sta(resultAddrArg));
                  break;
                }
              case AstBinaryOp.Min: {
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Suba(rightArg),
                    // if A >= 0, return rightArg
                    Jge(Arg.Val(count + 7)),
                    Lda(leftArg),
                    Sta(resultAddrArg),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 9)),
                    Lda(rightArg),      // count + 7
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Max: {
                  Code.AddRange(new[] {
                    Lda(rightArg),
                    Suba(leftArg),
                    // if A >= 0, return rightArg
                    Jge(Arg.Val(count + 7)),
                    Lda(leftArg),
                    Sta(resultAddrArg),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 9)),
                    Lda(rightArg),      // count + 7
                    Sta(resultAddrArg)
                  });
                  break;
                }
              case AstBinaryOp.Add: {
                  Code.AddRange(new[] {
                Lda(leftArg), Adda(rightArg), Sta(resultAddrArg)
              });
                  break;
                }
              case AstBinaryOp.Sub: {
                  Code.Add(Lda(leftArg));
                  Code.Add(Suba(rightArg));
                  Code.Add(Sta(new Arg(ArgType.Value, resultAddr, indirect: true)));
                  break;
                }
              case AstBinaryOp.Mul: {
                  // mul is implemented as repeated addition
                  // a loop is used for incrementing the value at resultAddr
                  var tmpId = $"{binaryOpNode}_tmp";
                  var tmpAddrArg = Arg.Mem(MemoryMap.MapObject(tmpId));
                  Code.AddRange(new[] {
                    // store 0 in the result
                    Lda(Arg.Val(0)),     // position count
                    Sta(resultAddrArg),  // position count+1
                    // load tmp and subtract left (loop will run left times)
                    Lda(tmpAddrArg),     // position count+2
                    Suba(leftArg),
                    // branch: when tmpAddrArg == left, jump out
                    Jge(Arg.Val(count + 12)),
                    // else add right to result and increment loop variable
                    Lda(resultAddrArg),
                    Adda(rightArg),
                    Sta(resultAddrArg),
                    // increment loop variable
                    Lda(tmpAddrArg),
                    Adda(Arg.Val(1)),
                    Sta(tmpAddrArg),
                    // jump to loop start
                    Jge(Arg.Val(count + 2)),
                    // clean up memory at tmpAddr
                    Lda(Arg.Val(0)),
                    Sta(tmpAddrArg),
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
                    Lda(leftArg),
                    Sta(resultAddrArg),
                    // loop: result = result - right
                    Lda(resultAddrArg),
                    Suba(rightArg),
                    Sta(resultAddrArg),
                    // if result >= 0, increment counter, else we are done
                    Jge(Arg.Val(count + 9)),
                    Lda(tmpAddrArg), // load counter value
                    Sta(resultAddrArg), // write to result
                    Jge(Arg.Val(count + 13)), // jump away
                    // increment counter
                    Lda(tmpAddrArg),
                    Adda(Arg.Val(1)),
                    Sta(tmpAddrArg),
                    // go back to loop start
                    Jge(Arg.Val(count + 2)),
                    // clean up tmpAddr
                    Lda(Arg.Val(0)),
                    Sta(tmpAddrArg)
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
                    Lda(leftArg),
                    Sta(tmpAddrArg),
                    // loop: result = result - right
                    Lda(tmpAddrArg),
                    Sta(resultAddrArg),
                    Suba(rightArg),
                    Sta(tmpAddrArg),
                    Jge(Arg.Val(count + 2)),
                    // if A < 0, we are done, result contains the right value
              });
                  break;
                }
              case AstBinaryOp.Pow: {
                  throw new NotImplementedException();
                }
              case AstBinaryOp.Eq: {
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Suba(rightArg),
                    // if left >= right, jump to the next test
                    Jge(Arg.Val(count + 5)),
                    // if A < 0, return
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 8)),
                    // test right < left
                    Lda(rightArg),
                    Suba(leftArg),
                    // at this point A can only be 0 (left = right) or negative (left > right)
                    Jge(Arg.Val(count + 9)), // if its 0 return 0
                    Lda(Arg.Val(-1)),
                    Sta(resultAddrArg),
              });
                  break;
                }
              case AstBinaryOp.Neq: {
                  // this operation should return the exact oposite values compared to Eq
                  var end = Arg.Val(count + 11);
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Suba(rightArg),
                    // if left >= right, jump to the next test
                    Jge(Arg.Val(count + 5)),
                    // if A < 0, return 0 (true)
                    Lda(Arg.Val(0)),
                    Jge(end),
                    // test right <= left
                    Lda(rightArg),
                    Suba(leftArg),
                    // at this point A can only be 0 (left = right) or negative (left > right)
                    Jge(Arg.Val(count + 10)), // if its 0 return -1 (since they are equal)
                    Lda(Arg.Val(0)),
                    Jge(end),
                    Lda(Arg.Val(-1)),
                    Sta(resultAddrArg),
              });
                  break;
                }
              case AstBinaryOp.Lt: {
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Suba(rightArg),
                    // if left < right, return 0
                    Jge(Arg.Val(count + 5)),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 6)),
                    Lda(Arg.Val(-1)),   // count + 5
                    Sta(resultAddrArg)  // count + 6
              });
                  break;
                }
              case AstBinaryOp.Gt: {
                  Code.AddRange(new[] {
                    Lda(rightArg),
                    Suba(leftArg),
                    // if left < right, return 0
                    Jge(Arg.Val(count + 5)),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 6)),
                    Lda(Arg.Val(-1)),   // count + 5
                    Sta(resultAddrArg)  // count + 6
              });
                  break;
                }
              case AstBinaryOp.Lte: {
                  throw new NotImplementedException();
                }
              case AstBinaryOp.Gte: {
                  throw new NotImplementedException();
                }
              case AstBinaryOp.And: {
                  // the logic here works on the following two assumptions:
                  // - the left and right arguments are logical binary operations
                  // - the logical operations follow the convention: return 0 if true, -1 if false
                  int jmpTrue = count + 4;
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Adda(rightArg),
                    Jge(Arg.Val(jmpTrue)),
                    // write -1 to result
                    Lda(Arg.Val(-1)),
                    Sta(resultAddrArg),
                  });
                  break;
                }
              case AstBinaryOp.Or: {
                  // the logic here works on the following two assumptions:
                  // - the left and right arguments are logical binary operations
                  // - the logical operations follow the convention: return 0 if true, -1 if false
                  int jmpTrue = count + 3;
                  Code.AddRange(new[] {
                    Lda(leftArg),
                    Jge(Arg.Val(jmpTrue)),
                    Lda(rightArg),
                    Sta(resultAddrArg)
                  });
                  break;
                }
              default:
                throw new Exception("Unknown binary Op.");
            }

            #endregion

            break;
          }
        case AstNodeType.Conditional: {
            var childVisitor = new GenerateAsmVisitor(MemoryMap);
            var conditionalNode = (AstConditionalNode)node;
            conditionalNode.TrueBranch.Accept(childVisitor);
            var trueBranchCode = childVisitor.Code.ToList();
            var conditionArg = Arg.Mem(MemoryMap.MapObject(conditionalNode.Condition.Id));
            switch (conditionalNode.Op) {
              case AstConditionalOp.IfThen: {
                  Code.AddRange(new[] {
                    Lda(conditionArg),
                    Jge(Arg.Val(count + 4)),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 4 + trueBranchCode.Count)),
                  });
                  conditionalNode.TrueBranch.Accept(this);
                  break;
                }
              case AstConditionalOp.IfThenElse: {
                  childVisitor.Code.Clear();
                  conditionalNode.FalseBranch.Accept(childVisitor);
                  var falseBranchCode = childVisitor.Code.ToList();
                  Code.AddRange(new[] {
                    Lda(conditionArg),
                    Jge(Arg.Val(count + 4)),
                    Lda(Arg.Val(0)),
                    // if condition false, jump over true branch code section
                    Jge(Arg.Val(count + 4 + trueBranchCode.Count + 2)),
                  });
                  conditionalNode.TrueBranch.Accept(this); // count + 4
                  // skip false branch section if condition true, at the end of the execution of the true branch code
                  Code.AddRange(new[] {
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(count + 4 + trueBranchCode.Count + 2 + falseBranchCode.Count)),
                  });
                  conditionalNode.FalseBranch.Accept(this); // count + 4 + trueBranch.Count + 2
                  break;
                }
              default:
                throw new Exception("Unknown conditional Op.");
            }
            break;
          }
        case AstNodeType.Loop: {
            var loopNode = (AstLoopNode)node;
            var body = loopNode.Body;
            var condition = loopNode.Condition;
            var conditionArg = Arg.Mem(MemoryMap.MapObject(condition.Id)); // cannot be leaf
            switch (loopNode.LoopType) {
              case AstLoopType.While: {
                  // need to visit the loop body to calculate the size of the code
                  // so we create a new visitor, allocate for 1000 variables (won't be used anyway)
                  var v = new GenerateAsmVisitor(new MemoryMap(0, 1000));
                  body.Accept(v);
                  var bodyCount = v.Code.Count;
                  int jmpCond = count;
                  condition.Accept(this);
                  int jmpBody = Code.Count + 4;
                  Code.AddRange(new[] {
                    Lda(conditionArg),
                    Jge(Arg.Val(jmpBody)),
                    Lda(Arg.Val(0)),
                    Jge(Arg.Val(jmpBody + bodyCount))
                  });
                  body.Accept(this);
                  Code.AddRange(new[] {
                    Lda(conditionArg),
                    Jge(Arg.Val(jmpCond))
                  });
                  JumpLocations[condition.Id] = jmpCond;
                  JumpLocations[body.Id] = jmpBody;
                  JumpLocations[loopNode.Id] = jmpCond;
                  break;
                }
              case AstLoopType.DoWhile: {
                  int jmpBody = count;
                  body.Accept(this);
                  int jmpCond = Code.Count;
                  condition.Accept(this);
                  Code.AddRange(new[] {
                    Lda(conditionArg),
                    Jge(Arg.Val(jmpBody))
                  });
                  JumpLocations[condition.Id] = jmpCond;
                  JumpLocations[body.Id] = jmpBody;
                  JumpLocations[loopNode.Id] = jmpBody;
                  break;
                }
              default:
                throw new Exception("Unknown loop type.");
            }
            break;
          }
        default:
          throw new Exception("Unknown AST node type.");
      }
    }

    public override void Visit(AstNode node) {
      if (node.IsLeaf) return;
      NodeNames[node.Id] = node.ToString();
      GenerateAsm(node);
    }
  }
}
