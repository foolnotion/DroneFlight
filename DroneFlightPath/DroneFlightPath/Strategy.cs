using CodeInterpreter;
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
    public static ast Assign(ast target, ast source) { return ast.Assign(target, source); }
    public static ast ArrayGet(ast array, ast index) { return ast.ArrayGet(array, index); }
    public static ast ArraySet(ast array, ast index, ast value) { return ast.ArraySet(array, index, value); }

    private static readonly ast hold = _(0);
    private static readonly ast up = _(1);
    private static readonly ast right = _(2);
    private static readonly ast down = _(3);
    private static readonly ast left = _(4);
    private static readonly ast nextDroneMovement = Mem(_(0));
    private static readonly ast numberOfMapRows = Mem(_(1));
    private static readonly ast numberOfMapColumns = Mem(_(2));
    private static readonly ast currentX = Mem(_(3));
    private static readonly ast currentY = Mem(_(4));
    private static readonly ast targetX = Mem(_(5));
    private static readonly ast targetY = Mem(_(6));
    private static readonly ast ret = ast.Return();

    public static ast NaiveGradientDescent() {
      var numberOfObstacles = _("numberOfObstacles");
      var numberOfCitizens = _("numberOfCitizens");
      var numberOfDrones = _("numberOfDrones");

      var i = _("i");
      var j = _("j");
      var x = _("x");
      var y = _("y");
      var currentMap = _("currentMap", 2500);
      var obstacleStartMemoryIndex = _(8);
      var citizenStartMemoryIndex = _("citizenStartMemoryIndex");
      var droneStartMemoryIndex = _("droneStartMemoryIndex");

      var citizenNumericMapValue = _(1);
      var droneNumericMapValue = _(1);
      var obstacleNumericMapValue = _(1);


      var putObjectsOnMap = Block(
      // put obstacles on map
      Assign(i, obstacleStartMemoryIndex),
      While(i < 2 * numberOfObstacles,
        Block(
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          ArraySet(currentMap, y * numberOfMapRows + x, obstacleNumericMapValue),
          Assign(i, i + 2)
        )
      ),
      // put citizens on map (without range, for the moment)
      Assign(numberOfCitizens, Mem(i)),
      Assign(i, i + 1), // skip memory pos holding nc
      Assign(citizenStartMemoryIndex, i),
      While(i < 2 * numberOfCitizens,
        Block(
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          ArraySet(currentMap, y * numberOfMapRows + x, citizenNumericMapValue),
          Assign(i, i + 2)
        )
      ),
      // put drones on map
      Assign(numberOfDrones, Mem(i)),
      Assign(i, i + 1), 
      Assign(droneStartMemoryIndex, i),
      While(i < 2 * numberOfDrones,
        Block(
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          ArraySet(currentMap, y * numberOfMapRows + x, droneNumericMapValue),
          Assign(i, i + 2)
        )
      )
    );

      return putObjectsOnMap;
    }

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
          Block(Set(nextDroneMovement, down), ret)
        ),
        If(
          cy > ty,
          Block(Set(nextDroneMovement, up), ret)
        ),
        If(
          cx > tx,
          Block(Set(nextDroneMovement, left), ret)
        ),
        If(
          cx < tx,
          Block(Set(nextDroneMovement, right), ret)
        ),
        ret
      );

      return block;
    }

    public static ast Avoid() {
      var rows = _("rows");
      var cols = _("cols");
      var map = _("map", 2500);
      var dx = _("dx");
      var dy = _("dy");
      var tx = _("tx");
      var ty = _("ty");
      var no = _("addrO"); // addr obstacles
      var nc = _("addrC"); // addr citizens
      var nd = _("addrD"); // addr drones
      var i = _("i");
      var j = _("j");
      var x = _("x");
      var y = _("y");
      var initializeVariables = Block(
        Assign(rows, Mem(_(1))),
        Assign(cols, Mem(_(2))),
        Assign(dx, Mem(_(3))),
        Assign(dy, Mem(_(4))),
        Assign(tx, Mem(_(5))),
        Assign(ty, Mem(_(6))),
        Assign(no, Mem(_(7))),
        Assign(nc, Mem(no * 2 + 8)),
        Assign(nd, Mem((no + nc) * 2 + 9))
      );
      var putObjectsOnMap = Block(
        // define some variables
        Assign(i, _(8)),
        While(i < 2 * no,
          Block(
            Assign(x, Mem(i)),
            Assign(y, Mem(i + 1)),
            ArraySet(map, y * cols + x, _(1)),
            Assign(i, i + 2)
          )
        ),
        Assign(i, i + 1), // skip memory pos holding nc
        While(i < 2 * nc,
          Block(
            Assign(x, Mem(i)),
            Assign(y, Mem(i + 1)),
            ArraySet(map, y * cols + x, _(1)),
            Assign(i, i + 2)
          )
        )
      );
      return Block(
        initializeVariables,
        ret
      );
    }

    public static ast Test() {
      var i = _("i");
      var j = _("j");
      var k = _("k");

      var innerK = Block(
        Mem(_(0), Mem(_(0)) + 1),
        Inc(k)
      );
      var innerJ = Block(
        //        Assign(k, _(0)),
        Do(k < 3, innerK),
        Inc(j)
      );
      var innerI = Block(
        //        Assign(j, _(0)),
        Do(j < 3, innerJ),
        Inc(i)
      );
      return Block(Do(i < 3, innerI), ret);
      //      return Block(Mem(_(0), ast.Max(_(7), _(5))), ret);
    }
  }
}
