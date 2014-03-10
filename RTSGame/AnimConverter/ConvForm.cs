using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

namespace AnimConverter {
    public partial class ConvForm : Form {
        public ConvForm() {
            InitializeComponent();
        }

        private void ConvForm_Load(object sender, EventArgs e) {
            rtbInfo.AppendText("Please Enter Directory To OBJ Animation");
        }

        static Color Convert(float f) {
            return Color.FromArgb(BitConverter.ToInt32(BitConverter.GetBytes(f), 0));
        }
        static float Convert(Color c) {
            return BitConverter.ToSingle(new byte[] { c.B, c.G, c.R, c.A }, 0);
        }

        static void SuperBake(FileInfo[] files, string pathOut, RichTextBox log) {
            VertexPositionNormalTexture[] verts;
            int[] inds;

            // Find The Width Of The Bitmap First
            int bmpWidth = 0;
            using(FileStream fs = File.OpenRead(files[0].FullName)) {
                ObjParser.tryParse(fs, out verts, out inds, ParsingFlags.ConversionOpenGL);
                bmpWidth = verts.Length;
            }
            Bitmap bmp = new Bitmap(bmpWidth, files.Length * 3);
            log.AppendText("Creating New Image " + bmp.Width + " x " + bmp.Height + "\n");

            // Bake
            int py = 0;
            float percent = 0f, dp = 100f / files.Length;
            log.AppendText("Percent Complete: " + percent + "%\n");
            foreach(var fi in files) {
                using(FileStream fs = File.OpenRead(fi.FullName)) {
                    ObjParser.tryParse(fs, out verts, out inds, ParsingFlags.ConversionOpenGL);
                }
                for(int px = 0; px < bmp.Width || px < verts.Length; px++) {
                    bmp.SetPixel(px, py, Convert(verts[px].Position.X));
                    bmp.SetPixel(px, py + 1, Convert(verts[px].Position.Y));
                    bmp.SetPixel(px, py + 2, Convert(verts[px].Position.Z));
                }
                py += 3;
                percent += dp;
                log.AppendText("Percent Complete: " + percent + "%\n");
            }

            // Save The Image
            bmp.Save(pathOut, System.Drawing.Imaging.ImageFormat.Png);
            log.AppendText("File Saved To - \n" + pathOut);
            bmp.Dispose();
        }
        static void TestBake(FileInfo[] files, string pathBMP, RichTextBox log) {
            Bitmap bmp = Bitmap.FromFile(pathBMP) as Bitmap;

            VertexPositionNormalTexture[] verts;
            int[] inds;
            int py = 0;
            float percent = 0f, dp = 100f / files.Length;
            float totalDistSq = 0f, maxDistSq = 0f;
            log.AppendText("Percent Complete: " + percent + "%\n");
            foreach(var fi in files) {
                using(FileStream fs = File.OpenRead(fi.FullName)) {
                    ObjParser.tryParse(fs, out verts, out inds, ParsingFlags.ConversionOpenGL);
                }
                for(int px = 0; px < bmp.Width || px < verts.Length; px++) {
                    Vector3 point = new Vector3(
                        Convert(bmp.GetPixel(px, py)),
                        Convert(bmp.GetPixel(px, py + 1)),
                        Convert(bmp.GetPixel(px, py + 2))
                        );
                    float d = (point - verts[px].Position).LengthSquared();
                    totalDistSq += d;
                    if(d > maxDistSq) maxDistSq = d;
                }
                py += 3;
                percent += dp;
                log.AppendText("Percent Complete: " + percent + "%\n");
            }
            log.AppendText("Total Error (Sum Squares): " + totalDistSq + "\nMax Error: " + maxDistSq + "\n");

            bmp.Dispose();
        }

        private void btnBuild_Click(object sender, EventArgs e) {
            rtbInfo.Clear();

            // Check Input
            if(string.IsNullOrWhiteSpace(tbDir.Text)) {
                rtbInfo.AppendText("ERROR: A Directory Must Be Entered");
                return;
            }
            DirectoryInfo di = new DirectoryInfo(tbDir.Text);
            if(!di.Exists) {
                rtbInfo.AppendText("ERROR: Directory Does Not Exist - \n" + di.FullName);
                return;
            }
            if(string.IsNullOrWhiteSpace(tbPrefix.Text)) {
                rtbInfo.AppendText("ERROR: A Prefix Must Be Entered");
                return;
            }
            FileInfo[] files = di.EnumerateFiles().Where((f) => {
                return f.Name.StartsWith(tbPrefix.Text) && f.Extension.ToLower().Equals(".obj");
            }).ToArray();
            if(files.Length < 1) {
                rtbInfo.AppendText("ERROR: Could Not Find Any Suitable Files Beginning With " + tbPrefix.Text);
                return;
            }

            // Sort The Files By Taking Off The Prefix
            FileInfo[] fSorted = files.OrderBy((f) => {
                return f.Name.Substring(tbPrefix.Text.Length);
            }).ToArray();
            rtbInfo.AppendText("Found Files:\n");
            foreach(var fi in fSorted) {
                rtbInfo.AppendText(fi.Name + "\n");
            }

            // Set Output Image Path
            string fn = di.FullName + "\\" + tbPrefix.Text + ".png";

            // Perform Baking
            rtbInfo.AppendText("\n\nSuper Baking...\n");
            SuperBake(fSorted, fn, rtbInfo);

            // Test The Procedure
            rtbInfo.AppendText("\n\nTesting Baking Procedure...\n");
            TestBake(fSorted, fn, rtbInfo);
        }
    }
}