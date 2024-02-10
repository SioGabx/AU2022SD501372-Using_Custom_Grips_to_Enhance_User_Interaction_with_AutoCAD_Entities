using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace GripOverruleSample
{
    public enum MyOverruleTypes
    {
        AttMoveGripOverrule = 0,
        AttEditGripOverrule = 1,
        IncrementDragOverrule = 2,
        MultiActionOverrule = 3,
    }

    public class SetOverruledEntityHelper
    {
        public static void SetOverruleXDictionary(ObjectId entId, MyOverruleTypes orType)
        {
            var db = entId.Database;
            using (var tran = db.TransactionManager.StartTransaction())
            {
                var att = (Entity)tran.GetObject(entId, OpenMode.ForRead);
                if (att.ExtensionDictionary.IsNull)
                {
                    att.UpgradeOpen();
                    att.CreateExtensionDictionary();
                }

                var attDict = (DBDictionary)tran.GetObject(att.ExtensionDictionary, OpenMode.ForRead);
                if (!attDict.Contains(orType.ToString()))
                {
                    attDict.UpgradeOpen();
                    var dict = new DBDictionary();
                    attDict.SetAt(orType.ToString(), dict);
                    tran.AddNewlyCreatedDBObject(dict, true);
                }
                
                tran.Commit();
            }
        }

        public static void ClearOverruleXDictionary(ObjectId entId, MyOverruleTypes orType)
        {
            var db = entId.Database;
            using (var tran = db.TransactionManager.StartTransaction())
            {
                var att = (Entity)tran.GetObject(entId, OpenMode.ForRead);
                if (!att.ExtensionDictionary.IsNull)
                {
                    var attDict = (DBDictionary)tran.GetObject(
                        att.ExtensionDictionary, OpenMode.ForRead);

                    if (attDict.Contains(orType.ToString()))
                    {
                        var dict=(DBDictionary)tran.GetObject(
                            attDict.GetAt(orType.ToString()), OpenMode.ForWrite);

                        attDict.UpgradeOpen();
                        attDict.Remove(dict.ObjectId);

                        dict.Erase();
                    }
                }

                tran.Commit();
            }
        }
    }
}
