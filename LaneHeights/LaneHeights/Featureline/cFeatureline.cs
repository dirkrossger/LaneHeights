using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Autodesk
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
#endregion

using LaneHeights;

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

    }
}
