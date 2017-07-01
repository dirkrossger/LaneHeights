using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
#endregion

namespace LaneHeights_Layer
{
    class Layer
    {
        public static string CreateLayerIfMissing(string layer)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTr = acDoc.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable acLyrTbl = acTr.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (acLyrTbl.Has(layer) == false)
                    {
                        LayerTableRecord acLyrTblRec = new LayerTableRecord();
                        acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                        acLyrTblRec.Name = layer;
                        acLyrTbl.UpgradeOpen();
                        acLyrTbl.Add(acLyrTblRec);
                        acTr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                    }
                    acTr.Commit();
                }
                catch
                {
                    acTr.Abort();
                }
            }
            return layer;
        }

    }
}
