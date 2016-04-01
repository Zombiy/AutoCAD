// (C) Copyright 2016 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(GabaritOborudovania.MyCommands))]

namespace GabaritOborudovania
{

   
    public class MyCommands
    {

        [CommandMethod("MyGroup", "MyGabaritOborudovania", "MyGabaritOborudovania", CommandFlags.Modal)]
        public void MyGabaritO()
        {
           
            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            if (DateTime.Now>(new DateTime(2017,01,01)))
            {
                ed.WriteMessage("\nВремя действия программы истекло, для продолжения, свяжитесь с создателем :)");
                return;
            }
            pPtOpts.Message = "\nТочка вставки габарита: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point2d ptStart = new Point2d(pPtRes.Value.X, pPtRes.Value.Y);
            if (pPtRes.Status == PromptStatus.Cancel) return;

            PromptIntegerResult pRadRes;
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            pIntOpts.Message = "\n Введите радиус: ";
            pRadRes = acDoc.Editor.GetInteger(pIntOpts);
            if (pRadRes.Status == PromptStatus.Cancel) return;

            PromptIntegerResult pHeightRes;
            pIntOpts.Message = "\n Введите возвышение: ";
            pHeightRes = acDoc.Editor.GetInteger(pIntOpts);
            if (pHeightRes.Status == PromptStatus.Cancel) return;

            Gabarit gabarit = new Gabarit(pRadRes.Value, pHeightRes.Value, ptStart);
            List<Point2d> ListOfPoints = gabarit.GetListOfPoints();
           
            Matrix3d ucs = ed.CurrentUserCoordinateSystem;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;
                
                // Open Model space for write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                using (Polyline acPoly = new Polyline())
                {
                    int i = 0;
                    foreach (Point2d item in ListOfPoints)
                    {
                        acPoly.AddVertexAt(i++, item, 0, 0, 0);
                        
                    }
                    acPoly.AddVertexAt(i++, ListOfPoints[0], 0, 0, 0);
                    acPoly.TransformBy(ucs);
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                }


                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }

        }



    }

}
