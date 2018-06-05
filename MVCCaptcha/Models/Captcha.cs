using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCCaptcha.Models
{
    public class Captcha
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ControllerContext Context { get; set; }

        public Captcha(int _length, int _width, int _height, ControllerContext _context)
        {
            Length = _length;
            Width = _width;
            Height = _height;
            Context = _context;
        }

        public Captcha(ControllerContext _context) : this(_length: 0, _width: 0, _height: 0, _context: _context)
        {
            //default constructor
        }
        
        public byte[] create_captcha()
        {
            Bitmap objBMP = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            Graphics objGraphics = Graphics.FromImage(objBMP);
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;

            //Font which will be used to create captcha
            Font objFont = new Font(FontFamilySelector(), 18, FontStyle.Bold);

            HatchBrush hatchBrush = new HatchBrush(
                RandomHatchStyle(),
                Color.LightGray,
                Color.White);

            //back ground shape
            Rectangle rect = new Rectangle(0, 0, Width, Height);

            //building the background
            objGraphics.FillRectangle(hatchBrush, rect);

            //random string to make captcha charahcter sequence
            char[] randomstr_char_arr = CreateRandomString(Length).ToCharArray();


            //rotate and color randomly each charachter in sequense
            Matrix mymat = new Matrix();
            Random rnd = new Random();

            AddColorRotation(objGraphics, objFont, randomstr_char_arr, mymat, rnd);


            //reassigning hatchBrush to add some noise to tha captcha image
            hatchBrush = new HatchBrush(
                HatchStyle.ZigZag,
                Color.Blue,
                Color.Red);

            //adding some noises
            rect = AddNoise(objGraphics, hatchBrush, rect, rnd);

            //paths and curves to make distortion
            GraphicsPath path = new GraphicsPath();
            Pen mypen = new Pen(RandomBrushes(), 1);

            AddCurves(objGraphics, rect, rnd, path, mypen);


            //gharar dadane string captcha sar session baraye authenticate
            //Session.Add("randomStr", randomstr_char_arr.ToString());

            string captchaString = new string(randomstr_char_arr);
            Context.HttpContext.Session["Captcha"] = captchaString.ToLower();


            //in order to return image without saving ion disk we need to return image inside memory 
            //in that case we need to convert it to byteArray
            //mvc action controll will use this byte array and will convert it to File
            //File will be used inside view inside img tag
            //<img src="@Url.Action("GenerateCaptcha")" alt="qr code" />
            byte[] byteArray = ConvertToByteArray(objBMP);

            path.Dispose();
            objFont.Dispose();
            objGraphics.Dispose();
            objBMP.Dispose();

            return byteArray;
        }



        private string FontFamilySelector()
        {
            string[] fontFamilyList =
            {
                "Arial Black",
                "Fantasy",
                "Franklin Gothic Heavy",
                "Helvetica",
                "Impact",
                "Lucida Console",
                "MS Sans Serif"
            };

            Random r = new Random();

            return fontFamilyList[r.Next(fontFamilyList.Length)];
        }

        private void AddColorRotation(Graphics objGraphics, Font objFont, char[] randomstr_char_arr, Matrix mymat, Random rnd)
        {
            for (int i = 0; i < Length; ++i)
            {
                mymat.Reset();
                mymat.RotateAt(rnd.Next(-30, 0), new PointF((float)(Width * (0.12 * i)), (float)(Height * 0.5)));
                objGraphics.Transform = mymat;
                objGraphics.DrawString(randomstr_char_arr[i].ToString(), objFont, RandomBrushes(), (float)(Width * (0.15 * i)), (float)(Height * 0.4));
                objGraphics.ResetTransform();
            }
        }

        private void AddCurves(Graphics objGraphics, Rectangle rect, Random rnd, GraphicsPath path, Pen mypen)
        {

            //declaring first curve's random turning points
            float v = 4F;
            PointF[] points =
              {
                new PointF(
                  rnd.Next(rect.Width) / v,
                  rnd.Next(rect.Height) / v),
                new PointF(
                  rect.Width - rnd.Next(rect.Width) / v,
                  rnd.Next(rect.Height) / v),
                new PointF(
                  rnd.Next(rect.Width) / v,
                  rect.Height - rnd.Next(rect.Height) / v),
                new PointF(
                  rect.Width - rnd.Next(rect.Width) / v,
                  rect.Height - rnd.Next(rect.Height) / v)
              };

            //showing first curve
            path.AddCurve(points);
            objGraphics.DrawPath(mypen, path);
            path.Reset();

            //declaring second curve's random turning points
            for (int j = 0; j < 4; ++j)
            {
                points[j] = new PointF(rnd.Next(rect.Width), rnd.Next(rect.Height));
            }

            //showing second curve with diffrent pen brush
            path.AddCurve(points);
            mypen.Brush = RandomBrushes();
            objGraphics.DrawPath(mypen, path);
        }

        private static Rectangle AddNoise(Graphics objGraphics, HatchBrush hatchBrush, Rectangle rect, Random rnd)
        {
            int m = Math.Max(rect.Width, rect.Height);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                int x = rnd.Next(rect.Width);
                int y = rnd.Next(rect.Height);
                int w = rnd.Next(m / 50);
                int h = rnd.Next(m / 50);
                objGraphics.FillEllipse(hatchBrush, x, y, w, h);
            }

            return rect;
        }

        private static byte[] ConvertToByteArray(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        private static byte[] ConvertToByteArray2(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private string CreateRandomString(int lenght)
        {
            int i;
            string res = "";
            string source_str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] source_char = source_str.ToCharArray();
            Random rnd = new Random();
            for (i = 0; i < lenght; ++i)
            {
                res += source_char[rnd.Next(62)];
            }

            return res;
        }

        private Brush RandomBrushes()
        {
            Brush[] mybrush = { Brushes.Red, Brushes.Orange, Brushes.Crimson, Brushes.ForestGreen, Brushes.Green, Brushes.Blue, Brushes.Black };
            Random rnd = new Random();
            System.Threading.Thread.Sleep(5);
            return mybrush[rnd.Next(7)];
        }

        private Brush RandomBrushes2()
        {
            Brush[] mybrush2 = { Brushes.LightSlateGray, Brushes.Navy, Brushes.Orchid, Brushes.Magenta, Brushes.LightGreen, Brushes.Cyan, Brushes.DarkOrange };
            Random rnd2 = new Random();
            System.Threading.Thread.Sleep(5);
            return mybrush2[rnd2.Next(7)];
        }

        private HatchStyle RandomHatchStyle()
        {
            HatchStyle[] myhatchs = { HatchStyle.ZigZag, HatchStyle.DarkDownwardDiagonal, HatchStyle.OutlinedDiamond, HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross, HatchStyle.SmallConfetti };
            Random rnd3 = new Random();
            return myhatchs[rnd3.Next(6)];
        }


    }
}