using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeInterpreter;
using DroneFlightPath;

namespace DroneFlightPathUI {
  public partial class MapView : Form {
    private static readonly Dictionary<ObjectType, string> ObjectLabels = new Dictionary<ObjectType, string> {
      {ObjectType.Obstacle, "O"},
      {ObjectType.Drone, "D"},
      {ObjectType.Citizen, "C"}
    };

    private static readonly Dictionary<ObjectType, Color> ObjectColors = new Dictionary<ObjectType, Color> {
      {ObjectType.Obstacle, Color.DimGray},
      {ObjectType.Drone, Color.Orange},
      {ObjectType.Citizen, Color.IndianRed}
    };

    private const int SquareSize = 25;

    private Map map;

    public Map Map {
      get { return map; }
      set {
        if (map == value) return;
        map = value;
        Draw();
      }
    }

    private RegisterMachine machine = new RegisterMachine(1000000);

    public MapView() {
      InitializeComponent();

      var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
          @"..\..\..\DroneFlightPath\Maps\01_letsGetToKnowEachOther.txt"));
      var m = MapUtil.LoadPath(path);
      Map = m;
    }

    public void DrawMap() {
      var bitmap = new Bitmap(map.Rows * SquareSize + 1, map.Cols * SquareSize + 1);
      using (var g = Graphics.FromImage(bitmap)) {
        g.SmoothingMode = SmoothingMode.HighQuality;
        for (int i = 0; i < map.Rows; ++i) {
          for (int j = 0; j < map.Cols; ++j) {
            int x = i * SquareSize;
            int y = j * SquareSize;
            var rectangle = new Rectangle(x, y, SquareSize, SquareSize);
            DrawRectangle(g, rectangle, Pens.Black, new SolidBrush(Color.White));
          }
        }
      }
      pictureBox.Image = bitmap;
    }

    private void Draw() {
      DrawMap();
      DrawObjects();
    }

    private void DrawObjects() {
      var font = new Font(FontFamily.GenericSansSerif, 10);
      var brush = new SolidBrush(Color.Black);
      using (var g = Graphics.FromImage(pictureBox.Image)) {
        Rectangle rectangle;
        string text;
        SizeF size;
        foreach (var o in map.ActiveObjects) {
          var x = o.Position.X * SquareSize;
          var y = o.Position.Y * SquareSize;
          g.SmoothingMode = SmoothingMode.HighQuality;
          g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
          rectangle = new Rectangle(x, y, SquareSize, SquareSize);
          DrawRectangle(g, rectangle, Pens.Black, new SolidBrush(ObjectColors[o.Type]));
          text = ObjectLabels[o.Type];
          size = g.MeasureString(text, font);
          g.DrawString(text, font, brush, rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + (rectangle.Height - size.Height) / 2);

          if (o.Type == ObjectType.Citizen || o.Type == ObjectType.Drone) {
            var m = (MovingObject)o;
            var drone = o as Drone;
            var citizen = o as Citizen;
            if (drone != null && drone.ControllerType != ControllerType.Direction) continue;
            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            switch (m.Direction) {
              case Direction.Down: {
                  x1 = (float)(x + 0.5 * SquareSize);
                  y1 = (float)(y + 0.5 * SquareSize);
                  x2 = x1;
                  y2 = Math.Min(y1 + SquareSize, map.Rows * SquareSize);
                  break;
                }
              case Direction.Up: {
                  x1 = (float)(x + 0.5 * SquareSize);
                  y1 = (float)(y + 0.5 * SquareSize);
                  x2 = x1;
                  y2 = Math.Max(0, y1 - SquareSize);
                  break;
                }
              case Direction.Left: {
                  x1 = (float)(x + 0.5 * SquareSize);
                  y1 = (float)(y + 0.5 * SquareSize);
                  x2 = Math.Max(0, x1 - SquareSize);
                  y2 = y1;
                  break;
                }
              case Direction.Right: {
                  x1 = (float)(x + 0.5 * SquareSize);
                  y1 = (float)(y + 0.5 * SquareSize);
                  x2 = Math.Min(x1 + SquareSize, map.Cols * SquareSize);
                  y2 = y1;
                  break;
                }
            }
            g.DrawLine(new Pen(Color.Black) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor }, x1, y1, x2, y2);
            if (citizen != null) {
              x1 = Math.Max(SquareSize * (citizen.Position.X - 3), 0);
              y1 = Math.Max(SquareSize * (citizen.Position.Y - 3), 0);
              x2 = Math.Min(map.Rows * SquareSize, (citizen.Position.X + 4) * SquareSize) - x1;
              y2 = Math.Min(map.Cols * SquareSize, (citizen.Position.Y + 4) * SquareSize) - y1;

              rectangle = new Rectangle((int)x1, (int)y1, (int)x2, (int)y2);
              DrawRectangle(g, rectangle, new Pen(ObjectColors[citizen.Type]), new SolidBrush(Color.FromArgb(50, ObjectColors[citizen.Type])));
            }
          }
        }

        text = "@";
        size = g.MeasureString(text, font);
        rectangle = new Rectangle(map.Drone.Position.X * SquareSize, map.Drone.Position.Y * SquareSize, SquareSize,
          SquareSize);
        DrawRectangle(g, rectangle, Pens.Black, Brushes.Gainsboro);
        g.DrawString(text, font, brush, rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + (rectangle.Height - size.Height) / 2);

        text = "T";
        size = g.MeasureString(text, font);
        rectangle = new Rectangle(map.Target.X * SquareSize, map.Target.Y * SquareSize, SquareSize, SquareSize);
        DrawRectangle(g, rectangle, Pens.Black, Brushes.LightGreen);
        g.DrawString(text, font, brush, rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + (rectangle.Height - size.Height) / 2);
      }
    }

    private void DrawRectangle(Graphics g, Rectangle r, Pen pen, Brush brush) {
      g.DrawRectangle(pen, r);
      g.FillRectangle(brush, r.X + 1, r.Y + 1, r.Width - 1, r.Height - 1);
    }

    private void button_LoadMap_Click(object sender, System.EventArgs e) {
      var ofd = new OpenFileDialog {
        InitialDirectory = "c:\\",
        Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
        FilterIndex = 2,
        RestoreDirectory = true
      };
      if (ofd.ShowDialog() == DialogResult.OK) {
        try {
          var path = Path.Combine(ofd.InitialDirectory, ofd.FileName);
          var m = MapUtil.LoadPath(path);
          Map = m;
        }
        catch (Exception ex) {
          MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }
    }

    private void button_LoadSolution_Click(object sender, EventArgs e) {
      var ofd = new OpenFileDialog {
        InitialDirectory = "c:\\",
        Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
        FilterIndex = 2,
        RestoreDirectory = true
      };
      if (ofd.ShowDialog() == DialogResult.OK) {
        try {
          var path = Path.Combine(ofd.InitialDirectory, ofd.FileName);
          var code = RegisterMachineUtil.LoadPath(path).ToArray();
          machine = new RegisterMachine();
          machine.LoadIntructions(code);
        }
        catch (Exception ex) {
          MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }
    }

    private void button_Step_Click(object sender, EventArgs e) {
      try {
        machine.Run();
        var direction = (Direction)machine.Memory[0];
        map.Drone.Direction = direction;
        map.Step();
        Draw();
      }
      catch (Exception exception) {
        MessageBox.Show(exception.Message, "Step", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void mapComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      string res;
      switch (mapComboBox.SelectedIndex) {
        case 0:
          res = Properties.Resources._01_letsGetToKnowEachOther;
          break;
        case 1:
          res = Properties.Resources._02_dontGetShot;
          break;
        case 2:
          res = Properties.Resources._03_shortestPath;
          break;
        case 3:
          res = Properties.Resources._04_gottaCircleAround;
          break;
        case 4:
          res = Properties.Resources._05_thinkAhead;
          break;
        case 5:
          res = Properties.Resources._06_beOnYourToes;
          break;
        default:
          throw new Exception("Unknown resource index");
      }
      Map = MapUtil.Load(res);
      machine = new RegisterMachine();
    }
  }
}
