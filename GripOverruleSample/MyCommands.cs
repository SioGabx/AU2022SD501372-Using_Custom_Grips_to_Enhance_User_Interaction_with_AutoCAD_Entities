using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(GripOverruleSample.MyCommands))]
[assembly: ExtensionApplication(typeof(GripOverruleSample.MyCommands))]

namespace GripOverruleSample
{
    public class MyCommands : IExtensionApplication
    {
        public void Initialize()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            try
            {
                ed.WriteMessage($"\nLoading custom addin: GripOverruleSample app...");
                ed.WriteMessage("done!\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nInitializing addin error:\n{ex.Message}\n");
            }
        }

        public void Terminate()
        {

        }

        #region Polygon grip overrule

        private static bool _polyGripEnabled = false;
        [CommandMethod("PolygonGripOr")]
        public static void EnablePolygonGripOverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_polyGripEnabled)
            {
                var opt = new PromptKeywordOptions(
                    "\nShow custom polygon grip only:");
                opt.AppendKeywordsToMessage = true;
                opt.Keywords.Add("Yes");
                opt.Keywords.Add("No");
                opt.Keywords.Default = "No";
                var res = ed.GetKeywords(opt);
                if (res.Status == PromptStatus.OK)
                {
                    Polygon.PolygonGripOverrule.Instance.HideOriginals = res.StringResult =="Yes";
                    Polygon.PolygonGripOverrule.Instance.EnableOverrule(true);
                    _polyGripEnabled = true;
                    ed.WriteMessage("\nPolygonGripOeverrule is enabled.\n");
                }
                else
                {
                    ed.WriteMessage("\n*Cancel*\n");
                }
            }
            else
            {
                Polygon.PolygonGripOverrule.Instance.EnableOverrule(false);
                _polyGripEnabled = false;
                ed.WriteMessage("\nPolygonGripOeverrule is disabled.\n");
            }
        }


        #endregion

        #region Att Move Grip

        private static bool _attMoveEnabled = false;
        [CommandMethod("AttMoveOr")]
        public static void EnableAttributeMoveGripOverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_attMoveEnabled)
            {
                AttMove.AttMoveGripOverrule.Instance.EnableOverrule(true);
                _attMoveEnabled = true;
                ed.WriteMessage("\nAttributeMoveGripOeverrule is enabled.\n");
            }
            else
            {
                AttMove.AttMoveGripOverrule.Instance.EnableOverrule(false);
                _attMoveEnabled = false;
                ed.WriteMessage("\nAttributeMoveGripOeverrule is disabled.\n");
            }
        }


        [CommandMethod("ApplyAttMoveOr")]
        public static void ApplyAttMoveOverruleTo()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var entId = SelectAttribute(ed, "Select an attribute in a block to apply AttMoveGripOverrule:");
            if (!entId.IsNull)
            {
                SetOverruledEntityHelper.SetOverruleXDictionary(entId, MyOverruleTypes.AttMoveGripOverrule);
            }
        }

        [CommandMethod("ClearAttMoveOr")]
        public static void ClearAttMoveOrverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var entId = SelectAttribute(ed, "Select an attribute in a block to unapply AttMoveGripOverrule:");
            if (!entId.IsNull)
            {
                SetOverruledEntityHelper.ClearOverruleXDictionary(entId, MyOverruleTypes.AttMoveGripOverrule);
            }
        }

        #endregion

        #region Increment Grip overrule

        private static bool _incrmentDrag = false;
        [CommandMethod("IncrementDragOr")]
        public static void EnableIncrementDrapGripOverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_incrmentDrag)
            {
                var opt = new PromptDoubleOptions(
                    "\nEnter drag incremenal distance:");
                opt.AllowNegative = false;
                opt.AllowZero = false;
                opt.DefaultValue = 100.0;

                var res = ed.GetDouble(opt);
                if (res.Status== PromptStatus.OK)
                {
                    IncrementDrag.IncrementDragGripOverrule.Instance.Increment = res.Value;
                }
                else
                {
                    ed.WriteMessage("\n*Cancel*\n");
                    return;
                }

                IncrementDrag.IncrementDragGripOverrule.Instance.EnableOverrule(true);
                _incrmentDrag = true;
                ed.WriteMessage("\nIncrementDragGripOverrule is enabled.\n");
            }
            else
            {
                IncrementDrag.IncrementDragGripOverrule.Instance.EnableOverrule(false);
                _incrmentDrag = false;
                ed.WriteMessage("\nIncrementDragGripOverrule is disabled.\n");
            }
        }

        [CommandMethod("ApplyIncrementDragOr")]
        public static void ApplyIncrementOverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var lineId = SelectEntity(
                ed, "Select a line to apply IncrementDragGripOverrule", new[] { typeof(Line) });
            if (!lineId.IsNull)
            {
                SetOverruledEntityHelper.SetOverruleXDictionary(lineId, MyOverruleTypes.IncrementDragOverrule);
            }
        }

        [CommandMethod("ClearIncrementDragOr")]
        public static void ClearIncrementOverrule()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var lineId = SelectEntity(
                ed, "Select a line to unapply IncrementDragGripOverrule", new[] { typeof(Line) });
            if (!lineId.IsNull)
            {
                SetOverruledEntityHelper.ClearOverruleXDictionary(lineId, MyOverruleTypes.IncrementDragOverrule);
            }
        }

        #endregion

        #region Attribute editing grip overrule

        private static bool _attEdit = false;
        [CommandMethod("AttEditOr")]
        public static void EnableAttEditGrip()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_attEdit)
            {
                AttEdit.AttEditGripOverrule.Instance.EnableOverrule(true);
                _attEdit = true;
                ed.WriteMessage("\nAttEditGripOverrule is enabled.\n");
            }
            else
            {
                AttEdit.AttEditGripOverrule.Instance.EnableOverrule(false);
                _attEdit = false;
                ed.WriteMessage("\nAttEditGripOverrule is disabled.\n");
            }
        }

        [CommandMethod("ApplyAttEdit")]
        public static void ApplyAttEditGripToAttribute()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            if (!GenericHelper.SetAttributeEditableDataList())
            {
                ed.WriteMessage("\n*Cancel*\n");
            }
        }

        [CommandMethod("ClearAttEdit")]
        public static void ClearAttEditGripFromAttibute()
        {
            GenericHelper.ClearAttributeEditableDataList();
        }

        #endregion

        #region Multiple action grip overrule

        private static bool _attMulti = false;
        [CommandMethod("AttMultiGripOr")]
        public static void EnableAttMultiGrip()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_attMulti)
            {
                MultiAction.AttMultiActionGripOverrule.Instance.EnableOverrule(true);
                _attMulti = true;
                ed.WriteMessage("\nAttMultiActionGripOverrule is enabled.\n");
            }
            else
            {
                MultiAction.AttMultiActionGripOverrule.Instance.EnableOverrule(false);
                _attMulti = false;
                ed.WriteMessage("\nAttMultiActionGripOverrule is disabled.\n");
            }
        }

        [CommandMethod("SetMultiGripAtt")]
        public static  void SetMultiGripToAtt()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            if (!GenericHelper.SetMultiActionGripToAttribute())
            {
                ed.WriteMessage("\n*Cancel*\n");
            }
        }

        [CommandMethod("ClearMultiGridAtt")]
        public static void ClearMultiGripFromAtt()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            if (!GenericHelper.RemoveMultiActionGripFromAttribute())
            {
                ed.WriteMessage("\n*Cancel*\n");
            }
        }

        [CommandMethod("ToggleAttributeVisible", CommandFlags.Redraw | CommandFlags.NoHistory)]
        public static void MoveAttribute()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;

            ObjectId targetId = MultiAction.AttMultiActionGrip.CommandTargetId;
            if (targetId.IsNull) return;
            if (targetId.Database.FingerprintGuid != dwg.Database.FingerprintGuid) return;

            GenericHelper.ToggleAttributeVisibility(targetId);
        }

        #endregion

        #region block data grip

        private static bool _blkDataEnabled = false;
        [CommandMethod("BlkDataGripOr")]
        public static void EnableBlockDataGrip()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            if (!_blkDataEnabled)
            {
                // supply data to the grip overrule
                var projectData = GetProjectData();
                BlockData.BlockDataGripOverrule.Instance.ProjectData = projectData;

                BlockData.BlockDataGripOverrule.Instance.EnableOverrule(true);
                _blkDataEnabled = true;
                ed.WriteMessage("\nAttributeMoveGripOeverrule is enabled.\n");
            }
            else
            {
                BlockData.BlockDataGripOverrule.Instance.EnableOverrule(false);
                _blkDataEnabled = false;
                ed.WriteMessage("\nAttributeMoveGripOeverrule is disabled.\n");
            }
        }

        private static List<(string project, string location, string client)> GetProjectData()
        {
            // project data usually is obtained from external data source,
            // such as database, data files... Here we use the hard-coded
            // for simplicity of the code
            var data = new List<(string, string, string)>();
            for (int i=1; i<=5; i++)
            {
                var p = $"2022-{i.ToString().PadLeft(3, '0')}";
                var l = $"Location {i.ToString().PadLeft(3, '0')}";
                var c = $"Client {i.ToString().PadLeft(3, '0')}";
                data.Add((p, l, c));
            }
            return data;
        }

        #endregion

        #region private methods

        private static ObjectId SelectEntity(Editor ed, string selectingMsg, Type[] entTypes=null)
        {
            var opt = new PromptEntityOptions($"\n{selectingMsg}:");
            if (entTypes!=null)
            {
                opt.SetRejectMessage("\nInvalid selection.");
                foreach (var t in entTypes)
                {
                    opt.AddAllowedClass(t, true);
                }
            }

            var res = ed.GetEntity(opt);
            return res.Status == PromptStatus.OK ? res.ObjectId : ObjectId.Null;
        }

        private static ObjectId SelectAttribute(Editor ed, string message)
        {
            while (true)
            {
                var opt = new PromptNestedEntityOptions($"\n{message}");
                opt.AllowNone = false;
                var res = ed.GetNestedEntity(opt);
                if (res.Status == PromptStatus.OK)
                {
                    if (res.ObjectId.ObjectClass.DxfName.ToUpper() == "ATTRIB")
                    {
                        return res.ObjectId;
                    }
                    else
                    {
                        ed.WriteMessage(
                            "\nInvalid selection: not an attribue in block!");
                    }
                }
                else
                {
                    break;
                }
            }
            return ObjectId.Null;
        }

        #endregion

        #region test commands
        [CommandMethod("ExtendLine")]
        public static void ExtendLine()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect a line:");
            if (res.Status != PromptStatus.OK) return;

            using (var tran = dwg.TransactionManager.StartTransaction())
            {
                var line = tran.GetObject(res.ObjectId, OpenMode.ForWrite) as Line;
                if (line!=null)
                {
                    var startVector = line.GetFirstDerivative(line.StartPoint).Negate();
                    var endVector = line.GetFirstDerivative(line.EndPoint);

                    var pt1 = line.StartPoint.TransformBy(Matrix3d.Displacement(startVector * 0.01));
                    var pt2 = line.EndPoint.TransformBy(Matrix3d.Displacement(endVector * 0.01));

                    line.StartPoint = pt1;
                    line.EndPoint = pt2;
                }

                tran.Commit();
            }
        }

        [CommandMethod("IncrementDrag")]
        public static void DragExtendsLine()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var res = ed.GetEntity("\nSelect a line:");
            if (res.Status != PromptStatus.OK) return;

            var jig = new IncrementDrag.LineIncrementJig(dwg, res.ObjectId, res.PickedPoint);
            jig.Drag(10);

        }

        #endregion
    }
}
