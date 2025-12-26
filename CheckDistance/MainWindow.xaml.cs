using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CheckDistance
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        // 🔑 카카오 REST API 키
        private const string KAKAO_API_KEY = "cac50da25a646f46f7f9f0c23aa53bdd";

        public MainWindow()
        {
            InitializeComponent();
        }

        // ===============================
        // 주소 → 위도/경도 변환
        // ===============================
        async Task<Coordinate> GetCoordinateAsync(string address)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add(
                "Authorization", $"KakaoAK {KAKAO_API_KEY}");

                    string url =
                        $"https://dapi.kakao.com/v2/local/search/address.json?query={Uri.EscapeDataString(address)}";

                    var response = await client.GetStringAsync(url);
                    var json = JObject.Parse(response);

                    if (json["documents"]?.HasValues != true)
                        return null;

                    var doc = json["documents"][0];

                    return new Coordinate
                    {
                        Latitude = doc["y"].Value<double>(),
                        Longitude = doc["x"].Value<double>()
                    };
                }
                catch (Exception ex)
                {
                    TextBox_Result.Text += $"{ex}\n";
                    return null;
                }

            }
        }

        // ===============================
        // 거리 계산 (Haversine 공식)
        // ===============================
        double GetDistanceMeter(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371000; // 지구 반지름 (m)

            double dLat = ToRadian(lat2 - lat1);
            double dLon = ToRadian(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadian(lat1)) *
                Math.Cos(ToRadian(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        double ToRadian(double degree)
        {
            return degree * Math.PI / 180;
        }

        private async void Button_Calculate_Click(object sender, RoutedEventArgs e)
        {
            string address1 = TextBox_First.Text;
            string address2 = TextBox_Second.Text;

            var coord1 = await GetCoordinateAsync(address1);
            var coord2 = await GetCoordinateAsync(address2);

            if (coord1 == null || coord2 == null)
            {
                TextBox_Result.Text += "❌ 좌표 변환 실패\n";
                return;
            }

            double distanceMeter = GetDistanceMeter(
                coord1.Latitude, coord1.Longitude,
                coord2.Latitude, coord2.Longitude
            );

            TextBox_Result.Text += $"📏 거리: {distanceMeter:F2} m\n";

            if (distanceMeter <= 1000)
                TextBox_Result.Text += "✅ 반경 1km 이내\n";
            else
                TextBox_Result.Text += "❌ 반경 1km 초과\n";
        }
    }

    // ===============================
    // 좌표 모델
    // ===============================
    class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
