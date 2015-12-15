using ast = CodeInterpreter.AST.AstNode;

namespace DroneFlightPath {
  public static class Strategy {
    public static ast _(int v) { return ast.Constant(v); }
    public static ast _(string v, int size = 1) { return ast.Variable(v, size); }
    public static ast Inc(ast variable) { return ast.Increment(variable); }
    public static ast Dec(ast variable) { return ast.Decrement(variable); }
    public static ast Mem(ast addr) { return ast.MemoryRead(addr); }
    public static ast Mem(ast addr, ast value) { return ast.MemoryWrite(addr, value); }
    public static ast Set(ast target, ast source) { return ast.Assign(target, source); }
    public static ast If(ast condition, ast trueBranch, ast falseBranch = null) {
      return object.Equals(falseBranch, null) ? ast.IfThen(condition, trueBranch) : ast.IfThenElse(condition, trueBranch, falseBranch);
    }
    public static ast While(ast condition, ast body) { return ast.While(condition, body); }
    public static ast Do(ast condition, ast body) { return ast.DoWhile(condition, body); }
    public static ast Block(params ast[] nodes) { return ast.Block(nodes); }

    private static readonly ast hold = _(0);
    private static readonly ast up = _(1);
    private static readonly ast right = _(2);
    private static readonly ast down = _(3);
    private static readonly ast left = _(4);
    private static readonly ast drone = Mem(_(0));
    private static readonly ast ret = ast.Return();

    public static ast BasicStrategy() {
      // define variables
      var cx = _("cx");
      var cy = _("cy");
      var tx = _("tx");
      var ty = _("ty");
      var block = ast.Block(
        Set(cx, Mem(_(3))),
        Set(cy, Mem(_(4))),
        Set(tx, Mem(_(5))),
        Set(ty, Mem(_(6))),
        If(
          cy < ty,
          Block(Set(drone, down), ret)
        ),
        If(
          cy > ty,
          Block(Set(drone, up), ret)
        ),
        If(
          cx > tx,
          Block(Set(drone, left), ret)
        ),
        If(
          cx < tx,
          Block(Set(drone, right), ret)
        ),
        ret
      );

      return block;
    }

    public static ast Test() {
      var rows = Mem(_(1));
      var cols = Mem(_(2));
      var x = Mem(_(3));
      var y = Mem(_(4));
      var nObstacles = _(5);
      var nObstaclesAddr = _(0);
      var i = _("i");
      var j = _("j");
      return Block(
        While(
          i < 10,
          Block(
            ast.Assign(j, i + 1),
            While(
              j < 10,
              Block(
                Mem(_(0), Mem(_(0)) + _(1)),
                Inc(j)
              )
            ),
            Inc(i)
          )
        ),
        ret
      );
    }
  }
}
