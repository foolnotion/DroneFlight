using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CodeInterpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DroneFlightPath {
  public static class MapUtil {
    private static readonly Dictionary<string, Direction> Directions = new Dictionary<string, Direction>  {
      { "RIGHT", Direction.Right },
      { "LEFT", Direction.Left },
      { "UP", Direction.Up },
      { "DOWN", Direction.Down },
      { "HOLD", Direction.Hold }
    };

    private static readonly Dictionary<string, ControllerType> ControllerTypes = new Dictionary<string, ControllerType> {
      { "Direction", ControllerType.Direction},
      { "Target", ControllerType.Target }
    };

    public static Map Load(string data) {
      return Load(JsonConvert.DeserializeObject<JObject>(data));
    }

    private static IEnumerable<Object> ReadObjects(JArray objects) {
      foreach (var o in objects) {
        var type = (string)o["type"];
        Object obj;
        switch (type) {
          case "Obstacle": {
              obj = new Obstacle(null, new Point((int)o["position"]["x"], (int)o["position"]["y"]), "");
              break;
            }
          case "Cetatean": {
              var id = (string)o["identifier"];
              var dir = (string)o["direction"];
              obj = new Citizen(null, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, Directions[dir]);
              break;
            }
          case "Drone": {
              var id = (string)o["identifier"];
              var controller = o["controller"];
              var ct = ControllerTypes[(string)controller["type"]];

              switch (ct) {
                case ControllerType.Target: {
                    var t = controller["target"];
                    obj = new Drone(null, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, ct, new Point((int)t["x"], (int)t["y"]), Direction.Hold);
                    break;
                  }
                case ControllerType.Direction: {
                    var d = (string)controller["direction"];
                    obj = new Drone(null, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, ct, new Point(0, 0), Directions[d]);
                    break;
                  }
                default:
                  throw new InvalidDataException($"Unknown controller type {controller["type"]}");
              }
            }
            break;
          default:
            throw new InvalidDataException($"Unknown object type {type}");
        }
        yield return obj;
      }
    }

    private static Map Load(JObject data) {
      var drone = data["simulatedDrone"];
      var pos = drone["position"];
      var dronePosition = new Point((int)pos["x"], (int)pos["y"]);
      var mapData = data["map"];
      var rows = (int)mapData["rows"];
      var cols = (int)mapData["cols"];
      var target = mapData["target"];
      var targetPosition = new Point((int)target["x"], (int)target["y"]);

      var map = new Map(targetPosition, dronePosition, rows, cols);
      var objects = (JArray)mapData["objects"];
      map.AddObjects(ReadObjects(objects));

      var steps = (JArray)data["steps"];
      if (steps == null)
        return map;

      foreach (var step in steps) {
        var stepNumber = (int)step["stepNumber"];
        objects = (JArray)step["objects"];
        foreach (var o in ReadObjects(objects)) {
          o.Step = stepNumber;
          map.AddObject(o);
        }
      }
      return map;
    }

    public static Map LoadPath(string path) {
      var jsonString = File.ReadAllText(path);
      var data = JsonConvert.DeserializeObject<JObject>(jsonString);
      return Load(data);
    }

    public struct MoveInstr {
      public MoveInstr(Direction d, int s) {
        D = d;
        S = s;
      }
      public Direction D; // which direction
      public int S; // how many steps
    }

    public static IEnumerable<Instruction> GenerateMachineCode(MoveInstr[] moveInstructions) {
      yield return new Instruction(OpCode.Lda, new Arg(ArgType.Value, 1000, true));
      //      int end = 1 + (moveInstructions.Length - 1) * 5 + 2;
      int end = 1 + (moveInstructions.Length - 1) * 4 + 1;
      for (int i = 0; i < moveInstructions.Length - 1; ++i) {
        var mi = moveInstructions[i];
        //var jgeAddr = 1 + (i + 1) * 5;
        var jgeAddr = 1 + (i + 1) * 4;
        yield return new Instruction(OpCode.Suba, new Arg(ArgType.Value, mi.S, false));
        yield return new Instruction(OpCode.Jge, new Arg(ArgType.Value, jgeAddr, false));
        yield return new Instruction(OpCode.Lda, new Arg(ArgType.Value, (int)mi.D, false));
        //yield return new Instruction(OpCode.Sta, new Arg(ArgType.Value, 0, true));
        yield return new Instruction(OpCode.Jge, new Arg(ArgType.Value, end, false));
      }
      var li = moveInstructions[moveInstructions.Length - 1];
      yield return new Instruction(OpCode.Lda, new Arg(ArgType.Value, (int)li.D, false));
      yield return new Instruction(OpCode.Sta, new Arg(ArgType.Value, 0, true));
      yield return new Instruction(OpCode.Lda, new Arg(ArgType.Value, 1000, true));
      yield return new Instruction(OpCode.Adda, new Arg(ArgType.Value, 1, false));
      yield return new Instruction(OpCode.Sta, new Arg(ArgType.Value, 1000, true));
      yield return new Instruction(OpCode.Hlt, new Arg(ArgType.Value, 0, false));
    }
  }
}
