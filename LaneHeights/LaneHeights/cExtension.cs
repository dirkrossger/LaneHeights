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
        }

        public void Terminate()
        {
        }
    }
}
