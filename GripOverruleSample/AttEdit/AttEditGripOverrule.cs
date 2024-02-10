using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace GripOverruleSample.AttEdit
{
    public class AttEditGripOverrule : MyGripOverruleBase
    {
        private static string _xDataApp = MyOverruleTypes.AttEditGripOverrule.ToString();

        private static AttEditGripOverrule _instance = null;

        public AttEditGripOverrule() :
            base(
                MyOverruleTypes.AttMoveGripOverrule,
                new[] { RXClass.GetClass(typeof(BlockReference)) },
                AttEditGripOverrule.IsTargetBlock)
        {

        }
        public static AttEditGripOverrule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AttEditGripOverrule();
                }
                return _instance;
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

                        var attGrip = new AttEditGrip()
                        {
                            GripPoint = att.Position,
                            AttributeId = id,
                            UpdateFunction=AttEditHelper.UpdateAttribute
                        };

                        grips.Add(attGrip);
                        
                    }

                    tran.Commit();
                }
            }
                
            base.GetGripPoints(entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }

        #region static methods to add data to attribute

        public static void SetEditableAttributeData(ObjectId attributeDefinitionId, string[] attDataList)
        {
            using (var tran=attributeDefinitionId.Database.TransactionManager.StartTransaction())
            {
                var att = (AttributeDefinition)tran.GetObject(attributeDefinitionId, OpenMode.ForWrite);
                var vals = new List<TypedValue>();
                vals.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, _xDataApp));
                foreach (string dataItem in attDataList)
                {
                    vals.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, dataItem));
                }

                GenericHelper.EnsureRegApp(_xDataApp, attributeDefinitionId.Database, tran);
                att.XData=new ResultBuffer(vals.ToArray());

                tran.Commit();
            }
        }

        public static (string attValue, string[] attValues) GetEditableAttributeData(ObjectId attReferenceId)
        {
            var data = new List<string>();
            var attValue = "";

            if (attReferenceId.ObjectClass.DxfName.ToUpper()!="ATTRIB")
            {
                return ("", data.ToArray());
            }

            using (var tran = attReferenceId.Database.TransactionManager.StartTransaction())
            {
                var att = (AttributeReference)tran.GetObject(attReferenceId, OpenMode.ForWrite);
                var xData = att.GetXDataForApplication(_xDataApp);
                if (xData!=null)
                {
                    attValue = att.TextString;

                    var vals = xData.AsArray();
                    for (int i=1; i<vals.Length; i++)
                    {
                        data.Add(vals[i].Value.ToString());
                    }
                }
                tran.Commit();
            }

            return (attValue, data.ToArray());
        }

        public static void ClearEditableAttributeData(ObjectId attributeDefinitionId)
        {
            using (var tran = attributeDefinitionId.Database.TransactionManager.StartTransaction())
            {
                var att = (AttributeDefinition)tran.GetObject(attributeDefinitionId, OpenMode.ForWrite);
                var vals = new TypedValue[]
                {
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, _xDataApp)
                };
                att.XData = new ResultBuffer(vals);
                tran.Commit();
            }
        }

        #endregion

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
                        if (xData!=null)
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
