using System;

namespace CodeInterpreter.AST {

  public enum AstNodeType { StartNode, Constant, Variable, BinaryOp, UnaryOp, Loop, Conditional }
  public enum AstUnaryOp { Negate, Increment, Decrement }
  public enum AstBinaryOp { Assign, Add, Sub, Mul, Div, Mod, Pow, Eq, Neq, Lt, Lte, Gt, Gte, IdxSet, IdxGet }
  public enum AstConditionalOp { IfThen, IfThenElse }


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
    public static AstNode Constant(int value) {
      return new ConstantAstNode(value);
    }
    public static AstNode Variable(string name) {
      return new VariableAstNode(name);
    }
    public static AstNode Assign(AstNode left, AstNode right) {
      if (left.Type != AstNodeType.Variable)
        throw new ArgumentException("Assignment target should be a variable.");
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
    public static AstNode Neg(AstNode arg) {
      return new UnaryAstNode(AstUnaryOp.Negate, arg);
    }
    public static AstNode Increment(AstNode arg) {
      return new UnaryAstNode(AstUnaryOp.Increment, arg);
    }
    public static AstNode Decrement(AstNode arg) {
      return new UnaryAstNode(AstUnaryOp.Decrement, arg);
    }
    #endregion
  }

  public class AstStartNode : AstNode {
    public AstStartNode(params AstNode[] children) : base(AstNodeType.StartNode, "AstStartNode", false) {
      Children = children;
    }
    public override void Accept(AstNodeVisitor visitor) {
      foreach (var child in Children)
        child.Accept(visitor);
      visitor.Visit(this);
    }
    public AstNode[] Children { get; }
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
      return $"Constant {Value}";
    }
  }

  public class VariableAstNode : AstNode {
    public string VariableName { get; }

    internal VariableAstNode(string variableName) : base(AstNodeType.Variable, "VariableAstNode", true) {
      VariableName = variableName;
    }
    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }
    public override string ToString() {
      return $"Var: {VariableName}";
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
  }

  public class BinaryAstNode : AstNode {
    public AstBinaryOp Op { get; }
    public AstNode Left { get; }
    public AstNode Right { get; }

    internal BinaryAstNode(AstBinaryOp op, AstNode left, AstNode right) : base(AstNodeType.BinaryOp, "BinaryAstNode", false) {
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
      return $"Binary op: ({Op} {Left} {Right})";
    }
  }

  public class ConditionalAstNode : AstNode {
    public AstConditionalOp Op { get; }
    public AstNode Condition { get; }
    public AstNode TrueBranch { get; }
    public AstNode FalseBranch { get; }

    public ConditionalAstNode(AstConditionalOp op, AstNode condition, AstNode trueBranch, AstNode falseBranch)
      : base(AstNodeType.Conditional, "ConditionalAstNode", false) {
      Op = op;
      Condition = condition;
      TrueBranch = trueBranch;
      FalseBranch = falseBranch;
    }
    public override void Accept(AstNodeVisitor visitor) {
      //      TrueBranch.Accept(visitor);
      //      if (Op == AstConditionalOp.IfThenElse)
      //        FalseBranch.Accept(visitor);
      Condition.Accept(visitor);
      visitor.Visit(this);
    }
  }

  public class LoopAstNode : AstNode {
    public AstNode Condition { get; }
    public AstNode Body { get; }

    public LoopAstNode(AstNode condition, AstNode body) : base(AstNodeType.Loop, "LoopAstNode", false) {
      Condition = condition;
      Body = body;
    }
    public override void Accept(AstNodeVisitor visitor) {
      Condition.Accept(visitor);
      Body.Accept(visitor);
      visitor.Visit(this);
    }
  }
}
