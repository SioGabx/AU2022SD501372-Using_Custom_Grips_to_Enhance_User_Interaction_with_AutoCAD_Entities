//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.GraphicsInterface;
//using Autodesk.AutoCAD.Runtime;
//using CadApp = Autodesk.AutoCAD.ApplicationServices.Application;

//namespace GripOverruleSample.IncrementDrag
//{
//    public class LineExtendJig : DrawJig
//    {
//        private Document _dwg;
//        private Database _db;
//        private Editor _ed;
//        private ObjectId _lineId;
//        private Line _line = null;
//        private double _increment = 0.0;
//        private Point3d _basePoint;

//        //private Line _ghostLine = null;
//        private List<DBPoint> _points = new List<DBPoint>();
//        private TransientManager _tsManager = TransientManager.CurrentTransientManager;

//        public void Drag(Document dwg, ObjectId lineId)
//        {
//            InitializeJig(dwg, lineId);
//            using (var tran = _db.TransactionManager.StartTransaction())
//            {
//                _line = (Line)tran.GetObject(_lineId, OpenMode.ForWrite);
                

//                var res = _ed.Drag(this);
//                if (res.Status== PromptStatus.OK)
//                {
//                    tran.Commit();
//                }
//            }
//        }

//        protected override SamplerStatus Sampler(JigPrompts prompts)
//        {


//            return SamplerStatus.Cancel;
//        }

//        protected override bool WorldDraw(WorldDraw draw)
//        {

//            return true;
//        }

//        #region private methods

//        private void InitializeJig(Document dwg, ObjectId lineId)
//        {
//            if (lineId.ObjectClass.DxfName.ToUpper() != "LINE")
//            {
//                throw new InvalidOperationException("Object must be a LINE.");
//            }
//            _db = _lineId.Database;
//            _lineId = lineId;
            
//            using (var tran = _db.TransactionManager.StartOpenCloseTransaction())
//            {
//                var line = (Line)tran.GetObject(_lineId, OpenMode.ForRead);
//                _increment = line.Length / 100.0;
//                tran.Commit();
//            }
//        }

//        private void ClearGhosts()
//        {

//        }

//        private void CreateGhosts()
//        {

//        }

//        #endregion
//    }
//}
