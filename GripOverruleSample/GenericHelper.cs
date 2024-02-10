using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace GripOverruleSample
{
    public class GenericHelper
    {
        public static void MoveAttribute(ObjectId attId)
        {
            var ed = CadApp.DocumentManager.MdiActiveDocument.Editor;

            using (var tran = attId.Database.TransactionManager.StartTransaction())
            {
                var att = (AttributeReference)tran.GetObject(attId, OpenMode.ForRead);
                att.Highlight();
                Point3d basePt = att.Position;
                var opt = new PromptPointOptions("\nPick new position for selected attribute");
                opt.UseBasePoint = true;
                opt.BasePoint = basePt;
                opt.UseDashedLine = true;

                var res = ed.GetPoint(opt);
                att.Unhighlight();

                if (res.Status == PromptStatus.OK)
                {
                    att.UpgradeOpen();
                    att.TransformBy(Matrix3d.Displacement(basePt.GetVectorTo(res.Value)));
                }

                tran.Commit();
            }
        }

        public static bool SetAttributeEditableDataList()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var selected = SelectAttributeInBlockDefinition(dwg);
            if (selected.IsNull) return false;

            var data = new List<string>();
            var count = 1;
            while(true)
            {
                var opt = new PromptStringOptions(
                    $"\nEnter attribute data item {count}:");
                opt.AllowSpaces = true;
                var res = ed.GetString(opt);
                if (res.Status== PromptStatus.OK)
                {
                    if (!string.IsNullOrEmpty(res.StringResult))
                    {
                        data.Add(res.StringResult);
                        count++;
                    }

                    var kOpt = new PromptKeywordOptions(
                        "\nEnter next data item?");
                    kOpt.AllowNone = false;
                    kOpt.Keywords.Add("Yes");
                    kOpt.Keywords.Add("No");
                    var kres = ed.GetKeywords(kOpt);
                    if (kres.Status== PromptStatus.OK)
                    {
                        
                        if (kres.StringResult == "No") break;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (data.Count == 0) return false;

            AttEdit.AttEditGripOverrule.SetEditableAttributeData(selected, data.ToArray());

            return true;
        }

        public static void ClearAttributeEditableDataList()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var selected = SelectAttributeInBlockDefinition(dwg);
            if (selected.IsNull) return;

            AttEdit.AttEditGripOverrule.ClearEditableAttributeData(selected);
        }

        public static bool SetMultiActionGripToAttribute()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var selected = SelectAttributeInBlockDefinition(dwg);
            if (selected.IsNull) return false;

            MultiAction.AttMultiActionGripOverrule.ApplyMultiGripOverrule(selected);
            return true;
        }

        public static bool RemoveMultiActionGripFromAttribute()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var selected = SelectAttributeInBlockDefinition(dwg);
            if (selected.IsNull) return false;

            MultiAction.AttMultiActionGripOverrule.RemoveMultiGripOverrule(selected);
            return true;
        }

        public static void ChangeAttributeHeight(ObjectId attId, bool increase)
        {
            using (var tran = attId.Database.TransactionManager.StartTransaction())
            {
                var att = tran.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                if (att != null)
                {
                    var h = att.Height;
                    if (increase)
                    {
                        h *= 1.25;
                    }
                    else
                    {
                        h *= 0.8;
                    }

                    att.Height = h;
                }
                tran.Commit();
            }
        }

        public static void ToggleAttributeVisibility(ObjectId attId)
        {
            using (var tran = attId.Database.TransactionManager.StartTransaction())
            {
                var att = tran.GetObject(attId, OpenMode.ForWrite) as AttributeReference;
                if (att != null)
                {
                    if (att.Invisible)
                    {
                        att.Invisible = false;
                    }
                    else
                    {
                        att.Invisible = true;
                    }
                }
                tran.Commit();
            }
        }

        public static void EnsureRegApp(string xDataApp, Database db, Transaction tran)
        {
            var regTable = (RegAppTable)tran.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!regTable.Has(xDataApp))
            {
                var regApp = new RegAppTableRecord();
                regApp.Name = xDataApp;
                regTable.UpgradeOpen();
                regTable.Add(regApp);
                tran.AddNewlyCreatedDBObject(regApp, true);
            }
        }

        #region private methods

        private static ObjectId SelectAttributeInBlockDefinition(Document dwg)
        {
            var attId = ObjectId.Null;

            var res = dwg.Editor.GetString("\nEnter block name:");
            if (res.Status == PromptStatus.OK)
            {
                using (var tran=dwg.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tran.GetObject(dwg.Database.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(res.StringResult))
                    {
                        var blk = (BlockTableRecord)tran.GetObject(bt[res.StringResult], OpenMode.ForRead);
                        if (blk.HasAttributeDefinitions)
                        {
                            var tags = new List<(string, ObjectId)>();
                            foreach (ObjectId id in blk)
                            {
                                var att = tran.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                                if (att!=null && !att.Constant)
                                {
                                    tags.Add((att.Tag.ToString(), id));
                                }
                            }

                            var attNames = string.Join(", ", (from t in tags select t.Item1).ToArray());
                            while (true)
                            {
                                var sRes = dwg.Editor.GetString(
                                    $"\nEnter att's tag({attNames}):");
                                if (sRes.Status == PromptStatus.OK)
                                {
                                    foreach (var t in tags)
                                    {
                                        if (t.Item1.ToUpper()==sRes.StringResult.ToUpper())
                                        {
                                            attId = t.Item2;
                                        }
                                    }
                                    if (!attId.IsNull)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        dwg.Editor.WriteMessage("\nWrong attribut tag.");
                                    }
                                }
                                else
                                {
                                    dwg.Editor.WriteMessage("\n*Cancel*\n");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            dwg.Editor.WriteMessage("\nThe block has no attribute.");
                        }
                    }
                    else
                    {
                        dwg.Editor.WriteMessage($"\nNo block found with bane \"{res.StringResult}\".");
                    }
                    tran.Commit();
                }
            }

            return attId;
        }

        #endregion
    }
}
