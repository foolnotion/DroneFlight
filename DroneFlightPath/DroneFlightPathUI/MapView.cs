using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeInterpreter;
using CodeInterpreter.AST;
using DroneFlightPath;
using DroneFlightPathUI.Models;

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

    private const int SquareSize = 30;

    private Map map;
    private BindingSource watchDataSource;
    private double mapWeight;

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
      m.Machine = new RegisterMachine(1000000);
      var block = Strategy.Manhattan();
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);
      m.Machine.MemoryMap = genVisitor.MemoryMap;
      m.Machine.LoadIntructions(genVisitor.Code);
      Map = m;
      InitWatchDataView();
      mapWeight = 0.01;
    }

    private void InitWatchDataView() {
      watchDataSource = new BindingSource();

      watchDataSource.Add(new VariableModel() { Name = "A", Value = 0 });
      watchDataSource.Add(new VariableModel() { Name = "N", Value = 0 });


      watchDataView.CellClick += WatchDataView_CellClick;
      watchDataView.CellEndEdit += WatchDataView_CellEndEdit;
      watchDataView.CellValidating += WatchDataView_CellValidating;

      watchDataView.AutoGenerateColumns = false;
      watchDataView.AutoSize = false;
      watchDataView.DataSource = watchDataSource;


      var nameColumn = new DataGridViewTextBoxColumn();
      nameColumn.DataPropertyName = "Name";
      nameColumn.Name = "Variable name";
      nameColumn.Width = 100;
      watchDataView.Columns.Add(nameColumn);

      var valueColumn = new DataGridViewTextBoxColumn();
      valueColumn.DataPropertyName = "Value";
      valueColumn.Name = "Value";
      valueColumn.ReadOnly = true;
      valueColumn.Width = 100;
      watchDataView.Columns.Add(valueColumn);


      var deleteColumn = new DataGridViewButtonColumn();
      deleteColumn.Text = "X";
      deleteColumn.UseColumnTextForButtonValue = true;
      deleteColumn.Width = 30;
      deleteColumn.Name = "deleteColumn";
      deleteColumn.HeaderText = "";
      watchDataView.Columns.Add(deleteColumn);

    }

    private void WatchDataView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
      string columnName = watchDataView.Columns[e.ColumnIndex].Name;

      if (!columnName.Equals("Variable name"))
        return;


      if (!VariableNameIsCorrect(e.FormattedValue.ToString())) {
        watchDataView.Rows[e.RowIndex].ErrorText =
            "Enter A, N or an interger value";
        e.Cancel = true;
      }
    }

    private bool VariableNameIsCorrect(string variableName) {
      if (variableName == "A" || variableName == "N")
        return true;
      int parsedArgument = 0;
      if (int.TryParse(variableName, out parsedArgument)) {
        return true;
      }
      return false;
    }

    private void WatchDataView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {

      if (e.RowIndex < 0)
        return;
      watchDataView.Rows[e.RowIndex].ErrorText = String.Empty;

      var row = watchDataView.CurrentRow;
      var valueCell = row.Cells[1];
      if (row.Cells[0].Value == null) {
        return;
      }
      var variableName = row.Cells[0].Value.ToString();
      valueCell.Value = GetMemoryValueForVariableName(variableName);
    }

    private int GetMemoryValueForVariableName(string variableName) {
      if (variableName == "A") {
        return machine.A;
      }

      if (variableName == "N") {
        return machine.N;
      }

      int memAddress = int.Parse(variableName);
      return machine.Memory[memAddress];

    }
    private void WatchDataView_CellClick(object sender, DataGridViewCellEventArgs e) {
      //if click is on new row or header row
      if (e.RowIndex == watchDataView.NewRowIndex || e.RowIndex < 0)
        return;

      //Handle Button Column Click
      if (e.ColumnIndex == watchDataView.Columns["deleteColumn"].Index) {
        watchDataView.Rows.Remove(watchDataView.Rows[e.RowIndex]);
      }
    }

    private void UpdateWatchVariables() {
      foreach (DataGridViewRow row in watchDataView.Rows) {
        if (row.Cells[0].Value != null) {
          var variableName = row.Cells[0].Value.ToString();
          var oldValue = (int)row.Cells[1].Value;
          var newValue = GetMemoryValueForVariableName(variableName);
          row.Cells[1].Value = newValue;
          if (newValue == oldValue) {
            row.Cells[1].Style = new DataGridViewCellStyle { ForeColor = Color.Black };
          } else {
            row.Cells[1].Style = new DataGridViewCellStyle { ForeColor = Color.Red };
          }
        }
      }
    }

    private void UpdateRunInfo() {
      var steps = map.TimeStep;
      var cycles = machine.Cycles;
      scoreValueLabel.Text = (mapWeight * 1e5 / Math.Log(steps * steps * cycles)).ToString(".##");
      cyclesValueLabel.Text = cycles.ToString();
      stepsValueLabel.Text = steps.ToString();
    }

    private void ClearRunInfo() {
      scoreValueLabel.Text = "0";
      cyclesValueLabel.Text = "0";
      stepsValueLabel.Text = "0";
    }

    private void ClearWatchVariables() {
      foreach (DataGridViewRow row in watchDataView.Rows) {
        if (row.Cells[0].Value != null) {
          row.Cells[1].Value = 0;
        }
      }
    }

    public void DrawMap() {
      var bitmap = new Bitmap(map.Rows * SquareSize + 1, map.Cols * SquareSize + 1);

      var font = new Font(FontFamily.GenericMonospace, 8);
      using (var g = Graphics.FromImage(bitmap)) {
        g.SmoothingMode = SmoothingMode.HighQuality;
        for (int i = 0; i < map.Rows; ++i) {
          for (int j = 0; j < map.Cols; ++j) {
            int x = i * SquareSize;
            int y = j * SquareSize;
            var rectangle = new Rectangle(x, y, SquareSize, SquareSize);
            DrawRectangle(g, rectangle, Pens.Gray, new SolidBrush(Color.White));
            try {
              var currentMap = machine.MemoryMap["mapDiff"];
              g.DrawString(machine.Memory[currentMap + i * map.Cols + j].ToString(), font, Brushes.Black, rectangle);
            }
            catch (Exception e) {

            }
            if (i % 5 == 0 && j % 5 == 0) {
              rectangle = new Rectangle(x - 1, y - 1, 2, 2);
              DrawRectangle(g, rectangle, Pens.Black, Brushes.Black);
            }
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
          //g.DrawString(text, font, brush, rectangle.X + (rectangle.Width - size.Width) / 2, rectangle.Y + (rectangle.Height - size.Height) / 2);

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
          Map.Machine = new RegisterMachine();
          machine = Map.Machine;
          machine.LoadIntructions(code);
          ClearWatchVariables();
          ClearRunInfo();
        }
        catch (Exception ex) {
          MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }


    }

    private void button_Step_Click(object sender, EventArgs e) {
      try {
        map.RunPathFinding();
        Draw();
        Map.Step();
        UpdateWatchVariables();
        UpdateRunInfo();
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
          mapWeight = 0.01;
          break;
        case 1:
          res = Properties.Resources._02_dontGetShot;
          mapWeight = 0.02;
          break;
        case 2:
          res = Properties.Resources._03_shortestPath;
          mapWeight = 0.03;
          break;
        case 3:
          res = Properties.Resources._04_gottaCircleAround;
          mapWeight = 0.04;
          break;
        case 4:
          res = Properties.Resources._05_thinkAhead;
          mapWeight = 0.05;
          break;
        case 5:
          res = Properties.Resources._06_beOnYourToes;
          mapWeight = 0.06;
          break;
        default:
          throw new Exception("Unknown resource index");
      }
      Map = MapUtil.Load(res);
      Map.Machine = new RegisterMachine(1000000);
      machine = Map.Machine;
      var block = Strategy.Manhattan();
      var mmapVisitor = new MapObjectsToMemoryVisitor();
      block.Accept(mmapVisitor);
      var genVisitor = new GenerateAsmVisitor(mmapVisitor.MemoryMap);
      block.Accept(genVisitor);
      machine.MemoryMap = genVisitor.MemoryMap;
      machine.LoadIntructions(genVisitor.Code);
    }
  }
}
