using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageToConsoleImage
{
    public partial class MainForm : Form
    {
        Bitmap sourceImage;
        Bitmap stretchedImage;
        List<string> bitmap = new List<string>();
        List<Color> defaultColors;
        string currentResult = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog
            {
                Title = "Open An Image File",
                Filter = "Image Files|*.png;*.jpg",
                InitialDirectory = @"Desktop"
            };
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = theDialog.FileName;
                    //MessageBox.Show(fileName);
                    sourceImage = new Bitmap(fileName);
                    stretchedImage = ResizeImage(sourceImage, new Size(320, 320));
                    panelImage.BackgroundImage = stretchedImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            currentResult = ProcessImage(sourceImage);
            textBoxResult.Text = currentResult.Replace("\n", "\r\n");

        }

        public string ProcessImage(Bitmap img)
        {
            string result = "";
            result += GetDefultColor(img);
            result += ProcessPixels(img);
            return result;
        }

        public string GetDefultColor(Bitmap img)
        {
            string result = "";
            defaultColors = new List<Color>();
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Color currentP = img.GetPixel(x, y);
                    if (currentP.A == 0)
                    {
                        currentP = Color.Black;
                    }

                    if (!defaultColors.Contains(currentP))
                    {
                        if (currentP.ToArgb() != Color.Black.ToArgb())
                        {
                            if (currentP.ToArgb() != Color.White.ToArgb())
                            {
                                defaultColors.Add(currentP);
                            }
                        }
                    }
                }
            }
            foreach (Color c in defaultColors)
            {
                result += "#" + ((int)c.R).ToString("X2") + ((int)c.G).ToString("X2") + ((int)c.B).ToString("X2") + "\n";
            }
            result += "\n";
            return result;
        }

        public string ProcessPixels(Bitmap img)
        {
            string result = "";
            char pixelInChar;
            bitmap.Clear();
            for (int y = 0; y < img.Height; y++)
            {
                bitmap.Add("");
                for (int x = 0; x < img.Width; x++)
                {
                    pixelInChar = ProcessPixel(img.GetPixel(x, y), defaultColors);
                    result += pixelInChar;
                    bitmap[y] += pixelInChar;
                }
                result += "\n";
            }
            return result;
        }

        public char ProcessPixel(Color color, List<Color> defCs)
        {
            if (color.A == 0)
            {
                return 'T';
            }
            if (color.ToArgb() == Color.Black.ToArgb())
            {
                return 'Z';
            }
            if (color.ToArgb() == Color.White.ToArgb())
            {
                return 'W';
            }

            for (int i = 0; i < defCs.Count; i++)
            {
                if (color.ToArgb() == defCs[i].ToArgb())
                {
                    if (i < 10)
                    {
                        return (char)('0' + i);
                    }
                    if (i < 14)
                    {
                        return (char)('A' + i - 10);
                    }
                }
            }
            return 'T';
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            StreamWriter myStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Console Image files (*.ci)|*.ci|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                myStream = new StreamWriter(saveFileDialog.FileName);

                myStream.Write(currentResult);
                myStream.Close();
            }
        }

        // Follow codes are modified from 
        // https://stackoverflow.com/questions/87753/resizing-an-image-without-losing-any-quality
        private static Bitmap ResizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            // this mode allows the low defination image to be strached out without looking blurry.
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }

        private void ButtonTrim_Click(object sender, EventArgs e)
        {
            int leftEdge = bitmap[0].Count()-1;
            int rightEdge = 0;
            int topEdge = 0;
            int bottemEdge = bitmap.Count;
            bool topDone = false;
            for (int i = 0; i < bitmap.Count; i++)
            {
                (bool isEdge, int lineLeftEdge, int lineRightEdge) = ProcessLine(bitmap[i]);
                rightEdge = Math.Max(rightEdge, lineRightEdge);
                leftEdge = Math.Min(leftEdge, lineLeftEdge);
                if (!topDone)
                {
                    if (isEdge)
                    {
                        topEdge = i;
                    }
                    else
                    {
                        topEdge = i;
                        topDone = true;
                    }
                }
                else
                {
                    if (!isEdge)
                    {
                        bottemEdge = i;
                    }
                }
            }
            Trim(leftEdge, rightEdge, topEdge, bottemEdge);
            string result = "";
            foreach (Color c in defaultColors)
            {
                result += "#" + ((int)c.R).ToString("X2") + ((int)c.G).ToString("X2") + ((int)c.B).ToString("X2") + "\n";
            }
            result += "\n";
            foreach (string l in bitmap)
            {
                result += l + "\n";
            }
            currentResult = result;
            textBoxResult.Text = currentResult.Replace("\n", "\r\n");
        }

        private void Trim(int leftEdge, int rightEdge, int topEdge, int bottemEdge)
        {
            bitmap.RemoveRange(bottemEdge + 1, bitmap.Count - bottemEdge - 1);
            bitmap.RemoveRange(0, topEdge);
            for (int i = 0; i < bitmap.Count; i++)
            {
                bitmap[i] = bitmap[i].Substring(leftEdge, rightEdge - leftEdge + 1);
            }
        }

        private (bool, int, int) ProcessLine(string line)
        {
            bool edge = true;
            int leftEdge = line.Length - 1;
            int rightEdge = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != 'T')
                {
                    edge = false;
                    rightEdge = i;
                }
                if (line[line.Length - 1 - i] != 'T')
                {
                    leftEdge = line.Length - 1 - i;
                }
            }
            return (edge, leftEdge, rightEdge);
        }
    }
}
