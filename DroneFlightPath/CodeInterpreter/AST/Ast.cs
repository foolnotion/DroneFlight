using System;
using System.Text;

namespace CodeInterpreter.AST {
  public enum AstNodeType {
    Block,
    Constant,
    Variable,
    BinaryOp,
    UnaryOp,
    Loop,
    Conditional,
    ArrayAccess,
    MemoryAccess,
    Return
  }

  public enum AstUnaryOp {
    // arithmetic operations
    Negate,
    Increment,
    Decrement,
    // logical operations
    True,
    False
  }

  public enum AstBinaryOp {
    // arithmetic operations
    Assign,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Pow,
    Min,
    Max,
    // logical operations
    Eq,
    Neq,
    Lt,
    Lte,
    Gt,
    Gte,
    And,
    Or
  }

  public enum AstArrayOp {
    GetIndex,
    SetIndex
  }

  public enum AstMemoryOp {
    Read,
    Write
  }

  public enum AstConditionalOp {
    IfThen,
    IfThenElse,
  }

  public enum AstLoopType {
    While,
    DoWhile
  }

  public class Ast {
    private Ast() { }

    public Ast(AstNode root) {
      Root = root;
    }

    public AstNode Root { get; private set; }
  }

  public abstract class AstNode {
    protected bool Equals(AstNode other) {
      return Type == other.Type && string.Equals(Id, other.Id) && string.Equals(Name, other.Name) && IsLeaf == other.IsLeaf;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((AstNode)obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = (int)Type;
        hashCode = (hashCode * 397) ^ (Id?.GetHashCode() ?? 0);
        hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
        hashCode = (hashCode * 397) ^ IsLeaf.GetHashCode();
        return hashCode;
      }
    }

    private AstNode() { }
    public AstNodeType Type { get; }

    protected AstNode(AstNodeType type, string name, bool isLeaf) {
      Type = type;
      Name = name;
      IsLeaf = isLeaf;
      Id = Guid.NewGuid().ToString();
    }

    public string Id { get; }
    public string Name { get; }
    public bool IsLeaf { get; }
    public abstract void Accept(AstNodeVisitor visitor);

    #region overloads providing syntactic sugar
    public static AstNode operator <<(AstNode left, int value) {
      return Assign(left, AstNode.Constant(value));
    }
    public static AstNode operator +(AstNode left, AstNode right) {
      return Add(left, right);
    }
    public static AstNode operator +(AstNode left, int right) {
      return Add(left, Constant(right));
    }
    public static AstNode operator +(int left, AstNode right) {
      return Add(Constant(left), right);
    }
    public static AstNode operator -(AstNode left, AstNode right) {
      return Sub(left, right);
    }
    public static AstNode operator -(AstNode left, int right) {
      return Sub(left, Constant(right));
    }
    public static AstNode operator -(int left, AstNode right) {
      return Sub(Constant(left), right);
    }
    public static AstNode operator /(AstNode left, AstNode right) {
      return Div(left, right);
    }
    public static AstNode operator /(AstNode left, int right) {
      return Div(left, Constant(right));
    }
    public static AstNode operator /(int left, AstNode right) {
      return Div(Constant(left), right);
    }
    public static AstNode operator %(AstNode left, AstNode right) {
      return Mod(left, right);
    }
    public static AstNode operator %(AstNode left, int right) {
      return Mod(left, Constant(right));
    }
    public static AstNode operator %(int left, AstNode right) {
      return Mod(Constant(left), right);
    }
    public static AstNode operator *(AstNode left, AstNode right) {
      return Mul(left, right);
    }
    public static AstNode operator *(AstNode left, int right) {
      return Mul(left, Constant(right));
    }
    public static AstNode operator *(int left, AstNode right) {
      return Mul(Constant(left), right);
    }
    public static AstNode operator ==(AstNode left, AstNode right) {
      return Eq(left, right);
    }
    public static AstNode operator ==(AstNode left, int right) {
      return Eq(left, Constant(right));
    }
    public static AstNode operator ==(int left, AstNode right) {
      return Eq(Constant(left), right);
    }
    public static AstNode operator !=(AstNode left, AstNode right) {
      return Neq(left, right);
    }
    public static AstNode operator !=(AstNode left, int right) {
      return Neq(left, Constant(right));
    }
    public static AstNode operator !=(int left, AstNode right) {
      return Neq(Constant(left), right);
    }
    public static AstNode operator <(AstNode left, AstNode right) {
      return Lt(left, right);
    }
    public static AstNode operator <(AstNode left, int right) {
      return Lt(left, Constant(right));
    }
    public static AstNode operator <(int left, AstNode right) {
      return Lt(Constant(left), right);
    }
    public static AstNode operator >(AstNode left, AstNode right) {
      return Gt(left, right);
    }
    public static AstNode operator >(AstNode left, int right) {
      return Gt(left, Constant(right));
    }
    public static AstNode operator >(int left, AstNode right) {
      return Gt(Constant(left), right);
    }
    public static AstNode operator <=(AstNode left, AstNode right) {
      return Lte(left, right);
    }
    public static AstNode operator >=(AstNode left, AstNode right) {
      return Gte(left, right);
    }
    public static AstNode operator &(AstNode left, AstNode right) {
      return And(left, right);
    }
    public static AstNode operator |(AstNode left, AstNode right) {
      return Or(left, right);
    }
    public static AstNode operator -(AstNode node) {
      return Neg(node);
    }
    public static AstNode operator --(AstNode node) {
      return Decrement(node);
    }
    public static AstNode operator ++(AstNode node) {
      return Increment(node);
    }
    #endregion

