using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Graph_Plotter
{
  public partial class MainWindow : System.Windows.Window
    {
    public MainWindow()
    {
      InitializeComponent();
      DrawGridLines();
    }

    private void PlotButton_Click(object sender, RoutedEventArgs e)
    {
      string functionText = FunctionInput.Text;

      try
      {
        PlotFunction(functionText);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error: {ex.Message}");
      }
    }

    private void PlotFunction(string expression)
    {
      GraphCanvas.Children.Clear();

      double canvasWidth = GraphCanvas.ActualWidth;
      double canvasHeight = GraphCanvas.ActualHeight;

      DataTable table = new DataTable();
      table.Columns.Add("x", typeof(double));
      table.Columns.Add("y", typeof(double));

      double xScale = canvasWidth /  (2*currentScale); // Adjust the scaling factor as needed
      double yScale = canvasHeight /  (2*currentScale);

      double minX = -canvasWidth / (2 * xScale) + canvasWidth / 2; // Calculate min x based on current scale and canvas width
      double maxX = canvasWidth / (2 * xScale) + canvasWidth / 2; // Calculate max x based on current scale and canvas width

      // Define a margin to extend the range of x-values beyond the visible canvas area
      double marginX = 2000000 / xScale;

      minX -= marginX;
      maxX += marginX;

      for (double x = minX; x <= maxX; x += 1)
      {
        double parsedX = (x - canvasWidth / 2) / xScale; // Apply the scaling factor for X
        double parsedY = EvaluateExpression(expression, parsedX);

        if (!double.IsNaN(parsedY))
        {
          table.Rows.Add(parsedX, parsedY);
        }
      }

      Polyline polyline = new Polyline();
      polyline.Stroke = Brushes.Blue;
      polyline.StrokeThickness = 2;

      foreach (DataRow row in table.Rows)
      {
        double x = (double)row["x"] * xScale + canvasWidth / 2; // Apply the scaling factor for X
        double y = (double)row["y"] * yScale + canvasHeight / 2; // Apply the scaling factor for Y
        polyline.Points.Add(new Point(x, canvasHeight - y)); // Flip y-axis
      }

      GraphCanvas.Children.Add(polyline);
    }



    private double currentScale = 1.0; // Initial scale factor

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
      // Increase the scale factor
      currentScale *= 1.2; // You can adjust the zoom factor as needed
      ApplyScaleTransform();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
      // Decrease the scale factor
      currentScale /= 1.2; // You can adjust the zoom factor as needed
      ApplyScaleTransform();
    }

    private void ApplyScaleTransform()
    {
      // Create a ScaleTransform based on the current scale factor
      ScaleTransform scaleTransform = new ScaleTransform(currentScale, currentScale);

      // Apply the scaling transformation to the GraphCanvas
      GraphCanvas.LayoutTransform = scaleTransform;

      // Recalculate and update grid lines
      DrawGridLines();
    }

    private void DrawGridLines()
    {
      double canvasWidth = GraphCanvas.ActualWidth;
      double canvasHeight = GraphCanvas.ActualHeight;

      // Calculate the grid spacing based on the current scale
      double xGridSpacing = 150 * currentScale;
      double yGridSpacing = 150 * currentScale;

      BlackLinesCanvas.Children.Clear();

      // Draw vertical grid lines
      for (double x = canvasWidth / 2; x < canvasWidth; x += xGridSpacing)
      {
        Line line = new Line();
        line.X1 = x;
        line.Y1 = 0;
        line.X2 = x;
        line.Y2 = canvasHeight;
        line.Stroke = Brushes.Black;
        line.StrokeThickness = 0.5;
        BlackLinesCanvas.Children.Add(line);
      }

      for (double x = canvasWidth / 2 - xGridSpacing; x >= 0; x -= xGridSpacing)
      {
        Line line = new Line();
        line.X1 = x;
        line.Y1 = 0;
        line.X2 = x;
        line.Y2 = canvasHeight;
        line.Stroke = Brushes.Black;
        line.StrokeThickness = 0.5;
        BlackLinesCanvas.Children.Add(line);
      }

      // Draw horizontal grid lines
      for (double y = canvasHeight / 2; y < canvasHeight; y += yGridSpacing)
      {
        Line line = new Line();
        line.X1 = 0;
        line.Y1 = y;
        line.X2 = canvasWidth;
        line.Y2 = y;
        line.Stroke = Brushes.Black;
        line.StrokeThickness = 0.5;
        BlackLinesCanvas.Children.Add(line);
      }

      for (double y = canvasHeight / 2 - yGridSpacing; y >= 0; y -= yGridSpacing)
      {
        Line line = new Line();
        line.X1 = 0;
        line.Y1 = y;
        line.X2 = canvasWidth;
        line.Y2 = y;
        line.Stroke = Brushes.Black;
        line.StrokeThickness = 0.5;
        BlackLinesCanvas.Children.Add(line);
      }
    }


    private Dictionary<string, Expression> cachedExpressions = new Dictionary<string, Expression>();

    private double EvaluateExpression(string expression, double x)
    {
      try
      {
        // Check if the compiled expression is already cached
        if (!cachedExpressions.ContainsKey(expression))
        {
          // Create a new expression with x as a variable and cache it
          Expression expr = new Expression(expression, new Argument("x"));
          cachedExpressions[expression] = expr;
        }

        // Get the cached expression and set its variable value
        Expression cachedExpr = cachedExpressions[expression];
        cachedExpr.setArgumentValue("x", x);

        // Evaluate the cached expression
        double result = cachedExpr.calculate();

        return result;
      }
      catch (Exception)
      {
        return double.NaN;
      }
    }



  }
}
