﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DroneFlightPath {
  public enum Direction : byte {
    Hold, Up, Right, Down, Left
  }
  public enum ObjectType : byte { Obstacle, Drone, Citizen }
  public enum ControllerType : byte { Target, Direction }

  // a fixed obstacle
  public abstract class Object {
    public Map Map { get; set; }
    public ObjectType Type { get; set; }
    public string Id { get; set; }
    public Point Position { get; set; }
    public int Step { get; set; }

    private Object() { }

    protected Object(ObjectType type, Map map, Point position, string id) {
      Map = map;
      Id = id;
      Position = position;
      Type = type;
    }

    public virtual void Move() { }
    public bool IsStillOnMap => Position.X >= 0 && Position.Y >= 0 && Position.X < Map.Rows && Position.Y < Map.Cols;
    public bool IsDead { get; set; }

    public virtual bool CollidesWith(Object other) {
      if (other.Type == ObjectType.Citizen)
        return other.CollidesWith(this);
      return Position.X == other.Position.X && Position.Y == other.Position.Y;
    }
  }

  public class Obstacle : Object {
    public Obstacle(Map map, Point position, string id) : base(ObjectType.Obstacle, map, position, id) { }
  }

  public abstract class MovingObject : Object {
    public Direction Direction { get; set; }

    protected MovingObject(ObjectType type, Map map, Point position, string id, Direction d) : base(type, map, position, id) {
      Direction = d;
    }

    public override void Move() {
      if (Type == ObjectType.Obstacle) return;
      int x = Position.X;
      int y = Position.Y;
      switch (Direction) {
        case Direction.Up:
          y--;
          break;
        case Direction.Down:
          y++;
          break;
        case Direction.Left:
          x--;
          break;
        case Direction.Right:
          x++;
          break;
        default:
          throw new InvalidOperationException($"Unsupported direction {Direction}");
      }
      Position = new Point(x, y);
    }
  }

  public class Drone : MovingObject {
    public ControllerType ControllerType { get; set; }
    public Point Target { get; set; }

    public Drone(Map map, Point position, string id, ControllerType controllerType, Point target, Direction d) : base(ObjectType.Drone, map, position, id, d) {
      ControllerType = controllerType;
      Target = target;
    }

    public override void Move() {
      if (ControllerType != ControllerType.Target)
        base.Move();
    }
  }

  public class Citizen : MovingObject {
    public Citizen(Map map, Point position, string id, Direction d) : base(ObjectType.Citizen, map, position, id, d) { }

    public override bool CollidesWith(Object other) {
      return 3 >= Math.Max(Math.Abs(Position.X - other.Position.X), Math.Abs(Position.Y - other.Position.Y));
    }
  }

  public class Map {
    private List<Object> objects;
    public IEnumerable<Object> ActiveObjects { get { return objects.Where(x => x.Step <= TimeStep); } }
    //    public IEnumerable<Object> Obstacles { get { return ActiveObjects.Where(x => x.Type == ObjectType.Obstacle); } }
    //    public IEnumerable<Object> Drones { get { return ActiveObjects.Where(x => x.Type == ObjectType.Drone); } }
    //    public IEnumerable<Object> Citizens { get { return ActiveObjects.Where(x => x.Type == ObjectType.Citizen); } }
    public int TimeStep { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public Point Target { get; set; }
    public Object Drone { get; set; }

    public void MoveUp() { Drone.Position = new Point(Drone.Position.X, Drone.Position.Y - 1); }
    public void MoveDown() { Drone.Position = new Point(Drone.Position.X, Drone.Position.Y + 1); }
    public void MoveLeft() { Drone.Position = new Point(Drone.Position.X - 1, Drone.Position.Y); }
    public void MoveRight() { Drone.Position = new Point(Drone.Position.X + 1, Drone.Position.Y); }

    public void Step() {
      Drone.Move();
      UpdateObjectPositions();
      ++TimeStep;
      ResolveCollisions();
    }

    private void UpdateObjectPositions() {
      // update all object positions
      foreach (var o in ActiveObjects.Where(x => x.Type != ObjectType.Obstacle)) {
        o.Move();
      }
      // remove objects that have already exited the map
      objects = objects.Where(x => x.Type != ObjectType.Obstacle && x.IsStillOnMap).ToList();
    }

    private void ResolveCollisions() {
      var activeObjects = ActiveObjects.ToList();
      for (int i = 0; i < activeObjects.Count; ++i) {
        var oi = activeObjects[i];
        if (oi.Type == ObjectType.Drone) continue;
        for (int j = 0; j < activeObjects.Count; ++j) {
          if (i == j) continue;
          var oj = activeObjects[j];
          if (oj.Type != ObjectType.Drone) continue;
          oj.IsDead = oj.CollidesWith(oi);
        }
      }
      objects = objects.Where(x => !x.IsDead).ToList();
    }

    public void AddObject(Object obj) {
      var p = obj.Position;
      if (!(p.X >= 0 && p.X < Rows && p.Y >= 0 && p.Y < Cols))
        throw new ArgumentException($"Position {obj.Position} of object {obj.Id} is outside the map.");
      var o = objects.FirstOrDefault(x => x.Position == obj.Position);
      if (o != null)
        throw new ArgumentException($"Cannot add object {obj.Id} at position {obj.Position} since it would overlap with already existing object {o.Id}");
      obj.Map = this;
      objects.Add(obj);
    }
    public void AddObjects(IEnumerable<Object> objects) {
      foreach (var o in objects) AddObject(o);
    }
    private Map() { }
    public Map(Point target, Point dronePosition, int rows, int cols) {
      Target = target;
      Rows = rows;
      Cols = cols;
      Drone = new Drone(this, new Point(dronePosition.X, dronePosition.Y), "@", ControllerType.Target, new Point(Target.X, target.Y), Direction.Hold);
      this.objects = new List<Object>();
    }
    public Map(IEnumerable<Object> objects, Point target, Point drone, int rows, int cols) : this(target, drone, rows, cols) {
      AddObjects(objects);
    }
  }
}
