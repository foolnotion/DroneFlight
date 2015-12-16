using CodeInterpreter.AST;
using ast = CodeInterpreter.AST.AstNode;

namespace DroneFlightPath {
  public static class Strategy {
    #region shortcuts
    public static ast _(int v) { return ast.Constant(v); }
    public static ast _(string v, int size = 1) { return ast.Variable(v, size); }
    public static ast _(params ast[] body) { return ast.Block(body); }
    public static ast Inc(ast variable) { return ast.Increment(variable); }
    public static ast Dec(ast variable) { return ast.Decrement(variable); }
    public static ast Mem(ast addr) { return ast.MemoryRead(addr); }
    public static ast Mem(ast addr, ast value) { return ast.MemoryWrite(addr, value); }
    public static ast Set(ast target, ast source) { return ast.Assign(target, source); }
    public static ast If(ast condition, ast trueBranch, ast falseBranch = null) {
      return object.Equals(falseBranch, null) ? ast.IfThen(condition, trueBranch) : ast.IfThenElse(condition, trueBranch, falseBranch);
    }
    public static ast While(ast condition, ast body) { return ast.While(condition, body); }
    public static ast While(ast condition, params ast[] body) { return ast.While(condition, Block(body)); }
    public static ast Do(ast condition, ast body) { return ast.DoWhile(condition, body); }
    public static ast Block(params ast[] nodes) { return ast.Block(nodes); }
    public static ast Assign(ast target, ast source) { return ast.Assign(target, source); }
    public static ast ArrayGet(ast array, ast index) { return ast.ArrayGet(array, index); }
    public static ast ArraySet(ast array, ast index, ast value) { return ast.ArraySet(array, index, value); }
    public static ast Min(ast a, ast b) { return ast.Min(a, b); }
    public static ast Min(ast a, ast b, ast c) { return ast.Min(a, ast.Min(b, c)); }
    public static ast Min(ast a, ast b, ast c, ast d) { return ast.Min(ast.Min(a, b), ast.Min(c, d)); }
    public static ast Max(ast a, ast b) { return ast.Max(a, b); }
    public static ast Max(ast a, ast b, ast c) { return ast.Max(a, ast.Max(b, c)); }
    public static ast Max(ast a, ast b, ast c, ast d) { return ast.Max(ast.Max(a, b), ast.Max(c, d)); }
    public static ast Abs(ast node) { return ast.Abs(node); }
    #endregion

    #region predefined variables
    private static readonly ast Hold = _(0);
    private static readonly ast Up = _(1);
    private static readonly ast Right = _(2);
    private static readonly ast Down = _(3);
    private static readonly ast Left = _(4);
    private static readonly ast NextDroneMovement = Mem(_(0));
    private static readonly ast NumberOfMapRows = Mem(_(1));
    private static readonly ast NumberOfMapColumns = Mem(_(2));
    private static readonly ast NumberOfObstacles = Mem(_(7));

    private static readonly ast CurrentX = Mem(_(3));
    private static readonly ast CurrentY = Mem(_(4));
    private static readonly ast TargetX = Mem(_(5));
    private static readonly ast TargetY = Mem(_(6));
    private static readonly ast Ret = ast.Return();
    #endregion

    public static ast NaiveGradientDescent() {
      return Ret;
    }

