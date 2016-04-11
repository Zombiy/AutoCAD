using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabaritS
{
    class Gabarit
    {
        public double R { get; set; }
        public double L { get; set; }
        public double H { get; set; }
        public bool KRPrav { get; set; }
        public bool KrivPrav { get; set; }
       
        public Gabarit(StartParametrs st)
        {
            R = st.pRadRes.Value; 
            L = st.polyLength; 
            H = st.pHeightRes.Value; 
            KRPrav = st.KRPrav;
            KrivPrav = st.KrivPrav;
        }
        public List<Point2d> GetListOfPoint()
        {
            List<Point2d> ListOfPoint = new List<Point2d>();
            List<double> ValueIN = new List<double>();
            List<double> ValueOUT = new List<double>();
            double C = R * L;
            double br = R - Math.Sqrt(R * R - 41.796225);
            double d_out = br - H * 0.619824216187852;
            double d_in = br + H * 2.06393447461629;
            double dlinaOtvoda;
            if (L > 0)
            {
                dlinaOtvoda = Math.Round(L + 5);
            }
            else { return null; }

            double gabIn;
            double gabOut;

            if (KrivPrav)
            {
                gabIn = KRPrav ? 1900 : 2200;
                gabOut = KRPrav ? 2200 : 1900;
            }
            else
            {
                gabIn = KRPrav ? 2200 : 1900;
                gabOut = KRPrav ? 1900 : 2200;
            }

            for (int i = 0; i < 5; i++)
            {
                ValueIN.Add(gabIn / 1000);
            }

            for (int i = 0; i <= dlinaOtvoda; i++)
            {
                
                ValueIN.Add(Math.Round(gabIn/1000 + (d_in * i ) / dlinaOtvoda,3));
                ValueOUT.Add(Math.Round(gabOut/1000 + (d_out * i) / dlinaOtvoda,3));
                if (i == dlinaOtvoda)
                {
                for (int j = 0; j < 5; j++)
                {
                    ValueOUT.Add(Math.Round(gabOut/1000 + (d_out * i) / dlinaOtvoda,3));
                }
                }
            }
            for (int i = 0; i <= dlinaOtvoda+5; i++)
            {
                Point2d pointKrivPr = KrivPrav ? new Point2d(ValueOUT[i], ValueIN[i]) : new Point2d(ValueIN[i], ValueOUT[i]); 
               
                ListOfPoint.Add(pointKrivPr);
            }
            
            return ListOfPoint;
        }

    }

}
