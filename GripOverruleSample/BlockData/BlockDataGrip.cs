using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace GripOverruleSample.BlockData
{
    public class BlockDataGrip : GripData
    {
        private readonly GripModeCollection _gripModes;
        private const short GRIP_COLOR = 60;
        private GripMode.ModeIdentifier _currentModeId = GripMode.ModeIdentifier.CustomStart;

        public BlockDataGrip()
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

        public ObjectId BlockId { set; get; } = ObjectId.Null;
        public List<(string project, string location, string client)> ProjectData { set; get; } = null;
        public GripModeCollection GripModes => _gripModes;
        public virtual GripMode.ModeIdentifier CurrentModeId
        {
            get => _currentModeId;
            set => _currentModeId = value;
        }

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
            if (ProjectData!=null)
            {
                uint modeId = 0;
                foreach (var project in ProjectData)
                {
                    var gripMode = new GripMode()
                    {
                        ModeId = modeId,
                        DisplayString = $"Project: {project.project}",
                        Action = GripMode.ActionType.Immediate,
                        ToolTip = $"Project name: {project.project}"
                    };
                    modes.Add(gripMode);
                    modeId++;
                }
            }
            return true;
        }

        public void UpdateAttributes()
        {
            var projData = ProjectData[(int)CurrentModeId];
            using (var tran = BlockId.Database.TransactionManager.StartTransaction())
            {
                var blk = (BlockReference)tran.GetObject(BlockId, OpenMode.ForRead);
                foreach (ObjectId id in blk.AttributeCollection)
                {
                    var att = (AttributeReference)tran.GetObject(id, OpenMode.ForWrite);
                    if (att.Tag.ToUpper() == "PROJECT") att.TextString = projData.project;
                    if (att.Tag.ToUpper() == "LOCATION") att.TextString = projData.location;
                    if (att.Tag.ToUpper() == "CLIENT") att.TextString = projData.client;
                }
                tran.Commit();
            }
        }
    }
}
