using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripOverruleSample.IncrementDrag
{
    public class IncrementDragGripOverrule : MyGripOverruleBase
    {
        private static IncrementDragGripOverrule _instance = null;
        public IncrementDragGripOverrule() :
            base(
                MyOverruleTypes.IncrementDragOverrule,
                new[] { RXClass.GetClass(typeof(Line)) } , 
                CustomFilter)
        {

        }
        public static IncrementDragGripOverrule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IncrementDragGripOverrule();
                }
                return _instance;
            }
        }

        public double Increment { get; set; } = 10.0;
        public int MaxIncrementCount { get; set; } = 10;

        public override void GetGripPoints(
            Entity entity,
            GripDataCollection grips,
            double curViewUnitSize,
            int gripSize,
            Vector3d curViewDir,
            GetGripPointsFlags bitFlags)
        {
            var curve = entity as Curve;
            if (curve != null)
            {
                using (var tran = entity.Database.TransactionManager.StartTransaction())
                {
                    var startPt=curve.StartPoint;
                    var endPt=curve.EndPoint;

                    var v = curve.GetFirstDerivative(curve.StartPoint).Negate();
                    Point3d gripPtAtStart = curve.StartPoint.TransformBy(
                        Matrix3d.Displacement(v * 0.01));
                    grips.Add(new IncrementDragGrip() { GripPoint = gripPtAtStart, DragIncrement=Increment });

                    v = curve.GetFirstDerivative(curve.EndPoint);
                    var gripPtAtEnd = curve.EndPoint.TransformBy(
                        Matrix3d.Displacement(v * 0.01));
                    grips.Add(new IncrementDragGrip() { GripPoint = gripPtAtEnd, DragIncrement = Increment });

                    tran.Commit();
                }

                // Also show regular grips
                base.GetGripPoints(
                    entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
                return;
            }

            base.GetGripPoints(
                entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }

        #region private methods

        private static bool CustomFilter(Entity ent)
        {
            var isTarget = false;
            if (ent is Line)
            {
                if (!ent.ExtensionDictionary.IsNull)
                {
                    using (var tran=ent.Database.TransactionManager.StartOpenCloseTransaction())
                    {
                        var entDict = (DBDictionary)tran.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                        if (entDict.Contains(MyOverruleTypes.IncrementDragOverrule.ToString()))
                        {
                            isTarget = true;
                        }
                        tran.Commit();
                    }
                }
            }
            return isTarget;
        }

        #endregion
    }

}
