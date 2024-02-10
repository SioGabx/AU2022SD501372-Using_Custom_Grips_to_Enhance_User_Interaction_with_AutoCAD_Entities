using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace GripOverruleSample.BlockData
{
    public class BlockDataGripOverrule : MyGripOverruleBase
    {
        private static BlockDataGripOverrule _instance = null;
        public BlockDataGripOverrule() :
            base(
                MyOverruleTypes.AttMoveGripOverrule,
                new[] { RXClass.GetClass(typeof(BlockReference)) },
                BlockDataGripOverrule.IsTargetBlock)
        {
            var overruled = new OverruledBlock();
            Overrule.GetClass(typeof(BlockReference)).AddX(GetClass(typeof(OverruledBlock)), overruled);
        }

        public static BlockDataGripOverrule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BlockDataGripOverrule();
                }
                return _instance;
            }
        }

        public List<(string project, string location, string client)> ProjectData 
            { set; get; } = null;

        private class OverruledBlock : MultiModesGripPE
        {
            public override GripMode CurrentMode(Entity entity, GripData gripData)
            {
                var grip = gripData as BlockDataGrip;
                if (grip == null) return null;
                var index = (int)grip.CurrentModeId - (int)GripMode.ModeIdentifier.CustomStart;
                return grip.GripModes[index];
            }

            public override uint CurrentModeId(Entity entity, GripData gripData)
            {
                var grip = gripData as BlockDataGrip;
                if (grip != null) return (uint)grip.CurrentModeId;
                return 0;
            }

            public override bool GetGripModes(
                Entity entity, GripData gripData, GripModeCollection modes, ref uint curMode)
            {
                if (!(gripData is BlockDataGrip)) return false;
                return ((BlockDataGrip)gripData).GetGripModes(ref modes, ref curMode);
            }

            public override GripType GetGripType(Entity entity, GripData gripData)
            {
                return (gripData is BlockDataGrip) ? GripType.Secondary : GripType.Primary;
            }

            public override bool SetCurrentMode(Entity entity, GripData gripData, uint curMode)
            {
                if (!(gripData is BlockDataGrip)) return false;
                ((BlockDataGrip)gripData).CurrentModeId = (GripMode.ModeIdentifier)curMode;
                return true;
            }

            public override void Reset(Entity entity)
            {
            }
        }

        private static bool IsTargetBlock(Entity ent)
        {
            var blk = ent as BlockReference;
            if (blk != null && blk.AttributeCollection.Count > 0)
            {
                using (var tran = blk.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in blk.AttributeCollection)
                    {
                        var att = (AttributeReference)tran.GetObject(id, OpenMode.ForRead);
                        if (att.Tag.ToUpper() == "GRIPUPDATE" &&
                            att.Invisible &&
                            att.TextString.ToUpper()=="YES")
                        {
                            return true;
                        }
                    }
                    tran.Abort();
                }
            }
            return false;
        }

        public override void GetGripPoints(
            Entity entity,
            GripDataCollection grips,
            double curViewUnitSize,
            int gripSize,
            Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            var blk = entity as BlockReference;
            if (blk != null && blk.AttributeCollection.Count > 0)
            {
                using (var tran = blk.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in blk.AttributeCollection)
                    {
                        var att = (AttributeReference)tran.GetObject(
                            id, OpenMode.ForRead);

                        if (att.Tag.ToUpper() != "PROJECT") continue;

                        Func<AttributeReference, Point3d?> GetAttributeCenter = (attRef) =>
                        {
                            if (!string.IsNullOrEmpty(attRef.TextString))
                            {
                                var ext = attRef.GeometricExtents;
                                return ext.MaxPoint;
                            }
                            else
                            {
                                return null;
                            }
                        };

                        var pt = GetAttributeCenter(att);
                        var attGrip = new BlockDataGrip()
                        {
                            GripPoint = pt.HasValue ? pt.Value : att.Position,
                            ProjectData = ProjectData,
                            BlockId = blk.ObjectId
                        };

                        grips.Add(attGrip);
                    }

                    tran.Commit();
                }
            }

            base.GetGripPoints(
                entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }

        public override void MoveGripPointsAt(
            Entity entity, 
            GripDataCollection grips, 
            Vector3d offset, 
            MoveGripPointsFlags bitFlags)
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            foreach (var grip in grips)
            {
                var attGrip = grip as BlockDataGrip;
                if (attGrip != null)
                {
                    attGrip.UpdateAttributes();
                }
                else
                {
                    base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                }
            }

        }
    }
}
