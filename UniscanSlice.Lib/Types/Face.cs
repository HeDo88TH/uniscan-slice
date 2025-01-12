﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UniscanSlice.Lib.Types
{
    public class Face : IType
    {
        public const int MinimumDataLength = 4;
        public const string Prefix = "f";

        public int[] VertexIndexList { get; set; }
        public int[] TextureVertexIndexList { get; set; }
		public HashSet<int> TextureVertexIndexHash { get; set; }

		private int[] originalVertexIndexList;
        private int[] originalTextureVertexIndexList;

        public void LoadFromStringArray(string[] data)
        {
            if (data.Length < MinimumDataLength)
                throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, nameof(data));

            if (!data[0].ToLower().Equals(Prefix))
                throw new ArgumentException("Data prefix must be '" + Prefix + "'", nameof(data));            

            int vcount = data.Length - 1;
            VertexIndexList = new int[vcount];
            TextureVertexIndexList = new int[vcount];
			originalVertexIndexList = new int[vcount];
			originalTextureVertexIndexList = new int[vcount];

            for (int i = 0; i < vcount; i++)
            {
                string[] parts = data[i + 1].Split('/');

                var success = int.TryParse(parts[0], out var vindex);
                if (!success) throw new ArgumentException("Could not parse parameter as int");
                VertexIndexList[i] = vindex;

                if (parts.Length > 1)
                {
                    success = int.TryParse(parts[1], out vindex);
                    if (!success) throw new ArgumentException("Could not parse parameter as int");
                    TextureVertexIndexList[i] = vindex;
                }
            }

			VertexIndexList.CopyTo(originalVertexIndexList,0);
			TextureVertexIndexList.CopyTo(originalTextureVertexIndexList, 0);

			TextureVertexIndexHash = new HashSet<int>(TextureVertexIndexList);

        }

        public bool InExtent(Extent extent, List<Vertex> vertexList)
        {
            //Vertex relevantVertex = VertexIndexList.Select(vi => vertexList[vi - 1]).OrderBy(v => v.X).ThenBy(v => v.Y).First();

            if (vertexList[VertexIndexList[0] - 1].InExtent(extent)) return true;

            return false;
        }

        public void UpdateVertexIndex(int oldIndex, int newIndex, bool retain = true)
        {           
            for(int index = 0; index < VertexIndexList.Count(); index++)
            {
                if (originalVertexIndexList[index] == oldIndex)
                {
                    VertexIndexList[index] = newIndex;

                    if (!retain)
                    {
                        originalVertexIndexList[index] = newIndex;
                    }
                    return;
                }
            }
        }

        public void UpdateTextureVertexIndex(int oldIndex, int newIndex, bool retain = true)
        {
            for (int index = 0; index < TextureVertexIndexList.Length; index++)
            {
                if (originalTextureVertexIndexList[index] == oldIndex)
                {
                    TextureVertexIndexList[index] = newIndex;

					if (!retain)
					{
						originalTextureVertexIndexList[index] = newIndex;
					}
                    return;
                }
            }
        }

		public void RevertVertices()
		{
			originalVertexIndexList.CopyTo(VertexIndexList, 0);
			originalTextureVertexIndexList.CopyTo(TextureVertexIndexList, 0);
		}

        // HACKHACK this will write invalid files if there are no texture vertices in
        // the faces, since we don't read that in properly yet
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append('f');

            for(int i = 0; i < VertexIndexList.Length; i++)
            {
                b.Append($" {VertexIndexList[i]}/{TextureVertexIndexList[i]}");                
            }

            return b.ToString();
        }

        public Face Clone()
        {
            var Face = new Face()
            {
                originalTextureVertexIndexList = (int[])this.originalTextureVertexIndexList.Clone(),
                originalVertexIndexList = (int[])this.originalVertexIndexList.Clone(),
                TextureVertexIndexList = (int[])this.TextureVertexIndexList.Clone(),
                VertexIndexList = (int[])this.VertexIndexList.Clone()                
            };

            Face.TextureVertexIndexHash = new HashSet<int>(Face.TextureVertexIndexList);

            return Face;
        }
	}
}
