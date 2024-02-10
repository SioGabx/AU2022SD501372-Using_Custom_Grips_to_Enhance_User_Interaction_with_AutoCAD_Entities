using Autodesk.AutoCAD.DatabaseServices;
using CADApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripOverruleSample.AttEdit
{
    public class AttEditHelper
    {
        public static void UpdateAttribute(ObjectId attReferenceId, IEnumerable<string> attValues, string defaultAttValue)
        {
            var newValue = "";
            using (var view = new AttValueDropdownView(attValues, defaultAttValue))
            {
                var res = CADApp.ShowModalDialog(view);
                if (res== System.Windows.Forms.DialogResult.OK)
                {
                    newValue = view.AttributeValue;
                }
            }

            if (string.IsNullOrEmpty(newValue)) return;

            using (var tran = attReferenceId.Database.TransactionManager.StartTransaction())
            {
                var att = (AttributeReference)tran.GetObject(attReferenceId, OpenMode.ForWrite);
                att.TextString = newValue;
                tran.Commit();
            }
        }
    }
}
