using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DroneFlightPath {
  public enum Direction : byte {
    HOLD, UP, RIGHT, DOWN, LEFT
  }

  public enum ObjectType : byte { OBSTACLE, DRONE, CITIZEN }

  // a fixed obstacle
  public class Object {
    public ObjectType Type { get; private set; }
    public string Identifier { get; private set; }
    public Point Position { get; private set; }
    public int Step { get; private set; } // not really necessary for fixed obstacles but then step=0 meh
    public Direction Direction { get; private set; }
    public int Range { get; private set; }
    public bool IsActive { get; set; }
    private Object() { }
    public Object(ObjectType type, string id, Point position) {
      Step = 0;
      Identifier = id;
      Position = position;
      Type = type;
      IsActive = true;
    }
    public Object(ObjectType type, string id, Point position, int step) : this(type, id, position) {
      Step = step;
    }
    public Object(ObjectType type, string id, Point position, int step, Direction direction) : this(type, id, position, step) {
      Direction = direction;
    }
    public Object(ObjectType type, string id, Point position, int step, Direction direction, int range) : this(type, id, position, step, direction) {
      Range = range;
    }
    public static Object CreateObstacle(string id, Point position) {
      return new Object(ObjectType.OBSTACLE, id, position);
    }
    public static Object CreateDrone(string id, Point position, int step, Direction direction) {
      return new Object(ObjectType.CITIZEN, id, position, step, direction);
    }
    public static Object CreateCitizen(string id, Point position, int step, Direction direction, int range) {
      return new Object(ObjectType.CITIZEN, id, position, step, direction, range);
    }
  }

  public class Map {
    private List<Object> Objects;
    public IEnumerable<Object> Obstacles { get { return Objects.Where(x => x.IsActive && x.Step <= step && x.Type == ObjectType.OBSTACLE); } }
    public IEnumerable<Object> Drones { get { return Objects.Where(x => x.IsActive && x.Step <= step && x.Type == ObjectType.DRONE); } }
    public IEnumerable<Object> Citizens { get { return Objects.Where(x => x.IsActive && x.Step <= step && x.Type == ObjectType.CITIZEN); } }
    private int step;
    public int Rows { get; set; }
    public int Cols { get; set; }
    public Point Target { get; set; }
    public Object Drone { get; set; }

    public void Step() {
      ++step;
      UpdateObjectPositions();
      CheckObjectCollisions();
    }

    private void UpdateObjectPositions() {

    }

    private void CheckObjectCollisions() {

    }

    public void AddObject(Object @object) {
      var o = Objects.FirstOrDefault(x => x.Position == @object.Position);
      if (o != null)
        throw new ArgumentException(string.Format("Cannot add object {0} at position {1} as it would overlap with existing object {2}", @object.Identifier, o.Position, o.Identifier));
      Objects = Objects.Where(x => x.IsActive).ToList();
      Objects.Add(@object); // order doesn't matter
    }

    private Map() { }

    public Map(IEnumerable<Object> objects, Point target, int rows, int cols) {
      Objects = objects.ToList();
      Target = target;
      Rows = rows;
      Cols = cols;
    }
  }
}
