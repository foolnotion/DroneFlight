using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

    public static Map ImportMap(string path) {
      var jsonString = File.ReadAllText(path);
      var data = JsonConvert.DeserializeObject<JObject>(jsonString);

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
      foreach (var o in objects) {
        var type = (string)o["type"];
        Object obj;
        switch (type) {
          case "Obstacle": {
              obj = new Obstacle(map, new Point((int)o["position"]["x"], (int)o["position"]["y"]), "");
              break;
            }
          case "Cetatean": {
              var id = (string)o["identifier"];
              var dir = (string)o["direction"];
              obj = new Citizen(map, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, Directions[dir]);
              break;
            }
          case "Drone": {
              var id = (string)o["identifier"];
              var controller = o["controller"];
              var ct = ControllerTypes[(string)controller["type"]];

              switch (ct) {
                case ControllerType.Target: {
                    var t = controller["target"];
                    obj = new Drone(map, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, ct, new Point((int)t["x"], (int)t["y"]), Direction.Hold);
                    break;
                  }
                case ControllerType.Direction: {
                    var d = (string)controller["direction"];
                    obj = new Drone(map, new Point((int)o["position"]["x"], (int)o["position"]["y"]), id, ct, new Point(0, 0), Directions[d]);
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
        map.AddObject(obj);
      }
      return map;
    }
  }
}