    #region factory methods
    public static AstNode MemoryWrite(AstNode address, AstNode value) {
      return new AstMemoryAccessNode(AstMemoryOp.Write, address, value);
    }

    public static AstNode MemoryRead(AstNode address) {
      return new AstMemoryAccessNode(AstMemoryOp.Read, address);
    }

    public static AstNode ArraySet(AstNode array, AstNode index, AstNode value) {
      if (array.Type != AstNodeType.Variable)
        throw new ArgumentException("Array argument should be of type Variable.");
      return new AstArrayAccessNode(AstArrayOp.SetIndex, array, index, value);
    }

    public static AstNode ArrayGet(AstNode array, AstNode index) {
      if (array.Type != AstNodeType.Variable)
        throw new ArgumentException("Array argument should be of type Variable.");
      return new AstArrayAccessNode(AstArrayOp.GetIndex, array, index);
    }

    public static AstNode Return() {
      return new AstReturnNode();
    }

    public static AstNode Block(params AstNode[] children) {
      return new AstBlockNode(children);
    }

    public static AstNode Constant(int value) {
      return new AstConstantNode(value);
    }

    public static AstNode Variable(string name, int size = 1) {
      return new AstVariableNode(name, size);
    }
    public static AstNode Assign(AstNode left, AstNode right) {
      //      if (left.Type != AstNodeType.Variable)
      //        throw new ArgumentException("Assignment target (left argument) should be a variable.");
      return new AstBinaryNode(AstBinaryOp.Assign, left, right);
    }

    public static AstNode Min(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Min, left, right);
    }

