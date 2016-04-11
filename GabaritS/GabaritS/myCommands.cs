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
            if (DateTime.Now > (new DateTime(2017, 01, 01)))
            {
                ed.WriteMessage("\nВремя действия программы истекло, для продолжения, свяжитесь с создателем :)");
                return;
            }
            #endregion

            StartParametrs startParametrs = new StartParametrs(acDoc, ed);
            if (startParametrs.IsCancel) return;
            const string blockName = "BlockPointOfGabarit";

            Point3dCollection listOfINgab = new Point3dCollection();
            Point3dCollection listOfOutgab = new Point3dCollection();
            Vector3d StartVector;
            Vector3d EndVector;
            using (Transaction transaction = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;
                LayerTable acLyrTbl = transaction.GetObject(acCurDb.LayerTableId, OpenMode.ForWrite) as LayerTable;

                acBlkTbl = transaction.GetObject(acCurDb.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                acBlkTblRec = transaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                BlockTable bt = (BlockTable)transaction.GetObject(acCurDb.BlockTableId, OpenMode.ForWrite);
                ObjectId btrId;
                if (!bt.Has(blockName))
                {
                    if (acLyrTbl.Has("ГТ_Значения_Габаритов"))
                        acCurDb.Clayer = acLyrTbl["ГТ_Значения_Габаритов"];
                    // создаем новое определение блока, задаем ему имя
                    BlockTableRecord btr = new BlockTableRecord();
                    btr.Name = blockName;

                    // добавляем созданное определение блока в таблицу блоков и в транзакцию,
                    // запоминаем ID созданного определения блока (оно пригодится чуть позже)
                    btrId = bt.Add(btr);
                    transaction.AddNewlyCreatedDBObject(btr, true);

                    // создаем окружность
                    Circle cir = new Circle();
                    cir.SetDatabaseDefaults();
                    cir.Center = new Point3d(0, 0, 0);
                    cir.Radius = 0.1;

                    // добавляем окружность в определение блока и в транзакцию
                    btr.AppendEntity(cir);
                    transaction.AddNewlyCreatedDBObject(cir, true);
                }
                else
                {
                    btrId = bt[blockName];
                }



                #region чертим габарит

                using (Curve curve = (Curve)transaction.GetObject(startParametrs.selPolyRes.ObjectId, OpenMode.ForWrite))
                {


                    startParametrs.polyLength = ((Polyline2d)curve).Length;
                    Gabarit gab = new Gabarit(startParametrs);
                    List<Point2d> listOfPointGab = gab.GetListOfPoint();

                    StartVector = curve.GetFirstDerivative(curve.StartPoint);
                    EndVector = curve.GetFirstDerivative(curve.EndPoint);

                    Point3d pointStart = curve.StartPoint - StartVector / StartVector.Length * 10;
                    Polyline2d poly = (Polyline2d)curve;

                    using (Polyline acPoly = new Polyline())
                    {
                        int j = 0;
                        acPoly.AddVertexAt(j++, new Point2d(pointStart.X, pointStart.Y), 0, 0, 0);
                        for (int i = 0; i <= startParametrs.polyLength; i++)
                        {
                            Point2d PointOnCurve = new Point2d(poly.GetPointAtDist(i).X, poly.GetPointAtDist(i).Y);
                            acPoly.AddVertexAt(j++, PointOnCurve, 0, 0, 0);
                        }

                        acPoly.AddVertexAt(j++, new Point2d(poly.EndPoint.X, poly.EndPoint.Y), 0, 0, 0);
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

                            if (i % 5 == 0)
                            {
                                if (acLyrTbl.Has("ГТ_Значения_Габаритов"))
                                    acCurDb.Clayer = acLyrTbl["ГТ_Значения_Габаритов"];
                                BlockReference br = new BlockReference(acPoly.GetPointAtDist(i), btrId);
                                acBlkTblRec.AppendEntity(br);
                                transaction.AddNewlyCreatedDBObject(br, true);
                                double ang = Vector3d.XAxis.GetAngleTo(perpendicularVector, Vector3d.ZAxis);

                                DBText textY = new DBText();
                                textY.Height = 0.6;

                                textY.Rotation = startParametrs.TextPrav ? ang + Math.PI : ang;
                                textY.Position = startParametrs.TextPrav ? acPoly.GetPointAtDist(i) - perpendicularVector * 0.2 : acPoly.GetPointAtDist(i) - perpendicularVector * 2;
                                textY.TextString = Add00Totext(listOfPointGab[i].Y.ToString());
                                acBlkTblRec.AppendEntity(textY);
                                transaction.AddNewlyCreatedDBObject(textY, true);

                                DBText textX = new DBText();
                                textX.Height = 0.6;
                                textX.Rotation = startParametrs.TextPrav ? ang + Math.PI : ang;
                                textX.Position = startParametrs.TextPrav ? acPoly.GetPointAtDist(i) + perpendicularVector * 2 : acPoly.GetPointAtDist(i) + perpendicularVector * 0.2;
                                
                                textX.TextString = Add00Totext(listOfPointGab[i].X.ToString());
                                acBlkTblRec.AppendEntity(textX);
                                transaction.AddNewlyCreatedDBObject(textX, true);

                                if (acLyrTbl.Has("ГТ_Габарит"))
                                    acCurDb.Clayer = acLyrTbl["ГТ_Габарит"];
                            }

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

                            double ang = Vector3d.XAxis.GetAngleTo(perpendicularVector, Vector3d.ZAxis);
                            if (acLyrTbl.Has("ГТ_Значения_Габаритов"))
                                acCurDb.Clayer = acLyrTbl["ГТ_Значения_Габаритов"];
                            BlockReference br = new BlockReference(acPoly.EndPoint, btrId);
                            acBlkTblRec.AppendEntity(br);
                            transaction.AddNewlyCreatedDBObject(br, true);
                          

                            DBText textY = new DBText();
                            textY.Height = 0.6;
                            textY.Rotation = startParametrs.TextPrav ? ang + Math.PI : ang;
                            textY.Position = startParametrs.TextPrav ? acPoly.EndPoint - perpendicularVector * 0.2 : acPoly.EndPoint - perpendicularVector * 2;
                            textY.TextString = Add00Totext(listOfPointGab[listOfPointGab.Count - 1].Y.ToString());
                            acBlkTblRec.AppendEntity(textY);
                            transaction.AddNewlyCreatedDBObject(textY, true);

                            DBText textX = new DBText();
                            textX.Height = 0.6;
                            textX.Rotation = startParametrs.TextPrav ? ang + Math.PI : ang;
                            textX.Position = startParametrs.TextPrav ? acPoly.EndPoint + perpendicularVector * 2 : acPoly.EndPoint + perpendicularVector * 0.2;
                            textX.TextString = Add00Totext(listOfPointGab[listOfPointGab.Count - 1].X.ToString());
                            acBlkTblRec.AppendEntity(textX);
                            transaction.AddNewlyCreatedDBObject(textX, true);

                            if (acLyrTbl.Has("ГТ_Габарит"))
                                acCurDb.Clayer = acLyrTbl["ГТ_Габарит"];

                        }
                    }
                }
                #endregion

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
        }

        private static string Add00Totext(string textValue)
        {
            while (textValue.Length < 5)
                textValue += "0";
            return textValue;
        }
    }


}