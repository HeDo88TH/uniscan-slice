using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniscanSlice.Lib
{
    public class Extent
    {
        public double XMax { get; set; }
        public double XMin { get; set; }
        public double YMax { get; set; }
        public double YMin { get; set; }
        public double ZMax { get; set; }
        public double ZMin { get; set; }

        public double XSize => XMax - XMin;
        public double YSize => YMax - YMin;
        public double ZSize => ZMax - ZMin;

        public Vector3D MaxCorner => new(XMax, YMax, ZMax);
        public Vector3D MinCorner => new(XMin, YMin, ZMin);
    }
}
