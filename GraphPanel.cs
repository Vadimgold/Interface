using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace winforms_Laba2
{
    public partial class GraphPanel : UserControl
    {
        public enum GraphType
        {
            Line,
            Spline
        }

        private Dictionary<string, List<Data>> datasets;
        private float scale = 1f;
        private PointF offset = new PointF(0, 0);
        private PointF mouseDownLocation;
        private List<Color> colors;
        private float xMin, xMax, yMin, yMax;
        private GraphType currentGraphType = GraphType.Line;

        public GraphPanel()
        {
            datasets = new Dictionary<string, List<Data>>();
            this.DoubleBuffered = true;
            this.MouseWheel += GraphPanel_MouseWheel;
            this.MouseDown += GraphPanel_MouseDown;
            this.MouseMove += GraphPanel_MouseMove;

            colors = new List<Color> { Color.Blue, Color.Red, Color.Green, Color.Orange, Color.Purple };
        }

        public void AddDataset(string name, List<Data> data)
        {
            if (!datasets.ContainsKey(name))
            {
                datasets[name] = data;
                UpdateDataRange();
                Invalidate();
            }
            else
            {
                datasets[name] = data; // Обновление данных если они уже существуют
                UpdateDataRange();
                Invalidate();
            }
        }

        public void SetGraphType(GraphType type)
        {
            currentGraphType = type;
            Invalidate();
        }

        private void UpdateDataRange()
        {
            if (datasets.Count > 0)
            {
                xMin = float.MaxValue;
                xMax = float.MinValue;
                yMin = float.MaxValue;
                yMax = float.MinValue;

                foreach (var dataset in datasets.Values)
                {
                    foreach (var point in dataset)
                    {
                        if (point.X < xMin) xMin = (float)point.X;
                        if (point.X > xMax) xMax = (float)point.X;
                        if (point.Y < yMin) yMin = (float)point.Y;
                        if (point.Y > yMax) yMax = (float)point.Y;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            DrawAxes(g);

            int colorIndex = 0;
            foreach (var dataset in datasets)
            {
                if (dataset.Value.Count > 1)
                {
                    DrawGraph(g, dataset.Value, colors[colorIndex % colors.Count]);
                    colorIndex++;
                }
            }

            DrawLegend(g);
        }

        private void DrawAxes(Graphics g)
        {
            Pen axisPen = new Pen(Color.Black, 2);

            g.DrawLine(axisPen, 0, this.Height / 2 + offset.Y, this.Width, this.Height / 2 + offset.Y);
            g.DrawLine(axisPen, this.Width / 2 + offset.X, 0, this.Width / 2 + offset.X, this.Height);

            float stepX = Math.Max(1, (xMax - xMin) / 10);
            for (float i = xMin; i <= xMax; i += stepX)
            {
                float x = (i * scale) + this.Width / 2 + offset.X;
                g.DrawLine(Pens.Black, x, this.Height / 2 - 5 + offset.Y, x, this.Height / 2 + 5 + offset.Y);
                g.DrawString(i.ToString("0.00"), this.Font, Brushes.Black, x - 10, this.Height / 2 + 10 + offset.Y);
            }

            float stepY = Math.Max(1, (yMax - yMin) / 10);
            for (float i = yMin; i <= yMax; i += stepY)
            {
                float y = -(i * scale) + this.Height / 2 + offset.Y;
                g.DrawLine(Pens.Black, this.Width / 2 - 5 + offset.X, y, this.Width / 2 + 5 + offset.X, y);
                g.DrawString(i.ToString("0.00"), this.Font, Brushes.Black, this.Width / 2 + 10 + offset.X, y - 10);
            }

            g.DrawString("Y", this.Font, Brushes.Black, 10, 10);
            g.DrawString("X", this.Font, Brushes.Black, this.Width - 20, this.Height - 20);
        }

        private void DrawGraph(Graphics g, List<Data> data, Color color)
        {
            if (currentGraphType == GraphType.Line)
            {
                Pen graphPen = new Pen(color, 2);
                PointF prevPoint = TransformToScreen(data[0]);

                for (int i = 1; i < data.Count; i++)
                {
                    PointF currentPoint = TransformToScreen(data[i]);
                    g.DrawLine(graphPen, prevPoint, currentPoint);
                    prevPoint = currentPoint;
                }
            }
            else if (currentGraphType == GraphType.Spline)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    PointF[] points = data.Select(d => TransformToScreen(d)).ToArray();
                    path.AddCurve(points);

                    using (Pen graphPen = new Pen(color, 2))
                    {
                        g.DrawPath(graphPen, path);
                    }
                }
            }
        }

        private PointF TransformToScreen(Data point)
        {
            float x = (float)((point.X * scale) + this.Width / 2 + offset.X);
            float y = (float)(-(point.Y * scale) + this.Height / 2 + offset.Y);
            return new PointF(x, y);
        }

        private void GraphPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                scale *= 1.1f;
            }
            else if (e.Delta < 0)
            {
                scale /= 1.1f;
            }
            Invalidate();
        }

        private void GraphPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownLocation = e.Location;
            }
        }

        private void GraphPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                offset.X += e.X - mouseDownLocation.X;
                offset.Y += e.Y - mouseDownLocation.Y;
                mouseDownLocation = e.Location;
                Invalidate();
            }
        }

        private void DrawLegend(Graphics g)
        {
            int legendX = this.Width - 120;
            int legendY = 10;

            foreach (var dataset in datasets)
            {
                using (Brush legendBrush = new SolidBrush(colors[datasets.Keys.ToList().IndexOf(dataset.Key) % colors.Count]))
                {
                    g.FillRectangle(legendBrush, legendX, legendY, 10, 10);
                    g.DrawString(dataset.Key, this.Font, Brushes.Black, legendX + 15, legendY - 3);
                    legendY += 15;
                }
            }
        }
    }
}
