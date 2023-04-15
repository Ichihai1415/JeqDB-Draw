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
        Bitmap canvas = new Bitmap(1000, 1000);
        List<Data> dataList = new List<Data>();
        double LatEnd = 48;
        double LonSta = 125;
        double ZoomW = 40;
        double ZoomH = 40;

        private void Form1_Load(object sender, EventArgs e)
        {
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

            Bitmap bitmap = canvas;
            Graphics g = Graphics.FromImage(bitmap);
            foreach (Data data in dataList)
            {
                int size = (int)(data.Mag * data.Mag);//(int)(data.Mag * 8);
                if (data.Mag < 7)
                    continue;
                g.FillEllipse(Depth2Color(data.Depth), (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);
                g.DrawEllipse(Pens.Black, (int)((data.Lon - LonSta) * ZoomW) - size / 2, (int)((LatEnd - data.Lat) * ZoomH) - size / 2, size, size);



            }
            MapImg.BackgroundImage = bitmap;
            g.Dispose();
            Console.WriteLine("情報描画終了");
            bitmap.Save("output.png",ImageFormat.Png);

        }
        private SolidBrush Depth2Color(int Depth)
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
            return new SolidBrush(Color.FromArgb(30, r, g, b));
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
            JObject json = JObject.Parse(File.ReadAllText("Map-jp.geojson"));
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.FromArgb(120, 120, 255));
            GraphicsPath Maps = new GraphicsPath();
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
            g.FillPath(new SolidBrush(Color.FromArgb(150, 150, 150)), Maps);
            g.DrawPath(new Pen(Color.FromArgb(255, 255, 255), 1), Maps);
            g.Dispose();
            MapImg.BackgroundImage = canvas;
        }

        private void MapImg_Click(object sender, EventArgs e)
        {
            //RC_Draw_Click(sender,e);
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
