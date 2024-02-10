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

namespace GripOverruleSample.AttMove
{
    public class AttGrip : GripData
    {
        public ObjectId AttributeId { set; get; } = ObjectId.Null;
        public Action<ObjectId> ClickAction { set; get; } = null;

        public AttGrip()
        {
            ForcedPickOn = true;
            GizmosEnabled = false;
            DrawAtDragImageGripPoint = false;
            IsPerViewport = false;
            ModeKeywordsDisabled = true;
            RubberBandLineDisabled = true;
            TriggerGrip = true;
            HotGripInvokesRightClick = false;
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
            points.Add(new Point3d(x - offset, y, 0.0));
            points.Add(new Point3d(x, y - offset, 0.0));
            points.Add(new Point3d(x + offset, y, 0.0));
            points.Add(new Point3d(x, y + offset, 0.0));

            worldDraw.SubEntityTraits.FillType = FillType.FillAlways;
            worldDraw.SubEntityTraits.Color = 3;
            worldDraw.Geometry.Polygon(points);

            return true;
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            if (ClickAction==null)
            {
                return ReturnValue.Ok;
            }

            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            using (dwg.LockDocument())
            {
                ClickAction(AttributeId);
            }

            return ReturnValue.GetNewGripPoints;
        }

        public override IEnumerable<IMenuItem> OnRightClick(
            GripDataCollection hotGrips, ObjectIdCollection entities)
        {
            return null;
        }
    }
}
