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

namespace GripOverruleSample.MultiAction
{
    public class AttMultiActionGrip : GripData
    {
        public const string USERDATA_KEY= "AttMultiActionGripData";
        public static ObjectId _commandTargetId = ObjectId.Null;
        private readonly GripModeCollection _gripModes;
        private const short GRIP_COLOR = 41;
        private GripMode.ModeIdentifier _currentModeId = GripMode.ModeIdentifier.CustomStart;

        public AttMultiActionGrip()
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

            _gripModes = new GripModeCollection();
        }

        public virtual GripMode.ModeIdentifier CurrentModeId
        {
            get => _currentModeId;
            set => _currentModeId = value;
        }

        public static ObjectId CommandTargetId => _commandTargetId;

        public GripModeCollection GripModes => _gripModes;
        public ObjectId AttributeId { set; get; } = ObjectId.Null;
        public Action<ObjectId, bool> AttHeightAction { set; get; } = null;

        public override bool ViewportDraw(
            ViewportDraw worldDraw,
            ObjectId entityId,
            DrawType type,
            Point3d? imageGripPoint,
            int gripSizeInPixels)
        {
            var unit = worldDraw.Viewport.GetNumPixelsInUnitSquare(GripPoint);
            var gripHeight = 3.0 * gripSizeInPixels / unit.X;
            var points = new Point3dCollection();
            var x = GripPoint.X;
            var y = GripPoint.Y;
            var offset = gripHeight / 2.0;

            points.Add(new Point3d(x - offset, y, 0.0));
            points.Add(new Point3d(x, y - offset, 0.0));
            points.Add(new Point3d(x + offset, y, 0.0));
            points.Add(new Point3d(x, y + offset, 0.0));

            Point3d center = new Point3d(x, y, 0.0);
            var radius = offset;

            worldDraw.SubEntityTraits.FillType = FillType.FillAlways;
            worldDraw.SubEntityTraits.Color = GRIP_COLOR;
            worldDraw.Geometry.Circle(center, radius, Vector3d.ZAxis);

            return true;
        }

        public override ReturnValue OnHotGrip(ObjectId entityId, Context contextFlags)
        {
            return base.OnHotGrip(entityId, contextFlags);
        }

        public bool GetGripModes(ref GripModeCollection modes, ref uint curMode)
        {
            var gripMode = new GripMode()
            {
                ModeId = 0,
                DisplayString = "Increase Height",
                Action = GripMode.ActionType.Immediate,
                ToolTip = "Increase attribute height"
            };
            modes.Add(gripMode);
            
            gripMode = new GripMode()
            {
                ModeId = 1,
                DisplayString = "Reduce Height",
                Action = GripMode.ActionType.Immediate,
                ToolTip = "Reduce attribute height"
            };
            modes.Add(gripMode);

            gripMode = new GripMode()
            {
                ModeId = 2,
                DisplayString = "Toggle Visibility",
                Action = GripMode.ActionType.Command,
                CommandString = "ToggleAttributeVisible ",
                ToolTip = "Show/hide attribute"
            };
            modes.Add(gripMode);
            _commandTargetId = AttributeId;

            return true;
        }

        public void ChangeAttribute(ObjectId attId)
        {
            if (AttHeightAction == null) return;

            switch((int)CurrentModeId)
            {
                case 0:
                    AttHeightAction(AttributeId, true);
                    break;
                case 1:
                    AttHeightAction(AttributeId, false);
                    break;
            }
        }
    }
}
