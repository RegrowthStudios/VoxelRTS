using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using BColor = System.Drawing.Color;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Interfaces;
using RTSEngine.Graphics;
using System.Collections.Concurrent;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Parsers {
    public class HeightmapResult {
        public Heightmap Data;
        public HeightmapModel View;
    }

    public static class HeightmapParser {
        // Data Detection
        const string EXTENSION = "map";
        const ParsingFlags READ_FLAGS = ParsingFlags.ConversionOpenGL;
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile("DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile("HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile("HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile("HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile("HTS");
        static readonly Regex rgxSize = RegexHelper.GenerateVec3("SIZE");

        private static void ConvertPixel(byte[] cols, int ci, float[] h, byte[] d, int i) {
            h[i] = 1f - (cols[ci + 2] / 255f);
            d[i] = cols[ci + 1] > 128 ? (byte)0x01u : (byte)0x00u;
        }

        private static HeightmapResult ParseFromInfo(GameEngine ge, Stream s, string rootDir) {
            HeightmapResult res = new HeightmapResult();
            res.View = new HeightmapModel();
            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();

            // Read All Data First
            Vector3 size = RegexHelper.ExtractVec3(rgxSize.Match(ms));
            FileInfo hfi = RegexHelper.ExtractFile(rgxDataFile.Match(ms), rootDir);
            FileInfo mpfi = RegexHelper.ExtractFile(rgxHModelPFile.Match(ms), rootDir);
            FileInfo tpfi = RegexHelper.ExtractFile(rgxHTexPFile.Match(ms), rootDir);

            FileInfo msfi = null, tsfi = null;
            Match s1 = rgxHModelSFile.Match(ms);
            Match s2 = rgxHTexSFile.Match(ms);
            if(s1.Success && s2.Success) {
                msfi = RegexHelper.ExtractFile(s1, rootDir);
                tsfi = RegexHelper.ExtractFile(s2, rootDir);
            }

            // Read Height Data
            using(var bmp = Bitmap.FromFile(hfi.FullName) as Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] cd = new byte[w * h];
                byte[] col = new byte[w * h * 4];
                int i = 0, ci = 0;

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, col, 0, data.Stride * data.Height);
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        ConvertPixel(col, ci, hd, cd, i++);
                        ci += 4;
                    }
                }
                bmp.UnlockBits(data);
                res.Data = new Heightmap(hd, cd, w, h);
            }
                // Apply Heightmap Size
                res.Data.Width = size.X;
                res.Data.Depth = size.Z;
                res.Data.ScaleHeights(size.Y);
            

            // Must Read Primary Model
            BoundingBox aabb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            Vector3 scaling = size;
            VertexPositionNormalTexture[] vertsPNT;
            int[] inds;

            // Read The Map Model
            using(var sPrimary = File.OpenRead(mpfi.FullName)) {
                ObjParser.TryParse(sPrimary, out vertsPNT, out inds, READ_FLAGS);
            }
            VertexPositionTexture[] verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > aabb.Max.X) aabb.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < aabb.Min.X) aabb.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > aabb.Max.Y) aabb.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < aabb.Min.Y) aabb.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > aabb.Max.Z) aabb.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < aabb.Min.Z) aabb.Min.Z = verts[i].Position.Z;
            }
            // Find Scaling
            scaling /= aabb.Max - aabb.Min;

            // Reposition Model
            for(int i = 0; i < verts.Length; i++) {
                // Move Model Minimum To Origin
                verts[i].Position -= aabb.Min;
                // Scale Heights To [0,1]
                verts[i].Position *= scaling;
            }

            // Create Primary Geometry
            ModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out res.View.VBPrimary, out res.View.IBPrimary, BufferUsage.WriteOnly);
            res.Data.BVH.Build(verts, inds);

            if(msfi != null && msfi.Exists) {
                // Read The Detail Model
                using(var sSecondary = File.OpenRead(msfi.FullName)) {
                    ObjParser.TryParse(sSecondary, out vertsPNT, out inds, READ_FLAGS);
                }
                for(int i = 0; i < vertsPNT.Length; i++) {
                    verts[i].Position = vertsPNT[i].Position;
                    verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;
                }


                // Reposition Detail Model To Match The Primary Model
                for(int i = 0; i < verts.Length; i++) {
                    // Move Model Minimum To Origin
                    verts[i].Position -= aabb.Min;
                    // Scale Heights To [0,1]
                    verts[i].Position *= scaling;
                }

                // Create Graphics Geometry
                ModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out res.View.VBSecondary, out res.View.IBSecondary, BufferUsage.WriteOnly);
            }

            // Read Primary Texture
            res.View.PrimaryTexture = ge.LoadTexture2D(tpfi.FullName);
            // Try To Read Secondary Texture
            if(tsfi != null)
                res.View.SecondaryTexture = ge.LoadTexture2D(tsfi.FullName);
            return res;
        }
        private static HeightmapModel CreateHeightmapModel(GameEngine ge, Vector3 size, out VertexPositionTexture[] verts, out int[] inds, FileInfo mpfi, FileInfo msfi = null) {
            HeightmapModel view = new HeightmapModel();
            
            // Parsing Information
            BoundingBox aabb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            Vector3 scaling = size;
            VertexPositionNormalTexture[] vertsPNT;

            // Read The Map Model
            using(var sPrimary = File.OpenRead(mpfi.FullName)) {
                ObjParser.TryParse(sPrimary, out vertsPNT, out inds, READ_FLAGS);
            }
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > aabb.Max.X) aabb.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < aabb.Min.X) aabb.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > aabb.Max.Y) aabb.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < aabb.Min.Y) aabb.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > aabb.Max.Z) aabb.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < aabb.Min.Z) aabb.Min.Z = verts[i].Position.Z;
            }
            // Find Scaling
            scaling /= aabb.Max - aabb.Min;

            // Reposition Model
            for(int i = 0; i < verts.Length; i++) {
                // Move Model Minimum To Origin
                verts[i].Position -= aabb.Min;
                // Scale Heights To [0,1]
                verts[i].Position *= scaling;
            }

            // Create Primary Geometry
            ModelHelper.CreateBuffers(ge.G, verts, VertexPositionTexture.VertexDeclaration, inds, out view.VBPrimary, out view.IBPrimary, BufferUsage.WriteOnly);

            if(msfi != null && msfi.Exists) {
                // Read The Detail Model
                int[] sInds;
                using(var sSecondary = File.OpenRead(msfi.FullName)) {
                    ObjParser.TryParse(sSecondary, out vertsPNT, out sInds, READ_FLAGS);
                }
                VertexPositionTexture[] sVerts = new VertexPositionTexture[vertsPNT.Length];
                for(int i = 0; i < vertsPNT.Length; i++) {
                    sVerts[i].Position = vertsPNT[i].Position;
                    sVerts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;
                }


                // Reposition Detail Model To Match The Primary Model
                for(int i = 0; i < sVerts.Length; i++) {
                    // Move Model Minimum To Origin
                    sVerts[i].Position -= aabb.Min;
                    // Scale Heights To [0,1]
                    sVerts[i].Position *= scaling;
                }

                // Create Graphics Geometry
                ModelHelper.CreateBuffers(ge.G, sVerts, VertexPositionTexture.VertexDeclaration, sInds, out view.VBSecondary, out view.IBSecondary, BufferUsage.WriteOnly);
            }
            return view;
        }
        public static HeightmapResult Parse(GameEngine ge, DirectoryInfo dir) {
            // Find The Information File
            var files = dir.GetFiles();
            FileInfo infoFile = files.FirstOrDefault((f) => {
                return f.Extension.ToLower().EndsWith(EXTENSION);
            });
            if(infoFile == null)
                throw new ArgumentException("Map Information File Could Not Be Found In The Directory");

            // Parse Data
            HeightmapResult res;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                res = ParseFromInfo(ge, s, dir.FullName);
            }
            return res;
        }
    }
}