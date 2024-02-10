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

namespace GripOverruleSample.IncrementDrag
{
    public class LineIncrementJig
    {
        private readonly Document _dwg;
        private readonly Database _db;
        private readonly Editor _ed;
        private readonly ObjectId _lineId;

        private Point3d _basePoint;
        private Point3d _secondPoint;
        private Point3d _finalPoint;
        private bool _startAsBasePoint;
        private double _increment = 0.0;

        private Line _line = null;
        private List<DBPoint> _points = null;
        private TransientManager _tsManager = TransientManager.CurrentTransientManager;

        public LineIncrementJig(Document dwg, ObjectId lineId, Point3d initPoint)
        {
            _dwg = dwg;
            _db = _dwg.Database;
            _ed= _dwg.Editor;
            _lineId= lineId;

            using (var tran = _db.TransactionManager.StartOpenCloseTransaction())
            {
                var line = (Line)tran.GetObject(_lineId, OpenMode.ForRead);
                var d1 = initPoint.DistanceTo(line.StartPoint);
                var d2 = initPoint.DistanceTo(line.EndPoint);
                if (d2 < d1)
                {
                    _basePoint = line.StartPoint;
                    _secondPoint = line.EndPoint;
                    _startAsBasePoint = true;
                }
                else
                {
                    _basePoint = line.EndPoint;
                    _secondPoint = line.StartPoint;
                    _startAsBasePoint = false;
                }
                tran.Commit();
            }
        }

        public void Drag(int dividedCount)
        {
            using (var tran = _db.TransactionManager.StartOpenCloseTransaction())
            {
                var line = (Line)tran.GetObject(_lineId, OpenMode.ForRead);
                _increment = line.Length / dividedCount;
                tran.Commit();
            }
            Drag();
        }

        public void Drag(double increment)
        {
            _increment = increment;
            Drag();
        }

        #region private methods

        private void Drag()
        {
            var pMode = (short)CadApp.GetSystemVariable("PDMODE");
            try
            {
                CadApp.SetSystemVariable("PDMODE", 2);
                _ed.PointMonitor += Editor_PointMonitor;

                var opt = new PromptPointOptions("\nSelect line's extended end point:");
                opt.AllowNone = false;
                opt.UseBasePoint = true;
                opt.BasePoint = _basePoint;
                opt.UseDashedLine = false;

                var res = _ed.GetPoint(opt);
                if (res.Status== PromptStatus.OK)
                {
                    if (_finalPoint!=_basePoint && _finalPoint!=_secondPoint)
                    {
                        using (var tran = _db.TransactionManager.StartTransaction())
                        {
                            var line = (Line)tran.GetObject(_lineId, OpenMode.ForWrite);
                            if (_startAsBasePoint)
                            {
                                line.EndPoint = _finalPoint;
                            }
                            else
                            {
                                line.StartPoint = _finalPoint;
                            }
                            tran.Commit();
                        }
                    }
                }
            }
            finally
            {
                ClearGhosts();
                _ed.PointMonitor -= Editor_PointMonitor;
                CadApp.SetSystemVariable("PDMODE", pMode);
            }
        }

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            ClearGhosts();
            var pt = e.Context.RawPoint;
            DrawGhosts(pt);
            if (_points.Count>0)
            {
                _finalPoint = _points[_points.Count-1].Position;
            }
            else
            {
                _finalPoint = _basePoint;
            }
        }

        private void ClearGhosts()
        {
            if (_points!=null && _points.Count>0)
            {
                foreach (var p in _points)
                {
                    _tsManager.EraseTransient(p, new IntegerCollection());
                    p.Dispose();
                }
                _points.Clear();
            }
            if (_line!=null)
            {
                _tsManager.EraseTransient(_line, new IntegerCollection());
                _line.Dispose();
                _line = null;
            }
        }

        private void DrawGhosts(Point3d mousePoint)
        {
            _points = CreateGhostPoints(mousePoint);
            foreach (var pt in _points)
            {
                _tsManager.AddTransient(pt, TransientDrawingMode.DirectShortTerm, 128, new IntegerCollection());
            }

            try
            {
                _line = new Line(_basePoint, _points[_points.Count - 1].Position);
                _tsManager.AddTransient(_line, TransientDrawingMode.Highlight, 126, new IntegerCollection());
            }
            catch { }
        }

        private List<DBPoint> CreateGhostPoints(Point3d mousePoint)
        {
            var dbPoints = new List<DBPoint>();

            using (var ray = new Ray())
            {
                ray.BasePoint = _basePoint;
                ray.SecondPoint = _secondPoint;

                try
                {
                    var pt = ray.GetClosestPointTo(mousePoint, false);
                    var len = ray.GetDistAtPoint(pt);

                    var curL = _increment;
                    while (curL < len)
                    {
                        var p = ray.GetPointAtDist(curL);
                        var dbP = new DBPoint(p);
                        dbP.ColorIndex = 2;
                        dbPoints.Add(dbP);

                        curL += _increment;
                    }
                }
                catch
                {
                }
            }

            return dbPoints;
        }

        #endregion
    }
}
