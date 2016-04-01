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
[assembly: CommandClass(typeof(GabaritS.MyCommands))]

namespace GabaritS
{


    public class MyCommands
    {

        [CommandMethod("MyGroup", "GabaritStroenia", "GabaritStroenia", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyGabaritS()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            #region Проверка даты
            //if (DateTime.Now > (new DateTime(2017, 01, 01)))
            //{
            //    ed.WriteMessage("\nВремя действия программы истекло, для продолжения, свяжитесь с создателем :)");
            //    return;
            //}
            #endregion

            StartParametrs startParametrs = new StartParametrs(acDoc, ed);
            if (startParametrs.IsCancel) return;


            Point3dCollection listOfINgab = new Point3dCollection();
            Point3dCollection listOfOutgab = new Point3dCollection();
            Vector3d StartVector;
            Vector3d EndVector;
            using (Transaction transaction = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;
                acBlkTbl = transaction.GetObject(acCurDb.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                acBlkTblRec = transaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                using (Curve curve = (Curve)transaction.GetObject(startParametrs.selPolyRes.ObjectId, OpenMode.ForWrite))
                {
                  
                    

                    startParametrs.polyLength = ((Polyline2d)curve).Length;
                    Gabarit gab = new Gabarit(startParametrs);
                    List<Point2d> listOfPointGab = gab.GetListOfPoint();

                    StartVector = curve.GetFirstDerivative(curve.StartPoint);
                    EndVector = curve.GetFirstDerivative(curve.EndPoint);
                    Point3d pointStart = curve.StartPoint - StartVector * 10;
                    Polyline2d poly = (Polyline2d)curve;
                    
                    using (Polyline acPoly = new Polyline())
                    {
                        int j = 0;
                        acPoly.AddVertexAt(j++, new Point2d(pointStart.X,pointStart.Y), 0, 0, 0);
                        for (int i = 0; i <= startParametrs.polyLength; i++)
                        {
                            Point2d PointOnCurve = new Point2d(poly.GetPointAtDist(i).X,poly.GetPointAtDist(i).Y);
                            acPoly.AddVertexAt(j++, PointOnCurve, 0, 0, 0);
                        }
                       
                        acPoly.AddVertexAt(j++, new Point2d(poly.EndPoint.X, poly.EndPoint.Y), 0, 0, 0);
                        //acPoly.Length
                     
                     //   acPoly.TransformBy(ucs);
                        // Add the new object to the block table record and the transaction
                        //acBlkTblRec.AppendEntity(acPoly);
                        //transaction.AddNewlyCreatedDBObject(acPoly, true);

                        for (int i = 0; i <= acPoly.Length; i++)
                        {
                            Vector3d tangentVector = acPoly.GetFirstDerivative(acPoly.GetPointAtDist(i));
                            Vector3d perpendicularVector = tangentVector.GetPerpendicularVector();
                            perpendicularVector = perpendicularVector.GetNormal();
                            Point3d startPoint = acPoly.GetPointAtDist(i) + perpendicularVector * listOfPointGab[i].X;
                            Point3d endPoint = acPoly.GetPointAtDist(i) - perpendicularVector * listOfPointGab[i].Y;

                            listOfINgab.Add(startPoint);
                            listOfOutgab.Add(endPoint);
                        }
                        if (acPoly.Length < Math.Round(acPoly.Length))
                        {
                            Vector3d tangentVector = acPoly.GetFirstDerivative(acPoly.EndPoint);
                            Vector3d perpendicularVector = tangentVector.GetPerpendicularVector();
                            perpendicularVector = perpendicularVector.GetNormal();
                            Point3d startPoint = acPoly.EndPoint + perpendicularVector * listOfPointGab[listOfPointGab.Count - 1].X;
                            Point3d endPoint = acPoly.EndPoint - perpendicularVector * listOfPointGab[listOfPointGab.Count - 1].Y;

                            listOfINgab.Add(startPoint);
                            listOfOutgab.Add(endPoint);
                        }
                        

                    }
                   
                

                }
                //using (Polyline acPoly = new Polyline())
                //{
                //    for (int i = 0; i <= startParametrs.polyLength; i++)
                //    {
                //        Point2d PointOnCurve = new Point2d(poly.GetPointAtDist(i).X, poly.GetPointAtDist(i).Y);
                //        acPoly.AddVertexAt(j++, PointOnCurve, 0, 0, 0);
                //    }
                //}


                Spline acSplineIn = new Spline(listOfINgab, StartVector, EndVector, 4, 0.0);
                acSplineIn.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(acSplineIn);
                transaction.AddNewlyCreatedDBObject(acSplineIn, true);

                Spline acSplineOut = new Spline(listOfOutgab, StartVector, EndVector, 4, 0.0);
                acSplineOut.SetDatabaseDefaults();
                acBlkTblRec.AppendEntity(acSplineOut);
                transaction.AddNewlyCreatedDBObject(acSplineOut, true);


                transaction.Commit();
            }

            Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();

            //GetParameterAtDistance()
        }

    

        //private static CoordinatesOfPointsGabarit GetPointOfGab(Database acCurDb, Polyline poly, Point2d CurrPoint, Point3d pointOnCurve)
        //{
        //    CoordinatesOfPointsGabarit Coord;
        //    using (Transaction transaction = acCurDb.TransactionManager.StartTransaction())
        //    {
        //        using (Curve curve = (Curve)transaction.GetObject(poly.ObjectId, OpenMode.ForRead))
        //        {

        //            Vector3d tangentVector = curve.GetFirstDerivative(pointOnCurve);
        //            Vector3d perpendicularVector = tangentVector.GetPerpendicularVector();
        //            perpendicularVector = perpendicularVector.GetNormal();
        //            Point3d startPoint = pointOnCurve + perpendicularVector * CurrPoint.X;
        //            Point3d endPoint = pointOnCurve - perpendicularVector * CurrPoint.Y;
        //            Coord = new CoordinatesOfPointsGabarit(startPoint, endPoint);
        //        }

        //        transaction.Commit();
        //    }
        //    return Coord;
        //}


    }


}