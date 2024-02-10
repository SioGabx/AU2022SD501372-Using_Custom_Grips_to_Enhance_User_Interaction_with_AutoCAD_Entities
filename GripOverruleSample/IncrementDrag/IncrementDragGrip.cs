using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace GripOverruleSample.IncrementDrag
{
    public class IncrementDragGrip : GripData
    {
        public IncrementDragGrip()
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

        public double DragIncrement { set; get; } = 100.0;
        
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
            worldDraw.SubEntityTraits.Color = 2;
            worldDraw.Geometry.Polygon(points);

            return true;
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            using (dwg.LockDocument())
            {
                var jig = new IncrementDrag.LineIncrementJig(dwg, entityId, GripPoint);
                jig.Drag(DragIncrement);
            }
            CadApp.UpdateScreen();
            return ReturnValue.GetNewGripPoints;
        }
    }
}
