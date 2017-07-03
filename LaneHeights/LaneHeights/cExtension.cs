using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using LaneHeights;

namespace LaneHeights_Extensions
{
    public class LaneHeights : IExtensionApplication
    {
        public void Initialize()
        {
            Active.Editor.WriteMessage("\n-> Create Block Start with cb");
            Active.Editor.WriteMessage("\n-> Get Featureline Start with gf");
            Active.Editor.WriteMessage("\n-> Get Polyline Start with gp");
            Active.Editor.WriteMessage("\n-> Create Featureline Start with cf");
            Active.Editor.WriteMessage(
                "\n** Linking objects commands" +
                "\n LINK ... linking objects" +
                "\n LOADLINKS ... " +
                "\n SAVELINKS ..."

                                      );
        }

        public void Terminate()
        {
        }
    }
}
