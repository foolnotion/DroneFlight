using ast = CodeInterpreter.AST.AstNode;


namespace DroneFlightPath {
  public static class Strategy {
    public static ast _(int v) { return ast.Constant(v); }
    public static ast _(string v, int size = 1) { return ast.Variable(v, size); }
    public static ast Mem(int v) { return ast.Pointer(v); }
    public static ast Set(ast target, ast source) { return ast.Assign(target, source); }
    public static ast If(ast condition, ast trueBranch, ast falseBranch = null) {
      return falseBranch == null ? ast.IfThen(condition, trueBranch) : ast.IfThenElse(condition, trueBranch, falseBranch);
    }
    public static ast Block(params ast[] nodes) { return ast.Block(nodes); }

    private static readonly ast hold = _(0);
    private static readonly ast up = _(1);
    private static readonly ast right = _(2);
    private static readonly ast down = _(3);
    private static readonly ast left = _(4);
    private static readonly ast drone = Mem(0);

    public static ast BasicStrategy() {
      // define variables
      var cx = _("cx");
      var cy = _("cy");
      var tx = _("tx");
      var ty = _("ty");
      var ret = ast.Return();
      var block = ast.Block(
        Set(cx, Mem(3)),
        Set(cy, Mem(4)),
        Set(tx, Mem(5)),
        Set(ty, Mem(6)),
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
  }
}
