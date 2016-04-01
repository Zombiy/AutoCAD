﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabaritS
{
    class StartParametrs
    {
        public bool IsCancel { get; set; }
        //    public PromptPointResult pPtRes { get; set; }
        public PromptDoubleResult pRadRes { get; set; }
        public PromptDoubleResult pHeightRes { get; set; }
        public double polyLength { get; set; }
        public PromptEntityResult selPolyRes { get; set; }
        public bool KRPrav { get; set; }
        public bool KrivPrav { get; set; }
        public StartParametrs(Document acDoc, Editor ed)
        {
            do
            {
                PromptEntityOptions promptOptions = new PromptEntityOptions("\nВыберите ось пути: ");
                selPolyRes = ed.GetEntity(promptOptions);
                if (selPolyRes.Status == PromptStatus.Cancel) { IsCancel = true; return; };
            } while (!PolylineToPolyline2d(selPolyRes));
           
            // PromptPointOptions pPtOpts = new PromptPointOptions("");
            // pPtOpts.Message = "\nТочка вставки габарита: ";
            //  pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            //Point2d ptStart = new Point2d(pPtRes.Value.X, pPtRes.Value.Y);
            //if (pPtRes.Status == PromptStatus.Cancel) { IsCancel = true; return; };
            if (!PolylineToPolyline2d(selPolyRes)) { IsCancel = true; return; };// если это не полилиния - выходим, есил это полилиния конвертируем в полилинию 2d
            
            
            
            pRadRes = GetDoubleFromMenu(acDoc, "\n Введите радиус от 0 до 5000: ", 5000);
            if (pRadRes.Status == PromptStatus.Cancel) { IsCancel = true; return; };
            pHeightRes = GetDoubleFromMenu(acDoc, "\n Введите возвышение (от 0 до 0.150) : ", 0.150);
            if (pHeightRes.Status == PromptStatus.Cancel) { IsCancel = true; return; };
            KRPrav = true;
            KrivPrav = true;

        }

        private static bool PolylineToPolyline2d(PromptEntityResult selPolyRes)
        {
            bool isPoly2d = true;
            ObjectId plineId = selPolyRes.ObjectId;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction t =
              db.TransactionManager.StartOpenCloseTransaction())
            {
                 using (Curve curve = (Curve)t.GetObject(selPolyRes.ObjectId, OpenMode.ForWrite))
                {
                    if (curve.GetType()==typeof(Polyline))
                    {
                        isPoly2d = false;
                    }
                    else
                    {
                        if (curve.GetType() == typeof(Polyline2d))
                        {
                            isPoly2d = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    
                 }
                 if (!isPoly2d)
                 {
                     using (Polyline pline = (Polyline)
                 t.GetObject(plineId, OpenMode.ForWrite))
                     {
                         t.AddNewlyCreatedDBObject(pline, false);
                         Polyline2d poly2 = pline.ConvertTo(true);
                         t.AddNewlyCreatedDBObject(poly2, true);
                         t.Commit();
                     }
                 }
               
            }
            return true;
        }
        private static PromptDoubleResult GetDoubleFromMenu(Document acDoc, string str, double max)
        {
            PromptDoubleResult pRes;
            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");
            do
            {
                pDoubleOpts.Message = str;
                pRes = acDoc.Editor.GetDouble(pDoubleOpts);
                if (pRes.Status == PromptStatus.Cancel) break;
            } while (pRes.Value < 0 || pRes.Value > max);

            return pRes;
        }
    }
}
