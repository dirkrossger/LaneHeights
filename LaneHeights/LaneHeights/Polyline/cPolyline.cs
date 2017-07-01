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

namespace LaneHeights_Polyline
{
    class LwPolyline
    {
        public Polyline SelectPolyline(string message)
        {
            PromptEntityOptions options = new PromptEntityOptions(message);
            options.SetRejectMessage("\nThe selected object is not a Polyline!");
            options.AddAllowedClass(typeof(Polyline), false);
            PromptEntityResult peo = Active.Editor.GetEntity(options);

            Polyline result = null;

            using (Transaction tr = Active.Database.TransactionManager.StartTransaction())
            {
                switch (peo.Status)
                {
                    case PromptStatus.OK:
                        Polyline poly = tr.GetObject(peo.ObjectId, OpenMode.ForRead) as Polyline;
                        result = poly;
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
