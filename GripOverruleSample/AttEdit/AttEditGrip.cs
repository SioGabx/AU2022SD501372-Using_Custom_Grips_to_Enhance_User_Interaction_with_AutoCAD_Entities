using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace GripOverruleSample.AttEdit
{
    public class AttEditGrip : GripData
    {
        public ObjectId AttributeId { set; get; } = ObjectId.Null;
        public Action<ObjectId, IEnumerable<string>, string> UpdateFunction { set; get; } = null;

        public AttEditGrip()
        {
            ForcedPickOn = true;
            GizmosEnabled = false;
            DrawAtDragImageGripPoint = false;
            IsPerViewport = false;
            ModeKeywordsDisabled = true;
            RubberBandLineDisabled = true;
            TriggerGrip = true;
            HotGripInvokesRightClick = true;
            HotGripInvokesRightClick = false;
        }

        public override bool ViewportDraw(
            ViewportDraw worldDraw, 
            ObjectId entityId, 
            DrawType type, 
            Point3d? imageGripPoint, 
            int gripSizeInPixels)
        {
            var unit = worldDraw.Viewport.GetNumPixelsInUnitSquare(GripPoint);
            var gripHeight = 2.0 * gripSizeInPixels / unit.X;
            var points = new Point3dCollection();
            var x = GripPoint.X;
            var y = GripPoint.Y;
            var offset = gripHeight / 2.0;
            points.Add(new Point3d(x - offset * 0.8, y, 0.0));
            points.Add(new Point3d(x, y - offset * 1.2, 0.0));
            points.Add(new Point3d(x + offset * 0.8, y, 0.0));
            points.Add(new Point3d(x, y + offset * 1.2, 0.0));

            worldDraw.SubEntityTraits.FillType = FillType.FillAlways;
            worldDraw.SubEntityTraits.Color = 4;
            worldDraw.Geometry.Polygon(points);

            return true;
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            using (dwg.LockDocument())
            {
                var attData = AttEditGripOverrule.GetEditableAttributeData(AttributeId);
                if (attData.attValues.Length>0)
                {
                    UpdateFunction(AttributeId, attData.attValues, attData.attValue);
                }
            }

            return ReturnValue.Ok;
        }
    }
}
