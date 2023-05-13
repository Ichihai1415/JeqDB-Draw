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
        Bitmap BaseMap;
        List<Data> dataList = new List<Data>();

        //全体
        readonly double LatSta = 20;
        readonly double LatEnd = 50;
        readonly double LonSta = 120;
        readonly double LonEnd = 150;
        readonly int MapWidth = 2160;
        readonly int MapHeight = 2160;

        //能登拡大
        //readonly double LatSta = 37;
        //readonly double LatEnd = 38;
        //readonly double LonSta = 136.8;
        //readonly double LonEnd = 137.8;
        //readonly int MapWidth = 1080;
        //readonly int MapHeight = 1080;

        double ZoomW;
        double ZoomH;
        readonly int a = 204;
        readonly string SaveDire = "7";

        private void Form1_Load(object sender, EventArgs e)
        {
            BaseMap = new Bitmap(MapHeight * 16 / 9, MapHeight);
            ZoomW = MapWidth / (LonEnd - LonSta);
            ZoomH = MapHeight / (LatEnd - LatSta);
            Console.WriteLine("画像サイズ:" + BaseMap.Size.Width + "," + BaseMap.Size.Height);
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

            Bitmap bitmap = (Bitmap)BaseMap.Clone();
            Graphics g = Graphics.FromImage(bitmap);
            foreach (Data data in dataList)
            {
                //int size = (int)(data.Mag * data.Mag * MapHeight / 1080);
                int size = (int)(data.Mag * MapHeight / 216);
                size *= 3;//拡大時仮
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
            /*
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
            */
            //震度データベースjsより
            double l = 50;
            double h = 0;
            if (Depth <= 10)
                l = 50 - 25 * ((10 - Depth) / 10);
            else if (Depth <= 20)
                h = 0 + 30 * ((Depth - 10) / 10);
            else if (Depth <= 30)
                h = 30 + 30 * ((Depth - 20) / 10);
            else if (Depth <= 50)
                h = 60;
            else if (Depth <= 100)
            {
                h = 60 + 60 * ((Depth - 50) / 50);
                l = 50 + 25 * ((50 - Depth) / 100);
            }
            else if (Depth <= 200)
            {
                h = 120 + 90 * ((Depth - 100) / 100);
                l = 25 - 30 * ((100 - Depth) / 100);
            }
            else if (Depth <= 700)
            {
                h = 210 + 30 * ((Depth - 200) / 500);
                l = 55 + 30 * ((200 - Depth) / 500);
            }
            else
            {
                h = 240;
                l = 25;
            }
            return new SolidBrush(HSL2RGB(h / 255, 1, l / 100, alpha));
        }
        private void ConvertData()
        {
            string csv = "";
            while (csv == "")
                try
                {
                    Console.WriteLine("ファイルパスを入力してください。");
                    csv = File.ReadAllText(Console.ReadLine().Replace("\"", ""));
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
            Graphics g = Graphics.FromImage(BaseMap);
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
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 60, 90)), MapWidth, 0, BaseMap.Width - MapWidth, BaseMap.Height);
            g.Dispose();
            MapImg.BackgroundImage = BaseMap;
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
            dataList_.Sort((a, b) => a.Time.CompareTo(b.Time));//古い順
            Console.WriteLine("ソート終了");
            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");
            if (!Directory.Exists($"output\\{SaveDire}"))
                Directory.CreateDirectory($"output\\{SaveDire}");
            DateTime DrawStartDate = new DateTime(2022, 1, 1);//描画開始
            DateTime DrawEndDate = new DateTime(2023, 1, 1);//描画終了
            TimeSpan DrawSpan = new TimeSpan(6, 0, 0);//tごとに描画
            TimeSpan DisappSpan = new TimeSpan(24, 0, 0);//消える時間

            //DrawStartDate = new DateTime(2023, 5, 5, 14, 30, 0);
            //DrawEndDate = new DateTime(2023, 5, 5, 15, 30, 0);
            //DrawSpan = new TimeSpan(0, 1, 0);
            //DisappSpan = new TimeSpan(0, 10, 0);


            //能登
            //DrawStartDate = new DateTime(2023, 5, 5, 14, 0, 0);
            //DrawEndDate = new DateTime(2023, 5, 6, 2, 0, 0);
            //DrawSpan = new TimeSpan(0, 1, 0);
            //DisappSpan = new TimeSpan(0, 15, 0);

            //
            //        | DisappSpan|  DrawSpan   |
            // noDraw | transDraw | normalDraw  | noDraw
            // -------|-----------|-------------|----------
            //    disAppTime  drawTimeSta  drawTimeEnd
            //
            DateTime DrawTime = DrawStartDate;//描画対象時間
            Console.WriteLine("画像作成開始");
            for (int i = 1; DrawTime < DrawEndDate; i++)//DateTime:古<新==true
            {
                List<Data> dataList_Draw = new List<Data>();
                List<Data> dataList_Copy = new List<Data>(dataList_);
                foreach (Data data in dataList_Copy)
                {
                    if (data.Time < DrawTime - DisappSpan)//描画終了
                        dataList_.RemoveAt(0);
                    else if (data.Time < DrawTime + DrawSpan)//描画
                        dataList_Draw.Add(data);
                    else//未描画
                        break;
                }
                Bitmap bitmap = (Bitmap)BaseMap.Clone();
                Graphics g = Graphics.FromImage(bitmap);
                string Text = "";
                foreach (Data data in dataList_Draw)
                {
                    //int size = (int)(data.Mag * data.Mag * bitmap.Height / 1080);
                    int size = (int)(data.Mag * bitmap.Height / 216);
                    int alpha = a;
                    if (data.Time < DrawTime)//描画時間より前
                        alpha = (int)((1.0 - (DrawTime - data.Time).TotalSeconds / DisappSpan.TotalSeconds) * a);//消える時間の割合*基本透明度
                    g.FillEllipse(Depth2Color(data.Depth, alpha), (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                    g.DrawEllipse(Pens.Gray, (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                    if(!(data.MaxInt=="震度１"|| data.MaxInt == "震度２"))
                    Text += $" {data.HypoName.PadRight(10, '　')}　最大{data.MaxInt.PadRight(4, '　')}　M{data.Mag:F1}　{data.Depth:d3}km\n";
                }
                g.FillRectangle(new SolidBrush(Color.FromArgb(30, 60, 90)), MapWidth, 0, bitmap.Width - MapWidth, bitmap.Height);
                g.DrawString(Text, new Font("Koruri Regular", bitmap.Height / 36, GraphicsUnit.Pixel), Brushes.White, MapWidth, 0);
                g.DrawString(DrawTime.ToString("yyyy/MM/dd HH:mm:ss"), new Font("Koruri Regular", (int)(bitmap.Height / 13.5), GraphicsUnit.Pixel), Brushes.White, MapWidth, MapHeight - (int)(bitmap.Height / 10.8));
                bitmap.Save($"output\\{SaveDire}\\{i:d4}.png", ImageFormat.Png);
                g.Dispose();
                bitmap.Dispose();
                Console.WriteLine($"{DrawTime:yyyy/MM/dd HH:mm:ss} {i:d4}.png : {dataList_Draw.Count}");
                DrawTime += DrawSpan;
            }
            Console.WriteLine("画像作成終了");
            Console.WriteLine($"動画化: ffmpeg -framerate 30 -i %04d.png -vcodec libx264 -pix_fmt yuv420p -r 30 _output.mp4");
        }
        public static Color HSL2RGB(double h, double s, double l, int alpha = 255)
        {
            double r, g, b;
            if (s == 0)
                r = g = b = l;
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = Hue2RGB(p, q, h + 1.0 / 3.0);
                g = Hue2RGB(p, q, h);
                b = Hue2RGB(p, q, h - 1.0 / 3.0);
            }
            return Color.FromArgb(alpha, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private static double Hue2RGB(double p, double q, double t)
        {
            if (t < 0)
                t += 1;
            else if (t > 1)
                t -= 1;
            if (t < 1.0 / 6.0)
                return p + (q - p) * 6 * t;
            else if (t < 1.0 / 2.0)
                return q;
            else if (t < 2.0 / 3.0)
                return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
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
