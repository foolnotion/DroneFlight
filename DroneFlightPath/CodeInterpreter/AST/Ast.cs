using System;
using System.Text;

namespace CodeInterpreter.AST {
  public enum AstNodeType {
    Block,
    Constant,
    Variable,
    Pointer,
    BinaryOp,
    UnaryOp,
    Loop,
    Conditional,
    ArrayAccess,
    Return
  }

  public enum AstUnaryOp {
    Negate,
    Increment,
    Decrement
  }

  public enum AstBinaryOp {
    Assign,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Pow,
    Eq,
    Neq,
    Lt,
    Lte,
    Gt,
    Gte,
  }

  public enum AstArrayOp {
    GetIndex,
    SetIndex
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
    private AstNode() { }
    public AstNodeType Type { get; private set; }

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

    public static AstNode operator -(AstNode left, AstNode right) {
      return Sub(left, right);
    }

    public static AstNode operator /(AstNode left, AstNode right) {
      return Div(left, right);
    }

    public static AstNode operator %(AstNode left, AstNode right) {
      return Mod(left, right);
    }

    public static AstNode operator *(AstNode left, AstNode right) {
      return Mul(left, right);
    }

    public static AstNode operator |(AstNode left, AstNode right) {
      return Eq(left, right);
    }

    public static AstNode operator ^(AstNode left, AstNode right) {
      return Neq(left, right);
    }

    public static AstNode operator <(AstNode left, AstNode right) {
      return Lt(left, right);
    }

    public static AstNode operator >(AstNode left, AstNode right) {
      return Gt(left, right);
    }

    public static AstNode operator <=(AstNode left, AstNode right) {
      return Lte(left, right);
    }

    public static AstNode operator >=(AstNode left, AstNode right) {
      return Gte(left, right);
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
    public static AstNode ArraySet(AstNode array, AstNode index, AstNode value) {
      if (array.Type != AstNodeType.Variable)
        throw new ArgumentException("Array argument should be of type Variable.");
      return new ArrayAccessAstNode(AstArrayOp.SetIndex, array, index, value);
    }

    public static AstNode ArrayGet(AstNode array, AstNode index) {
      if (array.Type != AstNodeType.Variable)
        throw new ArgumentException("Array argument should be of type Variable.");
      return new ArrayAccessAstNode(AstArrayOp.GetIndex, array, index);
    }

    public static AstNode Block(params AstNode[] children) {
      return new AstBlockNode(children);
    }

    public static AstNode Pointer(int value) {
      return new PointerAstNode(value);
    }

    public static AstNode Constant(int value) {
      return new ConstantAstNode(value);
    }

    public static AstNode Variable(string name, int size = 1) {
      return new VariableAstNode(name, size);
    }
    public static AstNode Assign(AstNode left, AstNode right) {
      if (left.Type != AstNodeType.Variable && left.Type != AstNodeType.Pointer)
        throw new ArgumentException("Assignment target (left argument) should be a variable or a pointer.");
      return new BinaryAstNode(AstBinaryOp.Assign, left, right);
    }

    public static AstNode Add(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Add, left, right);
    }

    public static AstNode Sub(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Sub, left, right);
    }

