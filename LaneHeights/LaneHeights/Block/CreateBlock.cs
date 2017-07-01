using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
#endregion

namespace LaneHeights_Block
{
    public class Create
    {
        #region Create Block
        public void CreateBlock()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the block table from the drawing
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                // Check the block name, to see whether it's 
                // already in use
                PromptStringOptions pso = new PromptStringOptions("\nEnter new block name: ");
                pso.AllowSpaces = true;

                // A variable for the block's name
                string blkName = "";

                do
                {
                    PromptResult pr = ed.GetString(pso);

                    // Just return if the user cancelled
                    // (will abort the transaction as we drop out of the using
                    // statement's scope)
                    if (pr.Status != PromptStatus.OK)
                        return;

                    try
                    {
                        // Validate the provided symbol table name
                        SymbolUtilityServices.ValidateSymbolName(pr.StringResult, false);

                        // Only set the block name if it isn't in use
                        if (bt.Has(pr.StringResult))
                            ed.WriteMessage("\nA block with this name already exists.");
                        else
                            blkName = pr.StringResult;
                    }
                    catch
                    {
                        // An exception has been thrown, indicating the
                        // name is invalid
                        ed.WriteMessage("\nInvalid block name.");
                    }

                } while (blkName == "");

                // Create our new block table record...
                BlockTableRecord btr = new BlockTableRecord();

                // ... and set its properties
                btr.Name = blkName;

                // Add the new block to the block table
                bt.UpgradeOpen();
                ObjectId btrId = bt.Add(btr);
                tr.AddNewlyCreatedDBObject(btr, true);

                // Add some lines to the block to form a square
                // (the entities belong directly to the block)
                DBObjectCollection ents = SquareOfLines(5);
                foreach (Entity ent in ents)
                {
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                }

                // Add a block reference to the model space
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                BlockReference br = new BlockReference(Point3d.Origin, btrId);
                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);

                // Commit the transaction
                tr.Commit();

                // Report what we've done
                ed.WriteMessage("\nCreated block named \"{0}\" containing {1} entities.", blkName, ents.Count);
            }
        }

        private DBObjectCollection SquareOfLines(double size)
        {
            // A function to generate a set of entities for our block

            DBObjectCollection ents = new DBObjectCollection();
            Point3d[] pts =
                { new Point3d(-size, -size, 0),
            new Point3d(size, -size, 0),
            new Point3d(size, size, 0),
            new Point3d(-size, size, 0)
          };
            int max = pts.GetUpperBound(0);

            for (int i = 0; i <= max; i++)
            {
                int j = (i == max ? 0 : i + 1);
                Line ln = new Line(pts[i], pts[j]);
                ents.Add(ln);
            }
            return ents;
        }
        #endregion

        #region CreateBlock From
        public void CreateBlockFrom()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                DBObjectCollection objs = new DBObjectCollection();
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                //EntProxy.Explode(objs);

                string blkName = "Getname";

                if (bt.Has(blkName) == false)
                {
                    BlockTableRecord btr = new BlockTableRecord();
                    btr.Name = blkName;

                    bt.UpgradeOpen();
                    ObjectId btrId = bt.Add(btr);
                    tr.AddNewlyCreatedDBObject(btr, true);

                    foreach (DBObject obj in objs)
                    {
                        Entity ent = (Entity)obj;
                        btr.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);
                    }

                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    BlockReference br =
                      new BlockReference(Point3d.Origin, btrId);

                    ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);
                }
                tr.Commit();
            }
        }
        #endregion
    }


}
