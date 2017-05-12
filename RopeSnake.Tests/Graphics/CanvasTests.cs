using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RopeSnake.Graphics;
using RopeSnake.Core;

namespace RopeSnake.Tests.Graphics
{
    [TestClass]
    public class CanvasTests
    {
        private Bitmap bitmap;

        [TestInitialize]
        public void TestInitialize()
        {
            bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        }

        [TestMethod]
        public void SetPixel()
        {
            using (var canvas = new Canvas(bitmap))
            {
                canvas.SetPixel(0, 0, Color.Blue);
                canvas.SetPixel(31, 0, Color.Red);
                canvas.SetPixel(16, 16, Color.Yellow);
            }

            Assert.AreEqual(Color.Blue.ToArgb(), bitmap.GetPixel(0, 0).ToArgb());
            Assert.AreEqual(Color.Red.ToArgb(), bitmap.GetPixel(31, 0).ToArgb());
            Assert.AreEqual(Color.Yellow.ToArgb(), bitmap.GetPixel(16, 16).ToArgb());
        }

        [TestMethod]
        public void GetPixel()
        {
            bitmap.SetPixel(0, 0, Color.Blue);
            bitmap.SetPixel(31, 0, Color.Red);
            bitmap.SetPixel(16, 16, Color.Yellow);

            using (var canvas = new Canvas(bitmap))
            {
                Assert.AreEqual(Color.Blue.ToArgb(), canvas.GetPixel(0, 0).ToArgb());
                Assert.AreEqual(Color.Red.ToArgb(), canvas.GetPixel(31, 0).ToArgb());
                Assert.AreEqual(Color.Yellow.ToArgb(), canvas.GetPixel(16, 16).ToArgb());
            }
        }

        [TestMethod]
        public void GetPixelOutOfBounds()
        {
            using (var canvas = new Canvas(bitmap))
            {
                Assert.ThrowsException<InvalidOperationException>(
                    () => canvas.GetPixel(-1, -2), "Coordinate (-1, -2) out of bounds");

                Assert.ThrowsException<InvalidOperationException>(
                    () => canvas.GetPixel(32, 32), "Coordinate (32, 32) out of bounds");
            }
        }
    }
}
