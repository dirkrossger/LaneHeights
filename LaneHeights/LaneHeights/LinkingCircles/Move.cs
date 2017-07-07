using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

[assembly:
  CommandClass(
    typeof(
      LaneHeights_LinkingCircles.Move
    )
  )
]

namespace LaneHeights_LinkingCircles
{
    public class Move
    {
        [CommandMethod("MoveCircle")]
        public static void MoveObject()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                PromptEntityOptions poCi = new PromptEntityOptions("\nSelect Circle! ");
                poCi.SetRejectMessage("\nOnly Circle can be selected.");
                poCi.AddAllowedClass(typeof(Circle), false);
                PromptEntityResult res = ed.GetEntity(poCi);

                ObjectId from = res.ObjectId;
                
                if (res.Status == PromptStatus.OK)
                {
                    ObjectId id = res.ObjectId;
                    Circle acCirc = acTrans.GetObject(id, OpenMode.ForWrite) as Circle;

                    PromptPointOptions pos = new PromptPointOptions("\nGet new position!");

                    // Create a matrix and move the circle using a vector from cCenter to acPt3d
                    PromptPointResult pr = ed.GetPoint(pos);
                    Point3d acPt3d = pr.Value;
                    Point3d cCenter = acCirc.Center;

                    Vector3d acVec3d = cCenter.GetVectorTo(acPt3d);
                    
                    acCirc.TransformBy(Matrix3d.Displacement(acVec3d));
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }
    }
}
