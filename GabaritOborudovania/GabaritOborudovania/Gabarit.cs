using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabaritOborudovania
{
    class Gabarit
    {
        double Radius { get; set; }
        double Height { get; set; }
        Point2d StartPoint { get; set; }
        double a;
        int br;
        public Gabarit(int R, int H, Point2d P)
        {
            Radius = (double)R;
            Height = (double)H;
            StartPoint = P;
        }
        public List<Point2d> GetListOfPoints()
        {
            br = (int)Math.Round((21000) / Radius);
            a = Math.Atan(Height / 1600);
            List<Point2d> ListOfPoints = new List<Point2d>();
            ListOfPoints.Add(new Point2d(1660, 0));
            ListOfPoints.Add(new Point2d(1660, 550));
            ListOfPoints.Add(new Point2d(Math.Ceiling(1485 + br + 0.03216 * Height), 550));
            ListOfPoints.Add(new Point2d(Math.Ceiling(1485 + br + 0.03216 * Height), Math.Ceiling(740 - 0.19832 * Height)));
            ListOfPoints.Add(new Point2d(Math.Ceiling(1625 + br + 0.37252 * Height), Math.Ceiling(3280 - 0.21708 * Height)));
            ListOfPoints.Add(new Point2d(Math.Ceiling(1330 + br + 0.41875 * Height), Math.Ceiling(3625 - 0.17755 * Height)));
            ListOfPoints.Add(new Point2d(Math.Ceiling(1010 + br - 0.43483 * Height), Math.Ceiling(3745 + 0.13467 * Height)));
            ListOfPoints.Add(new Point2d(Math.Ceiling(360 + br - 0.43952 * Height), Math.Ceiling(3780 + 0.04757 * Height)));

            List<Point2d> ResList = new List<Point2d>();
            double x = 0;
            double y = 0;
            foreach (Point2d item in ListOfPoints)
            {
                x = StartPoint.X + (item.X * Math.Cos(a) + item.Y * Math.Sin(a));
                y = StartPoint.Y + (item.Y * Math.Cos(a) - item.X * Math.Sin(a));
                ResList.Add(new Point2d(x, y));
            }
            ListOfPoints.Reverse();
            foreach (Point2d item in ListOfPoints)
            {
                x = StartPoint.X - (item.X * Math.Cos(a) - item.Y * Math.Sin(a));
                y = StartPoint.Y + (item.Y * Math.Cos(a) + item.X * Math.Sin(a));
                ResList.Add(new Point2d(x, y));
            }
            return ResList;
        }
    }
}
