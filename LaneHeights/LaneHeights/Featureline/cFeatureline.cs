using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AECC.Interop.Land;
using Autodesk.AECC.Interop.UiLand;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Interop;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
#endregion

using LaneHeights;
using LaneHeights_Layer;

namespace LaneHeights_Featureline
{
    class Featureline
    {
        //#region Test draw Featureline
        //private IAeccApplication m_oAeccApp = null;
        //private IAeccDocument m_oAeccDoc = null;
        //private Autodesk.AECC.Interop.Land.IAeccDatabase m_oAeccDb = null;
        //private Collection<cqFeatureLine> m_cqFeatureLines = null;
        //private CQLanguageResourceXml dbLanguage;
        //private Corridor mCorridor;

        //public ObjectId drawFeatureLine(Point3dCollection acPts3dPoly, string layer, string site)
        //{
        //    ObjectId objId = ObjectId.Null;
        //    if (acPts3dPoly.Count < 2) return objId; //don't draw lines with One point 

        //    try
        //    {
        //        ObjectId polyObjectID = ObjectId.Null;
        //        Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //        Database acCurDb = acDoc.Database;
        //        using (Transaction acTr = acDoc.TransactionManager.StartTransaction())
        //        {
        //            BlockTable acBlkTbl = null;
        //            acBlkTbl = acTr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
        //            BlockTableRecord acBlkTblRec = null;
        //            acBlkTblRec = acTr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //            using (Polyline3d acPoly3d = new Polyline3d(Poly3dType.SimplePoly, acPts3dPoly, false))
        //            {
        //                acPoly3d.SetDatabaseDefaults();
        //                acPoly3d.Layer = Layer.CreateLayerIfMissing(layer);
        //                acPoly3d.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);

        //                polyObjectID = acBlkTblRec.AppendEntity(acPoly3d);
        //                acTr.AddNewlyCreatedDBObject(acPoly3d, true);
        //                acTr.Commit();

        //                acBlkTbl.Dispose();
        //                acBlkTblRec.Dispose();
        //            }
        //            AeccSite oSite = m_oAeccDoc.Sites.Item(site);
        //            AeccFeatureLineStyles oFeatureLineStyles = m_oAeccDb.FeatureLineStyles;
        //            AeccFeatureLineStyle oFeatureLineStyle = null;

        //            foreach (AeccFeatureLineStyle flStyle in oFeatureLineStyles)//select iff style exist
        //            {
        //                if (flStyle.Name.Contains(layer))
        //                {
        //                    oFeatureLineStyle = flStyle;
        //                    break;
        //                }
        //            }

        //            if (oFeatureLineStyle == null)//create style from defalut
        //            {
        //                oFeatureLineStyle = m_oAeccDb.FeatureLineStyles.Add(layer);
        //            }

        //            IAeccLandFeatureLine oFeatureLine = null;
        //            //64 system
        //            oFeatureLine = oSite.FeatureLines.AddFromPolyline(polyObjectID.OldIdPtr.ToInt64(), oFeatureLineStyle);
        //            oFeatureLine.Layer = layer;
        //            cqFeatureLine featureLine = new cqFeatureLine(layer, 1, site);

        //            var sort =
        //               from fline in m_cqFeatureLines
        //               where fline.mGroupName.Contains(layer)
        //               orderby fline.mItemNumber ascending
        //               select fline;

        //            if (sort.Count() < 1)
        //            {
        //                m_cqFeatureLines.Add(featureLine);
        //                oFeatureLine.Name = featureLineNumbering(layer, featureLine.mItemNumber);
        //            }
        //            else
        //            {
        //                cqFeatureLine featureLineLast = sort.First();
        //                featureLineLast.mItemNumber++;
        //                m_cqFeatureLines.Add(featureLineLast);
        //                oFeatureLine.Name = featureLineNumbering(layer, featureLineLast.mItemNumber);
        //                featureLineLast = null;
        //            }

        //            long ln = Convert.ToInt64(oFeatureLine.Handle, 16);
        //            Handle hn = new Handle(ln);
        //            objId = acCurDb.GetObjectId(false, hn, 0);

        //            sort = null;
        //            featureLine = null;
        //        }