    public static AstNode Max(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Max, left, right);
    }

    public static AstNode Add(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Add, left, right);
    }

    public static AstNode Sub(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Sub, left, right);
    }

    public static AstNode Mul(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Mul, left, right);
    }

    public static AstNode Div(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Div, left, right);
    }

    public static AstNode Mod(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Mod, left, right);
    }

    public static AstNode Pow(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Pow, left, right);
    }

    public static AstNode Eq(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Eq, left, right);
    }

    public static AstNode Neq(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Neq, left, right);
    }

    public static AstNode Lt(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Lt, left, right);
    }

    public static AstNode Gt(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Gt, left, right);
    }

    public static AstNode Lte(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Lte, left, right);
    }

    public static AstNode Gte(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Gte, left, right);
    }

    public static AstNode And(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.And, left, right);
    }

    public static AstNode Or(AstNode left, AstNode right) {
      return new AstBinaryNode(AstBinaryOp.Or, left, right);
    }

    public static AstNode IfThen(AstNode condition, AstNode trueBranch) {
      return new AstConditionalNode(AstConditionalOp.IfThen, condition, trueBranch, null);
    }

    public static AstNode IfThenElse(AstNode condition, AstNode trueBranch, AstNode falseBranch) {
      return new AstConditionalNode(AstConditionalOp.IfThenElse, condition, trueBranch, falseBranch);
    }

    public static AstNode While(AstNode condition, AstNode body) {
      return new AstLoopNode(AstLoopType.While, condition, body);
    }

    public static AstNode DoWhile(AstNode condition, AstNode body) {
      return new AstLoopNode(AstLoopType.DoWhile, condition, body);
    }

    public static AstNode Neg(AstNode arg) {
      return new AstUnaryNode(AstUnaryOp.Negate, arg);
    }

    public static AstNode Increment(AstNode arg) {
      if (arg.Type != AstNodeType.Variable)
        throw new ArgumentException("Increment operations can only be applied to variables.");
      return new AstUnaryNode(AstUnaryOp.Increment, arg);
    }

    public static AstNode Decrement(AstNode arg) {
      if (arg.Type != AstNodeType.Variable)
        throw new ArgumentException("Decrement operations can only be applied to variables.");
      return new AstUnaryNode(AstUnaryOp.Decrement, arg);
    }

    #endregion
  }

  public class AstReturnNode : AstNode {
    internal AstReturnNode() : base(AstNodeType.Return, "AstReturnNode", false) { }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Type}";
    }
  }

  // a block represents a sequence of instructions executed in order
  public class AstBlockNode : AstNode {
    internal AstBlockNode(params AstNode[] children) : base(AstNodeType.Block, "AstBlockNode", false) {
      Children = children;
    }

    public override void Accept(AstNodeVisitor visitor) {
      foreach (var child in Children)
        child.Accept(visitor);
      visitor.Visit(this);
    }

    public AstNode[] Children { get; }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.AppendLine("Block: {");
      foreach (var c in Children)
        sb.AppendLine($"\t{c}");
      sb.AppendLine("}");
      return sb.ToString();
    }
  }

  public class AstConstantNode : AstNode {
    public int Value { get; }

    internal AstConstantNode(int value) : base(AstNodeType.Constant, "AstConstantNode", true) {
      Value = value;
    }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Value}";
    }
  }

  public class AstVariableNode : AstNode {
    public string VariableName { get; }
    public int Size { get; } // size is 1 by default

    internal AstVariableNode(string variableName, int size = 1) : base(AstNodeType.Variable, "AstVariableNode", true) {
      VariableName = variableName;
      Size = size;
    }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"Var \"{VariableName}\"";
    }
  }

  public class AstMemoryAccessNode : AstNode {
    public AstMemoryOp Op { get; }
    public AstNode Address { get; }
    public AstNode Value { get; }

    public override void Accept(AstNodeVisitor visitor) {
      Address.Accept(visitor);
      Value?.Accept(visitor);
      visitor.Visit(this);
    }

    internal AstMemoryAccessNode(AstMemoryOp op, AstNode address, AstNode value = null) : base(AstNodeType.MemoryAccess, "AstMemoryAccessNode", false) {
      Op = op;
      Address = address;
      Value = value;
    }

    public override string ToString() {
      var v = Value?.ToString() ?? "";
      return $"{Op}({Address} {Address} {v})";
    }
  }

  public class AstArrayAccessNode : AstNode {
    public AstArrayOp Op { get; }
    public AstNode Array { get; }
    public AstNode Index { get; }
    public AstNode Value { get; }

    public override void Accept(AstNodeVisitor visitor) {
      Array.Accept(visitor);
      Index.Accept(visitor);
      Value?.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      var v = Value?.ToString() ?? "";
      return $"{Op}({((AstVariableNode)Array).VariableName} {Index} {v})";
    }

    internal AstArrayAccessNode(AstArrayOp op, AstNode array, AstNode index, AstNode value = null) : base(AstNodeType.ArrayAccess, "AstArrayAccessNode", false) {
      Op = op;
      Array = array;
      Index = index;
      Value = value;
    }
  }

  public class AstUnaryNode : AstNode {
    public AstUnaryOp Op { get; }
    public AstNode Arg { get; }

    internal AstUnaryNode(AstUnaryOp op, AstNode arg) : base(AstNodeType.UnaryOp, "AstUnaryNode", false) {
      Op = op;
      Arg = arg;
    }

    public override void Accept(AstNodeVisitor visitor) {
      Arg.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Op}({Arg})";
    }
  }

  public class AstBinaryNode : AstNode {
    public AstBinaryOp Op { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }

    internal AstBinaryNode(AstBinaryOp op, AstNode left, AstNode right)
      : base(AstNodeType.BinaryOp, "AstBinaryNode", false) {
      Op = op;
      Left = left;
      Right = right;
    }

    public override void Accept(AstNodeVisitor visitor) {
      Left.Accept(visitor);
      Right.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Op}({Left}, {Right})";
    }
  }

  public class AstConditionalNode : AstNode {
    public AstConditionalOp Op { get; }
    public AstNode Condition { get; }
    public AstNode TrueBranch { get; }
    public AstNode FalseBranch { get; }

    internal AstConditionalNode(AstConditionalOp op, AstNode condition, AstNode trueBranch, AstNode falseBranch)
      : base(AstNodeType.Conditional, "AstConditionalNode", false) {
      Op = op;
      Condition = condition;
      TrueBranch = trueBranch;
      FalseBranch = falseBranch;
    }

    public override void Accept(AstNodeVisitor visitor) {
      Condition.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Op}({Condition}, {TrueBranch}, {FalseBranch})";
    }
  }

  // execute body as long as condition is true
  public class AstLoopNode : AstNode {
    public AstLoopType LoopType { get; }
    public AstNode Condition { get; }
    public AstNode Body { get; }

    internal AstLoopNode(AstLoopType loopType, AstNode condition, AstNode body)
      : base(AstNodeType.Loop, "AstLoopNode", false) {
      Condition = condition;
      Body = body;
      LoopType = loopType;
    }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var loopName = LoopType == AstLoopType.While ? "While" : "DoWhile";
      return $"{loopName}({Condition}, {Body})";
    }
  }
}
