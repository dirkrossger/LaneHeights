using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
#endregion

using LaneHeights;
using LaneHeights_Layer;

namespace LaneHeights_Featureline
{
    class Featureline
    {
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

        public void CreateFromPolyline()
        {
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    PromptEntityOptions promptEntOp = new PromptEntityOptions("Select a Polyline : ");
                    PromptEntityResult promptEntRs = default(PromptEntityResult);
                    promptEntRs = Active.Editor.GetEntity(promptEntOp);
                    if (promptEntRs.Status != PromptStatus.OK)
                    {
                        Active.Editor.WriteMessage("Exiting! Try Again !");
                        return;
                    }

                    ObjectId idEnt = default(ObjectId);
                    idEnt = promptEntRs.ObjectId;
                    ObjectId oFtrLn = FeatureLine.Create("Test", idEnt);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    Active.Editor.WriteMessage("Error : ", ex.Message);
                }
            }
        }
    }
}
