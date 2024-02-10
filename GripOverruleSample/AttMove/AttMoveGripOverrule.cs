using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace GripOverruleSample.AttMove
{
    public class AttMoveGripOverrule : MyGripOverruleBase
    {
        private static AttMoveGripOverrule _instance = null;
        public AttMoveGripOverrule() : 
            base(
                MyOverruleTypes.AttMoveGripOverrule, 
                new[] { RXClass.GetClass(typeof(BlockReference)) }, 
                AttMoveGripOverrule.IsTargetBlock )
        {

        }
        public static AttMoveGripOverrule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AttMoveGripOverrule();
                }
                return _instance;
            }
        }

        private static bool IsTargetBlock(Entity ent)
        {
            var blk = ent as BlockReference;
            if (blk!=null && blk.AttributeCollection.Count>0)
            {

                return true;
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
                        if (att.ExtensionDictionary.IsNull) continue;

                        var extDict = (DBDictionary)tran.GetObject(att.ExtensionDictionary, OpenMode.ForRead);
                        if (extDict.Contains(MyOverruleTypes.AttMoveGripOverrule.ToString()))
                        {
                            var attGrip = new AttGrip()
                            {
                                GripPoint = att.Position,
                                AttributeId = id,
                                ClickAction = GenericHelper.MoveAttribute
                            };

                            grips.Add(attGrip);
                        }
                    }

                    tran.Commit();
                }
            }

            base.GetGripPoints(
                entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }
    }
}
