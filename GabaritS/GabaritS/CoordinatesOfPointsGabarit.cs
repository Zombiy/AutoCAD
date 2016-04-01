using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabaritS
{
    class CoordinatesOfPointsGabarit
    {
        public Point3d Out { get; set; }
        public Point3d In { get; set; }
        public CoordinatesOfPointsGabarit(Point3d _out, Point3d _in)
        {
            Out = _out;
            In = _in;
        }
    }
}
