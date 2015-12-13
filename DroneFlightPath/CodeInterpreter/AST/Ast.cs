using System;

namespace CodeInterpreter.AST {
  public enum AstNodeType { StartNode, Constant, Variable, BinaryOp, Condition }
  public enum AstBinaryOp { Add, Sub, Div, Rem, Mul, Pow }
  public enum AstCondition { Equal, LessThan, GreaterThan, LessThanOrEqual, GreaterThanOrEqual }

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

    public string Id;
    public string Name;
    public bool IsLeaf;
    public abstract void Accept(AstNodeVisitor visitor);
  }

  public class AstStartNode : AstNode {
    public AstStartNode() : base(AstNodeType.StartNode, "AstStartNode", false) { }
    public override void Accept(AstNodeVisitor visitor) {
      Child.Accept(visitor);
      visitor.Visit(this);
    }

    public AstNode Child;
  }

  public class ConstantAstNode : AstNode {
    public int Value;
    public ConstantAstNode(int value) : base(AstNodeType.Constant, "ConstantAstNode", true) {
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
    public VariableAstNode(string variableName, int value) : base(AstNodeType.Variable, "VariableAstNode", true) {
      VariableName = variableName;
      Value = value;
    }
    public string VariableName;
    public int Value;

    public override void Accept(AstNodeVisitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"Var: {VariableName} value {Value}";
    }
  }

  public class BinaryOperationAstNode : AstNode {
    public BinaryOperationAstNode(AstBinaryOp op, AstNode left, AstNode right) : base(AstNodeType.BinaryOp, "BinaryOperationAstNode", false) {
      Op = op;
      Left = left;
      Right = right;
    }
    public AstBinaryOp Op;
    public AstNode Left;
    public AstNode Right;

    public override void Accept(AstNodeVisitor visitor) {
      Left.Accept(visitor);
      Right.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"Binary op: ({Op} {Left} {Right})";
    }
  }

  public class ConditionAstNode : AstNode {
    public ConditionAstNode(AstCondition condition, AstNode trueBranch, AstNode falseBranch) : base(AstNodeType.Condition, "IfElseAstNode", false) {
      Condition = condition;
      TrueBranch = trueBranch;
      FalseBranch = falseBranch;
    }

    public AstCondition Condition;
    public AstNode TrueBranch;
    public AstNode FalseBranch;

    public override void Accept(AstNodeVisitor visitor) {
      TrueBranch.Accept(visitor);
      FalseBranch.Accept(visitor);
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"Condition: ({Condition} {TrueBranch} {FalseBranch})";
    }
  }
}
