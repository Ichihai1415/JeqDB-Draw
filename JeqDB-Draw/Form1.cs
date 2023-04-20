using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace JeqDB_Draw
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap canvas = new Bitmap(1080, 1080);
        List<Data> dataList = new List<Data>();
        double LatSta = 20;
        double LatEnd = 50;
        double LonSta = 120;
        double LonEnd = 150;
        double ZoomW = 36;
        double ZoomH = 36;
        int a = 204;

        private void Form1_Load(object sender, EventArgs e)
        {
            canvas = new Bitmap(2160, 2160);
            ZoomW = canvas.Width / (LonEnd - LonSta);
            ZoomH = canvas.Height / (LatEnd - LatSta);
            Console.WriteLine("画像サイズ:" + canvas.Size.Width + "," + canvas.Size.Width);
            Console.WriteLine("緯度始点:" + LatSta);
            Console.WriteLine("緯度終点:" + LatEnd);
            Console.WriteLine("経度始点:" + LonSta);
            Console.WriteLine("経度終点:" + LonEnd);
            Console.WriteLine("横ズーム:" + ZoomW);
            Console.WriteLine("縦ズーム:" + ZoomH);
            Console.WriteLine("透明度:" + a);

            Console.WriteLine("マップ描画開始");
            DrawMap();
            Console.WriteLine("マップ描画終了");

        }

        private void RC_Draw_Click(object sender, EventArgs e)
        {
            Console.WriteLine("情報変換開始");

            ConvertData();
            Console.WriteLine("情報変換終了");
            Console.WriteLine("データ個数:" + dataList.Count);
            Console.WriteLine("情報描画開始");

            Bitmap bitmap = (Bitmap)canvas.Clone();
            Graphics g = Graphics.FromImage(bitmap);
            foreach (Data data in dataList)
            {
                int size = (int)(data.Mag * data.Mag * canvas.Width / 1080);
                //int size = (int)(data.Mag * canvas.Width / 200);
                g.FillEllipse(Depth2Color(data.Depth), (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                g.DrawEllipse(Pens.Gray, (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
            }
            MapImg.BackgroundImage = bitmap;
            g.Dispose();
            Console.WriteLine("情報描画終了");
            bitmap.Save("output.png", ImageFormat.Png);

        }
        private SolidBrush Depth2Color(int Depth, int alpha = 255)
        {
            int r, g, b;
            if (Depth <= 10)
            {
                r = Depth * 12 + 130;
                g = 0;
                b = 0;
            }
            else if (Depth <= 20)
            {
                r = 255;
                g = (int)((Depth - 10) * 12.5);
                b = 0;
            }
            else if (Depth <= 30)
            {
                r = 255;
                g = (int)((Depth - 10) * 12.5);
                b = 0;
            }
            else if (Depth <= 50)
            {
                r = 255;
                g = 255;
                b = 0;
            }
            else if (Depth <= 100)
            {
                r = 255 - (Depth - 50) * 5;
                g = (int)(255 - (Depth - 50) * 2.5);
                b = 0;
            }
            else if (Depth <= 200)
            {
                r = 7 + (Depth - 100) / 4;
                g = 130 + (Depth - 100) / 10;
                b = (int)((Depth - 100) * 2.5);
            }
            else if (Depth <= 700)
            {
                r = 31 - (Depth - 200) / 16;
                g = 140 - (Depth - 200) / 4;
                b = 255 - (Depth - 200) / 4;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
            return new SolidBrush(Color.FromArgb(alpha, r, g, b));
        }
        private void ConvertData()
        {
            string csv = "";
            while (csv == "")
                try
                {
                    Console.WriteLine("ファイルパスを入力してください。");
                    csv = File.ReadAllText(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            string[] data = csv.Split('\n');
            dataList = new List<Data>();
            foreach (string data_ in data)
            {
                string[] data__ = data_.Split(',');
                if (!data__[0].Contains('/'))
                    continue;
                string[] HypoLats = data__[3].Split('°');
                double HypoLat = -1;
                if (data__[3] != "不明")
                    HypoLat = double.Parse(HypoLats[0]) + double.Parse(HypoLats[1].Substring(0, 3)) / 60;
                string[] HypoLons = data__[4].Split('°');
                double HypoLon = -1;
                if (data__[4] != "不明")
                    HypoLon = double.Parse(HypoLons[0]) + double.Parse(HypoLons[1].Substring(0, 4)) / 60;
                Data datas = new Data()
                {
                    Time = DateTime.Parse(data__[0] + " " + data__[1]),
                    HypoName = data__[2],
                    Lat = HypoLat,
                    Lon = HypoLon,
                    Depth = int.Parse(data__[5].Split(' ')[0].Replace("不明", "-1")),
                    Mag = double.Parse(data__[6].Replace("不明", "-1")),
                    MaxInt = data__[7]
                };
                dataList.Add(datas);
            }
        }
        private void DrawMap()
        {
            JObject json = JObject.Parse(File.ReadAllText("Map-world.geojson"));
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.FromArgb(30, 30, 60));
            GraphicsPath Maps = new GraphicsPath();
            Maps.StartFigure();
            foreach (JToken json_1 in json.SelectToken("features"))
            {
                if (json_1.SelectToken("geometry").Count() == 0)
                    continue;
                List<Point> points = new List<Point>();
                foreach (JToken json_2 in json_1.SelectToken($"geometry.coordinates[0]"))
                    points.Add(new Point((int)(((double)json_2.SelectToken("[0]") - LonSta) * ZoomW), (int)((LatEnd - (double)json_2.SelectToken("[1]")) * ZoomH)));
                if (points.Count > 2)
                    Maps.AddPolygon(points.ToArray());
            }
            g.FillPath(new SolidBrush(Color.FromArgb(100, 100, 150)), Maps);

            json = JObject.Parse(File.ReadAllText("Map-jp.geojson"));
            Maps.Reset();
            Maps.StartFigure();
            foreach (JToken json_1 in json.SelectToken("features"))
            {
                if ((string)json_1.SelectToken("geometry.type") == "Polygon")
                {
                    List<Point> points = new List<Point>();
                    foreach (JToken json_2 in json_1.SelectToken($"geometry.coordinates[0]"))
                        points.Add(new Point((int)(((double)json_2.SelectToken("[0]") - LonSta) * ZoomW), (int)((LatEnd - (double)json_2.SelectToken("[1]")) * ZoomH)));
                    if (points.Count > 2)
                        Maps.AddPolygon(points.ToArray());
                }
                else
                {
                    foreach (JToken json_2 in json_1.SelectToken($"geometry.coordinates"))
                    {
                        List<Point> points = new List<Point>();
                        foreach (JToken json_3 in json_2.SelectToken("[0]"))
                            points.Add(new Point((int)(((double)json_3.SelectToken("[0]") - LonSta) * ZoomW), (int)((LatEnd - (double)json_3.SelectToken("[1]")) * ZoomH)));
                        if (points.Count > 2)
                            Maps.AddPolygon(points.ToArray());
                    }
                }
            }
            g.FillPath(new SolidBrush(Color.FromArgb(90, 90, 120)), Maps);
            g.DrawPath(new Pen(Color.FromArgb(255, 255, 255), 1), Maps);




            g.Dispose();
            MapImg.BackgroundImage = canvas;
        }

        private void MapImg_Click(object sender, EventArgs e)
        {
            //RC_Draw_Click(sender,e);
        }

        private void RC_output_Click(object sender, EventArgs e)
        {
            ConvertData();

            List<Data> dataList_ = new List<Data>(dataList);
            Console.WriteLine("ソート開始");
            dataList_.Sort((a, b) => a.Time.CompareTo(b.Time));
            Console.WriteLine("ソート終了");

            string SaveDire = "3";
            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");
            if (!Directory.Exists("output\\" + SaveDire))
                Directory.CreateDirectory("output" + SaveDire);
            DateTime DrawStartDate = new DateTime(2022, 1, 1);
            DateTime DrawEndDate = new DateTime(2023, 1, 1);
            TimeSpan DrawSpan = new TimeSpan(6, 0, 0);//tごとに描画
            TimeSpan DisappSpan = new TimeSpan(24, 0, 0);//消える時間
            //      | transDraw |  normalDraw |
            //      |           |             |
            //      |           |             |
            //------|-----------|-------------|----------
            //  disAppTime  drawTimeSta  drawTimeEnd
            //
            //disAppTime = DrawTime + DrawSpan - DisappSpan
            //drawTimeSta = DrawTime
            //drawTimeEnd = DrawTime + DrawSpan
            DateTime DrawTime = DrawStartDate;//描画対象時間
            for (int i = 1; DrawTime < DrawEndDate; i++)
            {
                List<Data> dataList__ = new List<Data>();
                List<Data> dataList_cp = new List<Data>(dataList_);
                foreach (Data data in dataList_cp)//古<新==true
                {
                    if (data.Time < DrawTime + DrawSpan - DisappSpan)//描画終了
                        dataList_.RemoveAt(0);
                    else if (data.Time < DrawTime + DrawSpan)//描画
                        dataList__.Add(data);
                    else//未描画
                        break;
                }
                Bitmap bitmap = (Bitmap)canvas.Clone();
                Graphics g = Graphics.FromImage(bitmap);
                foreach (Data data in dataList__)
                {
                    int size = (int)(data.Mag * data.Mag * canvas.Width / 1080);
                    //int size = (int)(data.Mag * canvas.Width / 200);
                    int alpha;

                    if (dataList_[0].Time < DrawTime + DrawSpan + DrawSpan)
                        alpha = 204;
                    else if (dataList_[0].Time < DrawTime + DrawSpan)
                        alpha = 136;
                    else
                        alpha = 68;
                    g.FillEllipse(Depth2Color(data.Depth, alpha), (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                    g.DrawEllipse(Pens.Gray, (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                }
                g.DrawString(DrawTime.ToString("yyyy/MM/dd HH:mm:ss"), new Font("Roboto", 100), Brushes.Black, 0, 0);
                bitmap.Save($"output\\{SaveDire}\\{string.Format("{0:D4}", i)}.png", ImageFormat.Png);
                g.Dispose();
                Console.WriteLine($"{DrawTime}({i}):{dataList__.Count}");
                DrawTime += DrawSpan;
            }//ffmpeg -framerate 30 -i %04d.png -vcodec libx264 -pix_fmt yuv420p -r 30 _output.mp4
        }
    }
    public class Data
    {
        public DateTime Time { get; set; }
        public string HypoName { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Depth { get; set; }
        public double Mag { get; set; }
        public string MaxInt { get; set; }
    }
}