    /// <summary>
    /// Uses the Manhattan (L1) distance
    /// For each adjacent square, calculates the distance to the goal:
    /// d = abs(drone.x - target.x) + abd(drone.y - target.y)
    /// </summary>
    /// <returns></returns>
    public static ast Manhattan() {
      var numberOfObstacles = _("NumberOfObstacles");
      var numberOfCitizens = _("numberOfCitizens");
      var numberOfDrones = _("numberOfDrones");

      var i = _("i");
      var j = _("j");
      var x = _("x");
      var y = _("y");
      var dx = _("dx");
      var dy = _("dy");
      var xmin = _("xmin");
      var ymin = _("ymin");
      var xmax = _("xmax");
      var ymax = _("ymax");
      var currentMap = (AstVariableNode)_("currentMap", 2500);
      var obstacleStartMemoryIndex = _(8);
      var citizenStartMemoryIndex = _("citizenStartMemoryIndex");
      var droneStartMemoryIndex = _("droneStartMemoryIndex");
      ast min = _("min"), left = _("left"), right = _("right"), down = _("down"), up = _("up"); // for the 4 squares surrounding the current position
      ast cxMinus1 = _("cxMinus1"), cxPlus1 = _("cxPlus1"), cyMinus1 = _("cyMinus1"), cyPlus1 = _("cyPlus1");

      var citizenNumericMapValue = _(2500);
      var droneNumericMapValue = _(2500);
      var obstacleNumericMapValue = _(2500);

      var clearMap = _(
        Assign(i, _(0)),
        While(i < currentMap.Size,
          ArraySet(currentMap, i, _(0)),
          Inc(i)
        )
      );

      var calculateL1Distance = _(
        // assign values to the surrounding squares(left, right, up, down) as the L1 distance
        Assign(dx, Abs(CurrentX - TargetX)),
        Assign(dy, Abs(CurrentY - TargetY)),
        Assign(cxMinus1, CurrentX - 1),
        Assign(cxPlus1, CurrentX + 1),
        Assign(cyMinus1, CurrentY - 1),
        Assign(cyPlus1, CurrentY + 1),
        Assign(left, _(2500)),
        Assign(right, _(2500)),
        Assign(up, _(2500)),
        Assign(down, _(2500)),
        If(CurrentX > 0, _(
          Assign(left, dy + Abs(cxMinus1 - TargetX)),
          ArraySet(currentMap, cxMinus1 * NumberOfMapColumns + CurrentY, left)
        )),
        If(CurrentY > 0, _(
          Assign(up, dx + Abs(cyMinus1 - TargetY)),
          ArraySet(currentMap, CurrentX * NumberOfMapColumns + cyMinus1, up)
        )),
        If(cxPlus1 < NumberOfMapColumns, _(
          Assign(right, dy + Abs(cxPlus1 - TargetX)),
          ArraySet(currentMap, cxPlus1 * NumberOfMapColumns + CurrentY, right)
        )),
        If(cyPlus1 < NumberOfMapRows, _(
          Assign(down, dx + Abs(cyPlus1 - TargetY)),
          ArraySet(currentMap, CurrentX * NumberOfMapColumns + cyPlus1, down)
        ))
      );

      var putObjectsOnMap = _(
        // put obstacles on map
        Assign(i, obstacleStartMemoryIndex),
        Assign(j, i + 2 * NumberOfObstacles),
        While(i < j,
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          ArraySet(currentMap, x * NumberOfMapColumns + y, obstacleNumericMapValue),
          Assign(i, i + 2)
        ),
        // put citizens on map
        Assign(numberOfCitizens, Mem(i)),
        Inc(i), // skip memory pos holding numberOfCitizens
        Assign(j, i + 2 * numberOfCitizens),
        While(i < j,
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          // calculate the area of the citizens and set all values to 2500
          Assign(xmin, Max(_(0), x - 3)),
          Assign(xmax, Min(NumberOfMapColumns, x + 3)),
          Assign(ymin, Max(_(0), y - 3)),
          Assign(ymax, Min(NumberOfMapColumns, y + 3)),
          While(xmin < xmax,
            Assign(j, ymin),
            While(j < ymax,
              ArraySet(currentMap, xmin * NumberOfMapColumns + j, citizenNumericMapValue),
              Inc(j)
            ),
            Inc(xmin)
          ),
          Assign(i, i + 2)
        ),
        // put drones on map
        Assign(numberOfDrones, Mem(i)),
        Inc(i), // skip memory pos holding numberOfDrones 
        Assign(j, i + 2 * numberOfDrones),
        Assign(droneStartMemoryIndex, i),
        While(i < 2 * numberOfDrones,
          Assign(x, Mem(i)),
          Assign(y, Mem(i + 1)),
          ArraySet(currentMap, x * NumberOfMapColumns + y, droneNumericMapValue),
          Assign(i, i + 2)
        )
      );

      var chooseDirection = _(
        // all values set, now make moves based on manhattan distance
        If(CurrentX > 0, _(
          Assign(left, ArrayGet(currentMap, cxMinus1 * NumberOfMapColumns + CurrentY))
        )),
        If(CurrentY > 0, _(
          Assign(up, ArrayGet(currentMap, CurrentX * NumberOfMapColumns + cyMinus1))
        )),
        If(CurrentX < NumberOfMapColumns - 1, _(
          Assign(right, ArrayGet(currentMap, cxPlus1 * NumberOfMapColumns + CurrentY))
        )),
        If(CurrentY < NumberOfMapRows - 1, _(
          Assign(down, ArrayGet(currentMap, CurrentX * NumberOfMapColumns + cyPlus1))
        )),
      // pick square with lowest value as next move
        Assign(min, Min(left, right, up, down)),
        If(min == 2500, _(
          Mem(_(0), Hold),
          Ret
        )),
        If(left == min & CurrentX > 0, _(
          Mem(_(0), Left),
          Ret
        )),
        If(right == min & cxPlus1 < NumberOfMapColumns, _(
          Mem(_(0), Right),
          Ret
        )),
        If(up == min & CurrentY > 0, _(
          Mem(_(0), Up),
          Ret
        )),
        If(down == min & cyPlus1 < NumberOfMapRows, _(
          Mem(_(0), Down),
          Ret
        )),
        Mem(_(0), Hold)
      );

      return _(
          clearMap,
          calculateL1Distance,
          putObjectsOnMap,
          chooseDirection,
          Ret
      );
    }

    public static ast Test() {
      //      var i = _("i");
      //      var j = _("j");
      //      var k = _("k");
      //
      //      var innerK = Block(
      //        Mem(_(0), Mem(_(0)) + 1),
      //        Inc(k)
      //      );
      //      var innerJ = Block(
      //        //        Assign(k, _(0)),
      //        Do(k < 3, innerK),
      //        Inc(j)
      //      );
      //      var innerI = Block(
      //        //        Assign(j, _(0)),
      //        Do(j < 3, innerJ),
      //        Inc(i)
      //      );
      //      return Block(Do(i < 3, innerI), Ret);
      return Block(Mem(_(0), _(2) == _(3)), Ret);
    }
  }
}