    public static AstNode Mul(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Mul, left, right);
    }

    public static AstNode Div(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Div, left, right);
    }

    public static AstNode Mod(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Mod, left, right);
    }

    public static AstNode Pow(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Pow, left, right);
    }

    public static AstNode Eq(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Eq, left, right);
    }

    public static AstNode Neq(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Neq, left, right);
    }

    public static AstNode Lt(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Lt, left, right);
    }

    public static AstNode Gt(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Gt, left, right);
    }

    public static AstNode Lte(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Lte, left, right);
    }

    public static AstNode Gte(AstNode left, AstNode right) {
      return new BinaryAstNode(AstBinaryOp.Gte, left, right);
    }

    public static AstNode IfThen(AstNode condition, AstNode trueBranch) {
      return new ConditionalAstNode(AstConditionalOp.IfThen, condition, trueBranch, null);
    }

    public static AstNode IfThenElse(AstNode condition, AstNode trueBranch, AstNode falseBranch) {
      return new ConditionalAstNode(AstConditionalOp.IfThenElse, condition, trueBranch, falseBranch);
    }

    public static AstNode While(AstNode condition, AstNode body) {
      return new LoopAstNode(AstLoopType.While, condition, body);
    }

    public static AstNode DoWhile(AstNode condition, AstNode body) {
      return new LoopAstNode(AstLoopType.DoWhile, condition, body);
    }

    public static AstNode Neg(AstNode arg) {
      return new UnaryAstNode(AstUnaryOp.Negate, arg);
    }

    public static AstNode Increment(AstNode arg) {
      if (arg.Type != AstNodeType.Variable)
        throw new ArgumentException("Increment operations can only be applied to variables.");
      return new UnaryAstNode(AstUnaryOp.Increment, arg);
    }

    public static AstNode Decrement(AstNode arg) {
      if (arg.Type != AstNodeType.Variable)
        throw new ArgumentException("Decrement operations can only be applied to variables.");
      return new UnaryAstNode(AstUnaryOp.Decrement, arg);
    }

    #endregion
  }

  public class AstReturnNode : AstNode {
    public AstReturnNode() : base(AstNodeType.Return, "AstReturnNode", false) { }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Type}";
    }
  }

  // a block represents a sequence of instructions executed in order
  public class AstBlockNode : AstNode {
    public AstBlockNode(params AstNode[] children) : base(AstNodeType.Block, "AstBlockNode", false) {
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

  public class ConstantAstNode : AstNode {
    public int Value { get; }

    internal ConstantAstNode(int value) : base(AstNodeType.Constant, "ConstantAstNode", true) {
      Value = value;
    }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Value}";
    }
  }

  public class PointerAstNode : AstNode {
    // points to a memory address
    public int Value { get; }

    internal PointerAstNode(int value) : base(AstNodeType.Pointer, "PointerAstNode", true) {
      Value = value;
    }

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"[{Value}]";
    }
  }

  public class VariableAstNode : AstNode {
    public string VariableName { get; }
    public int Size { get; } // size is 1 by default

    internal VariableAstNode(string variableName, int size = 1) : base(AstNodeType.Variable, "VariableAstNode", true) {
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

  public class ArrayAccessAstNode : AstNode {
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
      return $"{Op}({((VariableAstNode)Array).VariableName} {Index} {v})";
    }

    public ArrayAccessAstNode(AstArrayOp op, AstNode array, AstNode index, AstNode value = null) : base(AstNodeType.ArrayAccess, "ArrayAccessAstNode", false) {
      Op = op;
      Array = array;
      Index = index;
      Value = value;
    }
  }

  public class UnaryAstNode : AstNode {
    public AstUnaryOp Op { get; }
    public AstNode Arg { get; }

    internal UnaryAstNode(AstUnaryOp op, AstNode arg) : base(AstNodeType.UnaryOp, "UnaryAstNode", false) {
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

  public class BinaryAstNode : AstNode {
    public AstBinaryOp Op { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }

    internal BinaryAstNode(AstBinaryOp op, AstNode left, AstNode right)
      : base(AstNodeType.BinaryOp, "BinaryAstNode", false) {
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

  public class ConditionalAstNode : AstNode {
    public AstConditionalOp Op { get; }
    public AstNode Condition { get; }
    public AstNode TrueBranch { get; }
    public AstNode FalseBranch { get; }

    internal ConditionalAstNode(AstConditionalOp op, AstNode condition, AstNode trueBranch, AstNode falseBranch)
      : base(AstNodeType.Conditional, "ConditionalAstNode", false) {
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
  public class LoopAstNode : AstNode {
    public AstLoopType LoopType { get; }
    public AstNode Condition { get; }
    public AstNode Body { get; }

    internal LoopAstNode(AstLoopType loopType, AstNode condition, AstNode body)
      : base(AstNodeType.Loop, "LoopAstNode", false) {
      Condition = condition;
      Body = body;
      LoopType = loopType;
    }

    public override void Accept(AstNodeVisitor visitor) {
      // for the DoWhile loop, the body needs to be executed before the condition
      if (LoopType == AstLoopType.DoWhile) {
        Body.Accept(visitor);
      }
      Condition.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      var loopName = LoopType == AstLoopType.While ? "While" : "DoWhile";
      return $"{loopName}({Condition}, {Body})";
    }
  }
}
