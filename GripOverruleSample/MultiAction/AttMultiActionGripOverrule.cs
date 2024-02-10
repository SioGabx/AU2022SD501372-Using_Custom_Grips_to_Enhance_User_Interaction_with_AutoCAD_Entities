using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using static Autodesk.AutoCAD.DatabaseServices.GripData;

namespace GripOverruleSample.MultiAction
{
    public class AttMultiActionGripOverrule : MyGripOverruleBase
    {
        private static string _xDataApp = MyOverruleTypes.MultiActionOverrule.ToString();
        private static AttMultiActionGripOverrule _instance = null;

        public AttMultiActionGripOverrule() 
            : base(
                  MyOverruleTypes.MultiActionOverrule, 
                  new[] { RXClass.GetClass(typeof(BlockReference)) },
                  AttMultiActionGripOverrule.IsTargetBlock)
        {
            var overruled = new OverruledBlock();
            Overrule.GetClass(typeof(BlockReference)).AddX(GetClass(typeof(OverruledBlock)), overruled);
        }

        public static AttMultiActionGripOverrule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AttMultiActionGripOverrule();
                }
                return _instance;
            }
        }

        private class OverruledBlock : MultiModesGripPE
        {
            public override GripMode CurrentMode(Entity entity, GripData gripData)
            {
                var grip = gripData as AttMultiActionGrip;
                if (grip == null) return null;
                var index = (int)grip.CurrentModeId - (int)GripMode.ModeIdentifier.CustomStart;
                return grip.GripModes[index];
            }

            public override uint CurrentModeId(Entity entity, GripData gripData)
            {
                var grip = gripData as AttMultiActionGrip;
                if (grip != null) return (uint)grip.CurrentModeId;
                return 0;
            }

            public override bool GetGripModes(
                Entity entity, GripData gripData, GripModeCollection modes, ref uint curMode)
            {
                if (!(gripData is AttMultiActionGrip)) return false;
                return ((AttMultiActionGrip)gripData).GetGripModes(ref modes, ref curMode);
            }

            public override GripType GetGripType(Entity entity, GripData gripData)
            {
                return (gripData is AttMultiActionGrip) ? GripType.Secondary : GripType.Primary;
            }

            public override bool SetCurrentMode(Entity entity, GripData gripData, uint curMode)
            {
                if (!(gripData is AttMultiActionGrip)) return false;
                ((AttMultiActionGrip)gripData).CurrentModeId = (GripMode.ModeIdentifier)curMode;
                return true;
            }

            public override void Reset(Entity entity)
            {
            }
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
                        var xData = att.GetXDataForApplication(_xDataApp);
                        if (xData == null) continue;

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
                        var attGrip = new AttMultiActionGrip()
                        {
                            GripPoint = pt.HasValue?pt.Value:att.Position,
                            AttributeId = id,
                            AttHeightAction = GenericHelper.ChangeAttributeHeight
                        };

                        grips.Add(attGrip);

                    }

                    tran.Abort();
                }
            }

            

            base.GetGripPoints(entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }

        public override void MoveGripPointsAt(
            Entity entity, GripDataCollection grips, Vector3d offset, MoveGripPointsFlags bitFlags)
        {
            var ed = CadApp.DocumentManager.MdiActiveDocument.Editor;
            foreach (var grip in grips)
            {
                var attGrip = grip as AttMultiActionGrip;
                if (attGrip != null)
                {
                    attGrip.ChangeAttribute(attGrip.AttributeId);
                }
                else
                {
                    base.MoveGripPointsAt(entity, grips, offset, bitFlags);
                }
            }

        }

        public static void ApplyMultiGripOverrule(ObjectId attId)
        {
            using (var tran=attId.Database.TransactionManager.StartTransaction())
            {
                GenericHelper.EnsureRegApp(_xDataApp, attId.Database, tran);

                var att = (Entity)tran.GetObject(attId, OpenMode.ForWrite);
                var vals = new TypedValue[]
                {
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, _xDataApp),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString,"MultiGripped Attribute")
                };
                att.XData = new ResultBuffer(vals);

                tran.Commit();
            }
        }

        public static void RemoveMultiGripOverrule(ObjectId attId)
        {
            using (var tran = attId.Database.TransactionManager.StartTransaction())
            {
                GenericHelper.EnsureRegApp(_xDataApp, attId.Database, tran);

                var att = (Entity)tran.GetObject(attId, OpenMode.ForWrite);
                var vals = new TypedValue[]
                {
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, _xDataApp)
                };
                att.XData = new ResultBuffer(vals);

                tran.Commit();
            }
        }


        #region private methods

        private static bool IsTargetBlock(Entity ent)
        {
            var isTarget = false;
            var blk = ent as BlockReference;
            if (blk != null && blk.AttributeCollection.Count > 0)
            {

                using (var tran = ent.Database.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId attId in blk.AttributeCollection)
                    {
                        var att = (AttributeReference)tran.GetObject(attId, OpenMode.ForRead);
                        var xData = att.GetXDataForApplication(_xDataApp);
                        if (xData != null)
                        {
                            isTarget = true;
                            break;
                        }
                    }
                    tran.Commit();
                }
            }
            return isTarget;
        }

        #endregion
    }
}
