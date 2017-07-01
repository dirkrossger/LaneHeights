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
using LaneHeights_Extensions;
using LaneHeights_Block;
using LaneHeights_Featureline;
using LaneHeights_Polyline;

[assembly: CommandClass(typeof(LaneHeights.Commands))]

namespace LaneHeights
{
    public class Commands
    {
        [CommandMethod("CB")]
        public void CreateBlock()
        {
            LaneHeights_Block.Create o = new Create();
            o.CreateBlock();
        }

        [CommandMethod("GF")]
        public void GetFeatureline()
        {
            LaneHeights_Featureline.Featureline o = new Featureline();
            o.SelectFeatureline("\nSelect a Featureline");
        }

        [CommandMethod("GP")]
        public void GetPolyline()
        {
            LaneHeights_Polyline.LwPolyline o = new LwPolyline();
            o.SelectPolyline("\nSelect a Polyline");
        }
    }
}
