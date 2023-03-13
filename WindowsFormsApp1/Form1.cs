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

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        List<PictureBox> pictureBoxes = new List<PictureBox>();

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {


        }

        private void AddImageButton_Click(object sender, EventArgs e)
        {
            //restrict to 10 images max
            if (pictureBoxes.Count >= 10)
            {
                MessageBox.Show("You can only add 10 images");
                return;
            }
            //open diaglog box
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                //add picturebox to splitPanel.Panel2
                PictureBox pb = new PictureBox();
                pb.Image = new Bitmap(open.FileName);
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Size = new Size(100, 100);
                pb.Click += new EventHandler(pb_Click);
                splitContainer1.Panel2.Controls.Add(pb);

                //add caption to top left corner of picture box
                Label lbl = new Label();
                //index of image in list
                lbl.Text = pictureBoxes.Count.ToString();
                //only the filename is stored in the tag
                lbl.Tag = open.FileName.Substring(open.FileName.LastIndexOf("\\") + 1);
                lbl.AutoSize = true;
                lbl.BackColor = Color.Black;
                lbl.ForeColor = Color.White;
                lbl.Location = new Point(0, 0);
                pb.Controls.Add(lbl);
                

                //add picturebox to list
                pictureBoxes.Add(pb);

                //tile the pictureboxes
                int x = 0;
                int y = 0;
                foreach (PictureBox p in pictureBoxes)
                {
                    p.Location = new Point(x, y);
                    x += p.Width;
                    if (x + p.Width > splitContainer1.Panel2.Width)
                    {
                        x = 0;
                        y += p.Height;
                    }
                }
                //if there are too many pictureboxes, add a scroll bar and align them all horizontally
                if (y > splitContainer1.Panel2.Height)
                {
                    splitContainer1.Panel2.AutoScroll = true;
                    foreach (PictureBox p in pictureBoxes)
                    {
                        p.Location = new Point(x, 0);
                        x += p.Width;
                    }
                }


            }

        }

        //create pb_Click event
        void pb_Click(object sender, EventArgs e)
        {
            //get the picturebox
            PictureBox pb = (PictureBox)sender;

            //if right click
            if (e is MouseEventArgs && ((MouseEventArgs)e).Button == MouseButtons.Right)
            {
                //remove the picturebox from the list
                pictureBoxes.Remove(pb);

                //remove the picturebox from the splitPanel.Panel2
                splitContainer1.Panel2.Controls.Remove(pb);

                //recalculate positions of pictureboxes and relabel
                int x = 0;
                int y = 0;
                int i = 0;
                foreach (PictureBox p in pictureBoxes)
                {
                    p.Location = new Point(x, y);
                    x += p.Width;
                    if (x + p.Width > splitContainer1.Panel2.Width)
                    {
                        x = 0;
                        y += p.Height;
                    }
                    p.Controls[0].Text = i.ToString();
                    i++;
                }


            }

            else if (e is MouseEventArgs && ((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                //set the picturebox to the picturebox in splitPanel.Panel1
                PreviewBox.Image = pb.Image;

                //scale so image isnt streched, but doesnt get cut off
                if (PreviewBox.Image.Width > PreviewBox.Width || PreviewBox.Image.Height > PreviewBox.Height)
                {
                    PreviewBox.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    PreviewBox.SizeMode = PictureBoxSizeMode.CenterImage;
                }

                //adjust aspect ratio
                PreviewBox.Height = (int)(PreviewBox.Width * ((double)PreviewBox.Image.Height / PreviewBox.Image.Width));

                //fit picture box to area
                PreviewBox.Top = (splitContainer1.Panel1.Height - PreviewBox.Height) / 2;


                //update labels
                FileNameLabel.Text = "File Name: " + pb.Controls[0].Tag.ToString();
                ImageSizeLabel.Text = "Image Size: " + pb.Image.Width + " x " + pb.Image.Height;
                FileSizeLabel.Text = "File Size: " + GetBGR565Length(pb.Image) + " bytes";

                //show border around picture box that is selected
                foreach (PictureBox p in pictureBoxes)
                {
                    p.BorderStyle = BorderStyle.None;
                }
                
                pb.BorderStyle = BorderStyle.FixedSingle;

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private int GetBGR565Length(Image image)
        {
            Bitmap bmp = new Bitmap(image);
            byte[] bgr565 = new byte[bmp.Width * bmp.Height * 2];
            int i = 0;
            
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color c = bmp.GetPixel(x, y);
                    bgr565[i] = (byte)((c.B & 0xF8) | (c.G >> 5));
                    bgr565[i + 1] = (byte)(((c.G & 0x1C) << 3) | (c.R >> 3));
                    i += 2;
                }
            }
            return bgr565.Length;
        }
        

        private void button1_Click(object sender, EventArgs e)
        {

            //get all the images in the list as bgr565
            List<byte[]> images = new List<byte[]>();
            foreach (PictureBox p in pictureBoxes)
            {
                Bitmap bmp = new Bitmap(p.Image);
                byte[] bgr565 = new byte[bmp.Width * bmp.Height * 2];
                int i = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        bgr565[i] = (byte)((c.B & 0xF8) | (c.G >> 5));
                        bgr565[i + 1] = (byte)(((c.G & 0x1C) << 3) | (c.R >> 3));
                        i += 2;
                    }
                }
                images.Add(bgr565);
            }

            //create table of contents
            Int32 numberOfImages = images.Count;

            //open file save dialog
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Binary File(*.bin)|*.bin";

            if (save.ShowDialog() == DialogResult.OK)
            {

                //binary writer
                System.IO.BinaryWriter bw = new BinaryWriter(File.Open(save.FileName, FileMode.Create));

                //write number of iamges
                bw.Write(numberOfImages);
                int index = 0;
                int totalOffset = 0;
                int baseOffset = ((4 * numberOfImages) * 3) + 4;
                
                foreach (PictureBox p in pictureBoxes)
                {
                    bw.Write((Int32)p.Image.Width);
                    bw.Write((Int32)p.Image.Height);
                    int fileSize = (Int32)images[index].Length;

                    //calculate offset from start of file for each previous file added
                    int offset = 0;

                    if (index == 0)
                    {
                        offset = baseOffset;
                    }
                    else
                    {
                        totalOffset += (int)images[index - 1].Length;
                        offset = baseOffset + totalOffset;
                    }
                    bw.Write(offset);
                    index++;
                }

                //write each image bytes to file
                foreach (byte[] b in images)
                {
                    bw.Write(b);
                }
                //close file
                bw.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void RemoveImagesButton_Click(object sender, EventArgs e)
        {
            //remove all pictures boxes
            pictureBoxes.Clear();

            //remove all picture boxes from splitPanel.Panel2
            splitContainer1.Panel2.Controls.Clear();
            
        }
    }
}