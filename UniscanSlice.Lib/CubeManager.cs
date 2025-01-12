﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UniscanSlice.Lib
{
    public class CubeManager
    {
        private readonly string TextureSubDirectory = "texture";

        public Obj ObjInstance { get; set; }
        private Vector3 size;

        public CubeManager(SlicingOptions options)
        {
            size = options.CubeGrid;
            
            // Parse and load the object
            Trace.TraceInformation("Loading {0}", options.Obj);
            ObjInstance = new Obj();
            ObjInstance.LoadObj(options.Obj, ShowLinesLoaded, size, options);

            // Write out a bit of info about the object
            Trace.TraceInformation("Loaded {0} vertices and {1} faces", ObjInstance.VertexList.Count, ObjInstance.FaceList.Count);
            Trace.TraceInformation("Size: X {0} Y {1} Z {2}", ObjInstance.Size.XSize, ObjInstance.Size.YSize, ObjInstance.Size.ZSize);
            Trace.TraceInformation("Memory Used: " + GC.GetTotalMemory(true) / 1024 / 1024 + "mb");
        }

        public void GenerateCubes(string outputPath, SlicingOptions options)
        {
            CubeMetadata metadata = new CubeMetadata(size) {
                WorldBounds = ObjInstance.Size,
                VirtualWorldBounds = options.ForceCubicalCubes ? ObjInstance.CubicalSize : ObjInstance.Size,
                VertexCount = ObjInstance.VertexList.Count };

            // Configure texture slicing metadata
            if (options.RequiresTextureProcessing())
            {
                metadata.TextureSetSize = new Vector2(options.TextureSliceX, options.TextureSliceY);
            }
            else
            {
                metadata.TextureSetSize = new Vector2(1, 1);
            }

            // Create texture if there is one
            if (!string.IsNullOrEmpty(options.Texture))
            {
                Texture t = new Texture(this.ObjInstance, options.Texture);
                options.TextureInstance = t;
            }

            // Generate the data			
            if (options.Debug)
            {
                SpatialUtilities.EnumerateSpace(metadata.TextureSetSize, (x, y) =>
                {
                    var vertexCounts = GenerateCubesForTextureTile(outputPath, new Vector2(x, y), options);

                    foreach (var cube in vertexCounts.Keys)
                    {
                        metadata.CubeExists[cube.X, cube.Y, cube.Z] = vertexCounts[cube] > 0;
                    }
                });
            }
            else
            {
                SpatialUtilities.EnumerateSpaceParallel(metadata.TextureSetSize, (x, y) =>
                {
                    var vertexCounts = GenerateCubesForTextureTile(outputPath, new Vector2(x, y), options);

                    foreach (var cube in vertexCounts.Keys)
                    {
                        metadata.CubeExists[cube.X, cube.Y, cube.Z] = vertexCounts[cube] > 0;
                    }
                });
            }

            // Write out some json metadata
            string metadataPath = Path.Combine(outputPath, "metadata.json");
            if (File.Exists(metadataPath)) File.Delete(metadataPath);

            string metadataString = JsonConvert.SerializeObject(metadata);
            File.WriteAllText(metadataPath, metadataString);
        }

        public Dictionary<Vector3, int> GenerateCubesForTextureTile(string outputPath, Vector2 textureTile, SlicingOptions options)
        {          
            // If appropriate, generate textures and save transforms first
            if (options.Texture != null)
            {
                ProcessTextureTile(Path.Combine(outputPath, TextureSubDirectory), textureTile, options);
            }

            Dictionary<Vector3, int> vertexCounts = new Dictionary<Vector3, int>();
           
            // Generate some cubes		                
            var cubes = Texture.GetCubeListFromTextureTile(
                options.TextureSliceY, options.TextureSliceX, textureTile.X, textureTile.Y, ObjInstance).ToArray();
            
            foreach(var cube in cubes)
            {
                Trace.TraceInformation("Processing cube {0}", cube);
                string fileOutPath = Path.Combine(outputPath, $"{cube.X}_{cube.Y}_{cube.Z}");
                int vertexCount = ObjInstance.WriteSpecificCube(fileOutPath, cube, options);
                vertexCounts.Add(cube, vertexCount);
            }

            return vertexCounts;
        }

        public void ProcessTextureTile(string outputPath, Vector2 textureTile, SlicingOptions options)
        {
            Trace.TraceInformation("Processing texture tile {0}", textureTile);

            string fileOutPath = Path.Combine(outputPath, $"{textureTile.X}_{textureTile.Y}.jpg");

            // Generate new texture
            var transform = options.TextureInstance.GenerateTextureTile(fileOutPath, textureTile, options);

            // Transform associated UV's
            ObjInstance.TransformUVsForTextureTile(options, textureTile, transform);      
        }

        // Action to show incremental file loading status
        public static void ShowLinesLoaded(int lines)
        {			
            //Console.SetCursorPosition(0, Console.CursorTop);
            //Console.Write("Loaded {0} lines             ", lines);			
        }
    }
}
