using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using DroneFlightPath;

namespace DroneFlightPathUI {
  public partial class MapView : Form {
    private static readonly Dictionary<ObjectType, string> ObjectLabels = new Dictionary<ObjectType, string> {
      { ObjectType.Obstacle, "O" },
      { ObjectType.Drone, "D" },
      { ObjectType.Citizen, "C" }
    };

    private const int SquareSize = 30;

    private Map map;

    public Map Map {
      get { return map; }
      set {
        if (map == value) return;
        map = value;
        DrawMap();
        DrawObjects();
      }
    }

    public MapView() {
      InitializeComponent();

      var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\DroneFlightPath\Maps\01_letsGetToKnowEachOther.txt"));
      var m = MapUtil.ImportMap(path);
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

    private void DrawObjects() {
      var font = new Font(FontFamily.GenericSansSerif, 8);
      var brush = new SolidBrush(Color.Black);
      SizeF size;
      string text;
      Rectangle rectangle;
      using (var g = Graphics.FromImage(pictureBox.Image)) {
        foreach (var o in map.ActiveObjects) {
          var x = o.Position.X * SquareSize;
          var y = o.Position.Y * SquareSize;
          g.SmoothingMode = SmoothingMode.HighQuality;
          g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
          rectangle = new Rectangle(x, y, SquareSize, SquareSize);
          DrawRectangle(g, rectangle, Pens.Black, new SolidBrush(Color.Red));
          text = ObjectLabels[o.Type];
          size = g.MeasureString(text, font);
          g.DrawString(text, font, brush, rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + (rectangle.Height - size.Height) / 2);
        }
        text = "@";
        size = g.MeasureString(text, font);
        rectangle = new Rectangle(map.Drone.Position.X * SquareSize, map.Drone.Position.Y * SquareSize, SquareSize, SquareSize);
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

    private void button_Load_Click(object sender, System.EventArgs e) {
      var ofd = new OpenFileDialog {
        InitialDirectory = "c:\\",
        Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
        FilterIndex = 2,
        RestoreDirectory = true
      };
      if (ofd.ShowDialog() == DialogResult.OK) {
        try {
          var path = Path.Combine(ofd.InitialDirectory, ofd.FileName);
          var m = MapUtil.ImportMap(path);
          Map = m;
        }
        catch (Exception ex) {
          MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }
    }
  }
}
