﻿using System;
using System.Diagnostics;
using System.IO;
using UniscanSlice.Lib;
using clipr;
using clipr.Core;

namespace UniscanSlice.Cli
{
    class Program
    {
        static void Main(string[] args)
        {          
            Options opt;

            // Setup a timer for all operations
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("MESH-TILER: STARTING TILING PROCESS");

            try
            {
                opt = CliParser.Parse<Options>(args);

                foreach (string path in opt.Input)
                {
                    // Check if we are processing an image or a mesh
                    if (Path.GetExtension(path).ToUpper().EndsWith("JPG"))
                    {
                        Console.WriteLine(" -> Generating image tiles");
                        ImageTile tiler = new ImageTile(path, opt.XSize, opt.YSize);
                        tiler.GenerateTiles(opt.OutputPath);
                    }
                    else if (Path.GetExtension(path).ToUpper().EndsWith("OBJ"))
                    {
                        Console.WriteLine(" -> Slicing OBJ");

                        // Generate subfolders named after input file
                        // if multiple input files are provided
                        var outputPath = opt.Input.Count == 1 ? opt.OutputPath : Path.Combine(opt.OutputPath, Path.GetFileNameWithoutExtension(path));

                        if (opt.ForceCubical)
                        {
                            int longestGridSide = Math.Max(Math.Max(opt.XSize, opt.YSize), opt.ZSize);
                            opt.XSize = opt.YSize = opt.ZSize = longestGridSide;

                            Console.WriteLine("Due to -ForceCubical grid size is now {0},{0},{0}", longestGridSide);
                        }

                        var options = new SlicingOptions
                        {
                            OverrideMtl = opt.MtlOverride,
                            GenerateEbo = opt.Ebo,
                            GenerateOpenCtm =  opt.OpenCtm,
                            Debug = opt.Debug,
                            GenerateObj = opt.Obj,
                            Texture = opt.Texture,
                            Obj = path,
                            WriteMtl = opt.WriteMtl,
                            TextureScale = opt.ScaleTexture,
                            TextureSliceX = opt.TextureXSize,
                            TextureSliceY = opt.TextureYSize,
                            ForceCubicalCubes = opt.ForceCubical,
                            CubeGrid = new Vector3 { X = opt.XSize, Y = opt.YSize, Z = opt.ZSize }
                        };

                        CubeManager manager = new CubeManager(options);

                        if (opt.MarkupUV)
                        {
                            Texture tex = new Texture(manager.ObjInstance);
                            tex.MarkupTextureFaces(opt.Texture);
                        }
                        else
                        {
                            manager.GenerateCubes(outputPath, options);
                        }
                    }
                    else
                    {
                        Console.WriteLine("uniscan-slice only accepts .jpg and .obj files for input.");
                    }
                }
            }
            catch (ParserExit)
            {
                return;
            }
            catch (ParseException ex)
            {
                Console.WriteLine("usage: uniscan-slice --help\n" + ex.ToString());
            }

            stopwatch.Stop();
            Console.WriteLine(" ?> Elapsed " + stopwatch.Elapsed);
            Trace.TraceInformation(stopwatch.Elapsed.ToString());
        }
    }
}
