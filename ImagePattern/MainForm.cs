using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;

namespace ImagePattern
{
    public partial class MainForm : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd01, int hWnd02, string lpsz01, string lpsz02);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        IntPtr m_hWndMain = new IntPtr();
        IntPtr m_hWndCanvas = new IntPtr();
        RECT m_rcLocation = new RECT();
        RECT m_rcSize = new RECT();

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        private const int WM_LBUTTONDOWN = 0x0002;
        private const int WM_LBUTTONUP = 0x0004;
        private const int WM_LBUTTONDBLCLK = 0x203;

        Point m_ptStartDot = new Point(0, 0);
        Bitmap bmpScreen;

        List<IplImage> m_ltPatterns = new List<IplImage>();
        List<Point> m_ltPoints = new List<Point>();

        public MainForm()
        {
            InitializeComponent();
        }
        private void Load()
        {
            m_hWndMain = FindWindow(null, "Mobizen");

            GetWindowRect(m_hWndMain, ref m_rcLocation); // Top, Left
            GetClientRect(m_hWndMain, ref m_rcSize); // Bottom, Right

            if (m_hWndMain == IntPtr.Zero)
                return;

            m_ptStartDot = new Point((m_rcLocation.left), (m_rcLocation.top));

            m_ltPatterns.Add(new IplImage("Pattern01.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern02.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern03.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern04.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern05.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern06.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern07.bmp", LoadMode.GrayScale));

            foreach (IplImage iplTemp in m_ltPatterns)
            {
                Cv.Threshold(iplTemp, iplTemp, 200, 255, ThresholdType.Binary);
            }

            
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            Load();
        }

        private void Start()
        {
            if (m_hWndMain == IntPtr.Zero)
                return;

            Size szScreen = new Size(m_rcSize.right, m_rcSize.bottom);
            Bitmap bmpScreen = new Bitmap(szScreen.Width, szScreen.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmpScreen);
            g.CopyFromScreen(m_rcLocation.left, m_rcLocation.top, 0, 0, szScreen);

            string text = DateTime.Now.Millisecond.ToString() + "\r\n";
            bmpScreen.Save(DateTime.Now.Millisecond.ToString()+".bmp");

            IplImage imgConvertBitmap = IplImage.FromBitmap(bmpScreen);
            IplImage imgMain = new IplImage(new CvSize(imgConvertBitmap.Width, imgConvertBitmap.Height), BitDepth.U8, 1);

            Cv.CvtColor(imgConvertBitmap, imgMain, ColorConversion.RgbToGray);
            Cv.Threshold(imgMain, imgMain, 200, 255, ThresholdType.Binary);

            foreach (IplImage iplTemp in m_ltPatterns)
            {
                int iWidth = imgMain.Width - iplTemp.Width + 1;
                int iHeight = imgMain.Height - iplTemp.Height + 1;

                IplImage imgResult = new IplImage(Cv.Size(iWidth, iHeight), BitDepth.F32, 1);
                Cv.MatchTemplate(imgMain, iplTemp, imgResult, MatchTemplateMethod.CCoeffNormed);

                double dMin, dMax;
                CvPoint ptLeftTop;
                CvPoint ptRightBottom;
                Cv.MinMaxLoc(imgResult, out dMin, out dMax, out ptRightBottom, out ptLeftTop);

                m_ltPoints.Add(new Point(m_ptStartDot.X + ptLeftTop.X + 15, m_ptStartDot.Y + ptLeftTop.Y + 15));
                
                
            }

            //
            //102 369

            SetCursorPos(m_ptStartDot.X, m_ptStartDot.Y);

            
            foreach (Point ptLocation in m_ltPoints)
            {
                text+= "X : " + ptLocation.X + ", " + ptLocation.Y + "\r\n";

                SetCursorPos(ptLocation.X, ptLocation.Y);

                Thread.Sleep(72);

                mouse_event(WM_LBUTTONDOWN, 0, 0, 0, 0);

                Thread.Sleep(65);

                mouse_event(WM_LBUTTONUP, 0, 0, 0, 0);

                Thread.Sleep(65);
            }

            pictureBoxIpl.ImageIpl = imgMain;

            pictureBoxIpl.Invalidate();

            System.IO.File.WriteAllText("Recent_crood.txt", text);  // 로그 쓴다
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void btnSample_Click(object sender, EventArgs e)
        {
            m_ltPoints.Clear();
            m_ltPatterns.Clear();

            Bitmap bmpScreen = new Bitmap("575.bmp");
            IplImage imgConvertBitmap = IplImage.FromBitmap(bmpScreen);
            IplImage imgMain = new IplImage(new CvSize(imgConvertBitmap.Width, imgConvertBitmap.Height), BitDepth.U8, 1);
            Cv.CvtColor(imgConvertBitmap, imgMain, ColorConversion.RgbToGray);
            Cv.Threshold(imgMain, imgMain, 210, 255, ThresholdType.Binary);

            m_ltPatterns.Add(new IplImage("Pattern01.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern02.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern03.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern04.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern05.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern06.bmp", LoadMode.GrayScale));
            m_ltPatterns.Add(new IplImage("Pattern07.bmp", LoadMode.GrayScale));

            foreach (IplImage iplTemp in m_ltPatterns)
            {
                Cv.Threshold(iplTemp, iplTemp, 210, 255, ThresholdType.Binary);
            }

            foreach (IplImage iplTemp in m_ltPatterns)
            {
                int iWidth = imgMain.Width - iplTemp.Width + 1;
                int iHeight = imgMain.Height - iplTemp.Height + 1;

                IplImage imgResult = new IplImage(Cv.Size(iWidth, iHeight), BitDepth.F32, 1);
                Cv.MatchTemplate(imgMain, iplTemp, imgResult, MatchTemplateMethod.CCorrNormed);

                double dMin, dMax;
                CvPoint ptLeftTop;
                CvPoint ptRightBottom;
                Cv.MinMaxLoc(imgResult, out dMin, out dMax, out ptRightBottom, out ptLeftTop);
                //cvRectangle(A, left_top, cvPoint(left_top.x + B->width, left_top.y + B->height), CV_RGB(255, 0, 0)); 
                Cv.Rectangle(imgMain, ptLeftTop, new CvPoint(ptLeftTop.X + iplTemp.Width, ptLeftTop.Y + iplTemp.Height), Cv.RGB(255, 255, 255));

                m_ltPoints.Add(new Point(ptLeftTop.X, ptLeftTop.Y));
            }

            pictureBoxIpl.ImageIpl = imgMain;

            pictureBoxIpl.Invalidate();
        }

        private void pictureBoxIpl_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Font drawFont = new Font("Arial", 14, System.Drawing.FontStyle.Bold);
            SolidBrush drawBrush = new SolidBrush(Color.Red);

            if (pictureBoxIpl.Image == null)
                return;

            float fWidthRate = (float)pictureBoxIpl.Size.Width / (float)pictureBoxIpl.Image.Size.Width;
            float fHeightRate = (float)pictureBoxIpl.Size.Height / (float)pictureBoxIpl.Image.Size.Height;

            int iIndex = 0;
            foreach (PointF ptLocation in m_ltPoints)
            {
                iIndex++;

                g.DrawString(iIndex.ToString(), drawFont, drawBrush, new PointF(ptLocation.X * fWidthRate, ptLocation.Y * fHeightRate));
            }
        }
    }
}
