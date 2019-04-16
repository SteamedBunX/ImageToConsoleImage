using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageToConsoleImage
{
    public partial class Form1 : Form
    {
        Bitmap sourceImage;
        List<Color> defaultColors;
        string currentResult = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open An Image File";
            theDialog.Filter = "Image Files|*.png;*.jpg";
            theDialog.InitialDirectory = @"Desktop";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string fileName = theDialog.FileName;
                    //MessageBox.Show(fileName);
                    sourceImage = new Bitmap(fileName);
                    panelImage.BackgroundImage = sourceImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            string currentResult = processImage(sourceImage);
            string ciText = currentResult;
            textBoxResult.Text = ciText.Replace("\n", "\r\n");

        }

        public string processImage(Bitmap img)
        {
            string result = "";
            result += getDefultColor(img);
            result += ProcessPixels(img);
            return result;
        }

        public string getDefultColor(Bitmap img)
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
                            defaultColors.Add(currentP);
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
            for (int y = 0; y < img.Height; y += 2)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    result += ProcessPixel(img.GetPixel(x, y), defaultColors);
                    result += ProcessPixel(img.GetPixel(x, y + 1), defaultColors);
                }
                result += "\n";
            }
            return result;
        }

        public char ProcessPixel(Color color, List<Color> defCs)
        {
            if (color.A == 0)
            {
                return '0';
            }
            if (color.ToArgb() == Color.Black.ToArgb())
            {
                return '0';
            }

            for (int i = 0; i < defCs.Count; i++)
            {
                if (color.ToArgb() == defCs[i].ToArgb())
                {
                    switch (i)
                    {
                        case 0:
                            return '1';
                        case 1:
                            return '2';
                        case 2:
                            return '3';
                        case 3:
                            return '4';
                        case 4:
                            return '5';
                        case 5:
                            return '6';
                        case 6:
                            return '7';
                        case 7:
                            return '8';
                        case 8:
                            return '9';
                        case 9:
                            return 'A';
                        case 10:
                            return 'B';
                        case 11:
                            return 'C';
                        case 12:
                            return 'D';
                        case 13:
                            return 'E';
                        default:
                            break;

                    }
                }
            }
            char result = '0';
            return result;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            StreamWriter myStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                myStream = new StreamWriter(saveFileDialog.FileName);

                myStream.Write(currentResult);
                myStream.Close();
            }
        }
    }
}
