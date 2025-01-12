﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniscanSlice.Lib
{
	public class Vector2
	{
        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

		public int X { get; set; }
		public int Y { get; set; }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
	}
}
