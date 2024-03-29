﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace GripOverruleSample.Polygon
{
    public class PolygonGripOverrule : GripOverrule
    {
        private bool _enabled = false;
        private bool _originalOverruling = false;
        private static PolygonGripOverrule _instance = null;

        public static PolygonGripOverrule Instance
        {
            get
            {
                if (_instance == null )
                {
                    _instance = new PolygonGripOverrule();
                }
                return _instance;
            }
        }

        public bool HideOriginals { get; set; } = true;

        public void EnableOverrule(bool enable)
        {
            if (enable)
            {
                if (_enabled) return;
                _originalOverruling = Overrule.Overruling;
                AddOverrule(RXClass.GetClass(typeof(Polyline)), this, false);
                //SetIdFilter([IdArray]);
                //SetXDataFilter("MyXDataAppName");
                SetCustomFilter();

                Overrule.Overruling = true;
                _enabled = true;
            }
            else
            {
                if (!_enabled) return;
                RemoveOverrule(RXClass.GetClass(typeof(Polyline)), this);
                Overrule.Overruling = _originalOverruling;
                _enabled = false;
            }
        }

        public override bool IsApplicable(RXObject overruledSubject)
        {
            var poly = overruledSubject as Polyline;
            if (poly!=null)
            {
                return poly.Closed;
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
            var poly = entity as Polyline;
            if (poly != null && poly.Closed)
            {
                    var pt = GetPolygonExtentsCenter(poly);
                    var grip = new PolygonGrip() 
                    { 
                        GripPoint = pt, 
                        EntityId = entity.ObjectId 
                    };
                    grips.Add(grip);


                if (!HideOriginals)
                {
                    base.GetGripPoints(
                        entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
                }
                return;
            }

            base.GetGripPoints(
                entity, grips, curViewUnitSize, gripSize, curViewDir, bitFlags);
        }

        private Point3d GetPolygonExtentsCenter(Polyline poly)
        {
            var ext = poly.GeometricExtents;
            var x = ext.MinPoint.X + (ext.MaxPoint.X - ext.MinPoint.X) / 2.0;
            var y = ext.MinPoint.Y + (ext.MaxPoint.Y - ext.MinPoint.Y) / 2.0;
            return new Point3d(x, y, ext.MaxPoint.Z);
        }
    }
}
