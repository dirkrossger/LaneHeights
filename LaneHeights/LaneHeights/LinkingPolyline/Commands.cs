using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

[assembly:
  CommandClass(
    typeof(
      LaneHeights_LinkingPolyline.LinkingCommands
    )
  )
]

namespace LaneHeights_LinkingPolyline
{/// <summary>
 /// This class defines our commands and event callbacks.
 /// </summary>
    public class LinkingCommands
    {
        LinkedObjectManager m_linkManager;
        ObjectIdCollection m_entitiesToUpdate;

        public LinkingCommands()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            db.ObjectModified += new ObjectEventHandler(OnObjectModified);
            db.ObjectErased += new ObjectErasedEventHandler(OnObjectErased);
            db.BeginSave += new DatabaseIOEventHandler(OnBeginSave);
            doc.CommandEnded += new CommandEventHandler(OnCommandEnded);

            m_linkManager = new LinkedObjectManager();
            m_entitiesToUpdate = new ObjectIdCollection();
        }

        ~LinkingCommands()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                db.ObjectModified -= new ObjectEventHandler(OnObjectModified);
                db.ObjectErased -= new ObjectErasedEventHandler(OnObjectErased);
                db.BeginSave -= new DatabaseIOEventHandler(OnBeginSave);
                doc.CommandEnded += new CommandEventHandler(OnCommandEnded);
            }
            catch (System.Exception)
            {
                // The document or database may no longer
                // be available on unload
            }
        }

        // Define "LINK" command
        [CommandMethod("LINK")]
        public void LinkEntities()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptEntityOptions poPl = new PromptEntityOptions("\nSelect first Polyline to link: ");
            poPl.AllowNone = true;
            poPl.SetRejectMessage("\nOnly Polyline can be selected.");
            poPl.AddAllowedClass(typeof(Polyline3d), false);

            PromptEntityResult res = ed.GetEntity(poPl);
            if (res.Status == PromptStatus.OK)
            {
                PromptEntityOptions poCi = new PromptEntityOptions("\nSelect second circle to link: ");
                poCi.SetRejectMessage("\nOnly Circle can be selected.");
                poCi.AddAllowedClass(typeof(Circle), false);

                ObjectId from = res.ObjectId;
                res = ed.GetEntity(poCi);
                if (res.Status == PromptStatus.OK)
                {
                    ObjectId to = res.ObjectId;
                    m_linkManager.LinkObjects(from, to);
                    m_entitiesToUpdate.Add(from);
                }
            }
        }

        // Define "LOADLINKS" command
        [CommandMethod("LOADLINKS")]
        public void LoadLinkSettings()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            m_linkManager.LoadFromDatabase(db);
        }

        // Define "SAVELINKS" command
        [CommandMethod("SAVELINKS")]
        public void SaveLinkSettings()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            m_linkManager.SaveToDatabase(db);
        }

        // Define callback for Database.ObjectModified event
        private void OnObjectModified(object sender, ObjectEventArgs e)
        {
            ObjectId id = e.DBObject.ObjectId;
            if (m_linkManager.Contains(id) &&
                !m_entitiesToUpdate.Contains(id))
            {
                m_entitiesToUpdate.Add(id);
            }
        }

        // Define callback for Database.ObjectErased event
        private void OnObjectErased(object sender, ObjectErasedEventArgs e)
        {
            if (e.Erased)
            {
                m_linkManager.RemoveLinks(e.DBObject.ObjectId);
            }
        }

        // Define callback for Database.BeginSave event
        void OnBeginSave(object sender, DatabaseIOEventArgs e)
        {
            Database db = sender as Database;
            if (db != null)
            {
                m_linkManager.SaveToDatabase(db);
            }
        }

        // Define callback for Document.CommandEnded event
        private void OnCommandEnded(object sender, CommandEventArgs e)
        {
            foreach (ObjectId id in m_entitiesToUpdate)
            {
                UpdateLinkedEntities(id);
            }
            m_entitiesToUpdate.Clear();
        }

        private void MoveEntitiy(Transaction tr, Point3d basePt, Point3d moveTo, ObjectId id)
        {
            Vector3d vec = basePt.GetVectorTo(moveTo);
            Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
            ent.UpgradeOpen();
            ent.TransformBy(Matrix3d.Displacement(vec));
      }

        // Helper function for OnCommandEnded
        private void UpdateLinkedEntities(ObjectId from)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            ObjectIdCollection linked = m_linkManager.GetLinkedObjects(from);

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                try
                {
                    Point3d polyInters;
                    Point3d circleCenter;
                    double firstRadius;
                    double secondRadius;

                    Entity entPoly = (Entity)tr.GetObject(from, OpenMode.ForRead);

                    if (entPoly.ObjectId.IsValid)
                    {
                        foreach (ObjectId to in linked)
                        {
                            Entity ent2 = (Entity)tr.GetObject(to, OpenMode.ForRead);
                            if (GetCenterAndRadius(ent2, out circleCenter, out secondRadius))
                            {
                                Polyline3d acPoly3d = (Polyline3d)tr.GetObject(entPoly.ObjectId, OpenMode.ForRead);

                                Point3dCollection acPts3d = new Point3dCollection();
                                foreach (ObjectId acObjIdVert in acPoly3d)
                                {
                                    PolylineVertex3d acPolVer3d;
                                    acPolVer3d = tr.GetObject(acObjIdVert, OpenMode.ForRead) as PolylineVertex3d;
                                    acPts3d.Add(acPolVer3d.Position);
                                }
                                foreach (Point3d pt in acPts3d)
                                {
                                    MoveEntitiy(tr, circleCenter, pt, ent2.ObjectId);
                                }


                                //#region Move Circle to Intersection of Polyline
                                //polyInters = acPoly3d.GetClosestPointTo(circleCenter, true);

                                //MoveEntitiy(tr, circleCenter, polyInters, ent2.ObjectId);

                                //Vector3d vec = polyInters - circleCenter;
                                //double apart = vec.Length - 1;

                                //if (apart > 0.00001)
                                //{
                                //    ent2.UpgradeOpen();
                                //    ent2.TransformBy(Matrix3d.Displacement(vec.GetNormal() * apart));
                                //}
                            }
                            //#endregion
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Autodesk.AutoCAD.Runtime.Exception ex2 = ex as Autodesk.AutoCAD.Runtime.Exception;
                    if (ex2 != null && ex2.ErrorStatus != ErrorStatus.WasOpenForUndo)
                    {
                        ed.WriteMessage("\nAutoCAD exception: {0}", ex2);
                    }
                    else if (ex2 == null)
                    {
                        ed.WriteMessage("\nSystem exception: {0}", ex);
                    }
                }
                tr.Commit();
            }
        }

        private bool GetPolylineInters(Entity ent, out object firstCenter, out double firstRadius)
        {
            throw new NotImplementedException();
        }

        // Helper function to get the center and radius
        // for all supported circular objects
        private bool GetCenterAndRadius(Entity ent, out Point3d center, out double radius)
        {
            // For circles it's easy...
            Circle circle = ent as Circle;
            if (circle != null)
            {
                center = circle.Center;
                radius = circle.Radius;
                return true;
            }
            else
            {
                // Throw in some empty values...
                // Returning false indicates the object
                // passed in was not useable
                center = Point3d.Origin;
                radius = 0.0;
                return false;
            }
        }
    }
}