        //        //remove polyline
        //        using (Transaction acTr = acDoc.TransactionManager.StartTransaction())
        //        {
        //            BlockTable acBlkTbl = null;
        //            acBlkTbl = acTr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
        //            BlockTableRecord acBlkTblRec = null;
        //            acBlkTblRec = acTr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //            using (Polyline3d acPoly3d = acTr.GetObject(polyObjectID, OpenMode.ForWrite) as Polyline3d)
        //            {
        //                acPoly3d.Erase(true);
        //                acPoly3d.Dispose();
        //                acPoly3d.Dispose();
        //                acTr.Commit();
        //                acBlkTbl.Dispose();
        //                acBlkTblRec.Dispose();
        //            }
        //        }

        //    }

        //    catch
        //    {
        //    }
        //    return objId;
        //}
        //#endregion

        public FeatureLine SelectFeatureline(string message)
        {
            PromptEntityOptions options = new PromptEntityOptions(message);
            options.SetRejectMessage("\nThe selected object is not a Featureline!");
            options.AddAllowedClass(typeof(FeatureLine), false);
            PromptEntityResult peo = Active.Editor.GetEntity(options);

            FeatureLine result = null;

            using (Transaction tr = Active.Database.TransactionManager.StartTransaction())
            {
                switch (peo.Status)
                {
                    case PromptStatus.OK:
                        FeatureLine feat = tr.GetObject(peo.ObjectId, OpenMode.ForRead) as FeatureLine;
                        result = feat;
                        break;
                    case PromptStatus.Cancel:
                        Active.Editor.WriteMessage("Select canceled");
                        return null;
                }
                tr.Commit();
                return result;
            }
        }

//#region Test2 Featureline from Polyline
//        // COM objects:
//        private Autodesk.AutoCAD.Interop.IAcadApplication m_oAcadApp = null;
//        private Autodesk.AECC.Interop.UiLand.IAeccApplication m_oAeccApp = null;
//        private Autodesk.AECC.Interop.UiLand.IAeccDocument m_oAeccDoc = null;
//        private Autodesk.AECC.Interop.Land.IAeccDatabase m_oAeccDb = null;
//        string m_sAcadProdID = "AutoCAD.Application";
//        //Civil 3D 2013
//        string m_sAeccAppProgId = "AeccXUiLand.AeccApplication.10.0";
//        //Civil 3D 2012 
//        //string m_sAeccAppProgId = "AeccXUiLand.AeccApplication.9.0"; 
//        private string m_sMessage = "";

//        public void Create()
//        {
//            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
//            {
//                try
//                {
//                    if (m_oAcadApp == null)
//                    {
//                        m_oAcadApp = (IAcadApplication)System.Runtime.InteropServices.Marshal.GetActiveObject(m_sAcadProdID);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Active.Editor.WriteMessage(ex.Message);
//                }

//                try
//                {

//                    // Select the 3D Polyline which you want to convert to Feature Line

//                    PromptEntityOptions promptEntOp = new PromptEntityOptions("Select a 3D Polyline : ");
//                    PromptEntityResult promptEntRs = default(PromptEntityResult);
//                    promptEntRs = Active.Editor.GetEntity(promptEntOp);
//                    if (promptEntRs.Status != PromptStatus.OK)
//                    {
//                        Active.Editor.WriteMessage("Exiting! Try Again !");
//                        return;
//                    }
//                    ObjectId idEnt = default(ObjectId);
//                    idEnt = promptEntRs.ObjectId;

//                    AeccLandFeatureLine oFtrLn = null;
//                    AeccLandFeatureLines oFtrLns = m_oAeccDoc.Sites.Item(0).FeatureLines;

//                    long plineObjId = idEnt.GetObject().ObjectIdfyOldIdPtr;

//                    oFtrLn = oFtrLns.AddFromPolyline(plineObjId, oAeccDB.FeatureLineStyles.Item(0));

//                    trans.Commit();
//                }
//                catch (Exception ex)
//                {
//                    Active.Editor.WriteMessage("Error : ", ex.Message + Constants.vbCrLf);
//                }
//            }
//        }

//        #endregion
    }
}
