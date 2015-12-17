using System;

namespace DroneFlightPath {
  public class Test {

    public static void TestCit() {
      var curr = new int[Rows * Cols];
      var last = new int[Rows * Cols];
      var diff = new int[Rows * Cols];

      var c = new Point(8, 8);
      Update(last, c);

      Console.WriteLine();
      c = new Point(8, 9);
      Update(curr, c);
      //Print(curr);
      var d = Diff(curr, last);
      Update(d, c);
      Print(d);
    }

    private static int Cols = 20;
    private static int Rows = 20;
    private static int cVal = 2500;

    static int Get(int[] mat, int row, int col) { return mat[row * Cols + col]; }
    static void Set(int[] mat, int row, int col, int value) { mat[row * Cols + col] = value; }

    static int[] Diff(int[] curr, int[] last) {
      var diff = new int[Rows * Cols];
      for (int i = 0; i < Rows; ++i) {
        for (int j = 0; j < Cols; ++j) {
          var vCurr = Get(curr, i, j);
          var vLast = Get(last, i, j);
          var v = vCurr < vLast ? vCurr - vLast : vCurr;
          Set(diff, i, j, v);
        }
      }

      return diff;
    }

    static void Update(int[] mat, Point c) {
      var xmin = Math.Max(0, c.X - 3);
      var xmax = Math.Min(Cols, c.X + 4);
      var ymin = Math.Max(0, c.Y - 3);
      var ymax = Math.Min(Rows, c.Y + 4);

      int i = xmin;
      int j = ymin;
      for (i = xmin; i < xmax; ++i) {
        for (j = ymin; j < ymax; ++j) {
          Set(mat, i, j, cVal);
        }
      }
      Set(mat, c.X, c.Y, 3000);

      Console.WriteLine("c.X: " + c.X);
      Console.WriteLine("c.Y: " + c.Y);

      // citizen moved right
      if (c.X - 3 > 0) {
        var v = Get(mat, c.X - 4, c.Y);
        if (v == -cVal) {
          int y = ymin;
          while (y < ymax) {
            Set(mat, c.X - 4, y, 0);
            ++y;
          }
          var min = c.X + 4;
          var max = Math.Min(min + 4, Cols);
          int x = min;
          while (x < max) {
            y = ymin;
            while (y < ymax) {
              Set(mat, x, y, cVal);
              ++y;
            }
            ++x;
            ymin++;
            ymax--;
          }
        }
      }
      // citizen moved left
      if (c.X + 4 < Cols) {
        var v = Get(mat, c.X + 4, c.Y);
        if (v == -cVal) {
          int y = ymin;
          while (y < ymax) {
            Set(mat, c.X + 4, y, 0);
            ++y;
          }
          var min = Math.Max(0, c.X - 7);
          var max = c.X - 3;
          int x = min;
          while (x < max) {
            y = ymin;
            while (y < ymax) {
              Set(mat, max - x - 1, y, cVal - x - 1);
              ++y;
            }
            ymin++;
            ymax--;
            ++x;
          }
        }
      }
      // citizen moved down
      if (c.Y - 3 > 0) {
        var v = Get(mat, c.X, c.Y - 4);
        if (v == -cVal) {
          int x = xmin;
          while (x < xmax) {
            Set(mat, x, c.Y - 4, 0);
            ++x;
          }
          var min = c.Y + 4;
          var max = Math.Min(min + 4, Rows);
          int y = min;
          while (y < max) {
            x = xmin;
            while (x < xmax) {
              Set(mat, x, y, cVal + min - y - 1);
              ++x;
            }
            ++y;
            xmin++;
            xmax--;
          }
        }
      }
      // citizen moved up
      if (c.Y + 4 < Rows) {
        var v = Get(mat, c.X, c.Y + 4);
        if (v == -cVal) {
          int x = xmin;
          while (x < xmax) {
            Set(mat, x, c.Y + 4, 0);
            ++x;
          }
          var min = Math.Max(0, c.Y - 7);
          var max = c.Y - 3;
          int y = min;
          while (y < max) {
            x = xmin;
            while (x < xmax) {
              Set(mat, x, max - y - 1, cVal - y - 1);
              ++x;
            }
            ++y;
            xmin++;
            xmax--;
          }
        }
      }
    }

    struct Point {
      public Point(int x, int y) {
        X = x;
        Y = y;
      }

      public int X;
      public int Y;
    }

    static void Print(int[] mat) {
      for (int y = 0; y < Rows; ++y) {
        for (int x = 0; x < Cols; ++x) {
          Console.Write(Get(mat, x, y) + " ");
        }
        Console.WriteLine();
      }
    }
  }
}
