using CompareHWP.Services;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core.FilteringUI;
using JVM.ViewCommon.WPF.View.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CompareHWP.ViewModel
{
    public class CheckDistanceVM : ViewModelBase
    {
        private readonly BusyService _busyService = BusyService.Instance;
        private MainWindow main = Application.Current.MainWindow as MainWindow;

        /// <summary>
        /// 카카오 REST API 키
        /// </summary>
        private const string KAKAO_API_KEY = "cac50da25a646f46f7f9f0c23aa53bdd";

        private string _addressA;
        /// <summary>
        /// 첫번째 주소
        /// </summary>
        public string AddressA
        {
            get => _addressA;
            set => SetProperty(ref _addressA, value, nameof(AddressA));
        }

        private string _addressB;
        /// <summary>
        /// 두번째 주소
        /// </summary>
        public string AddressB
        {
            get => _addressB;
            set => SetProperty(ref _addressB, value, nameof(AddressB));
        }

        private string _distanceKmText;
        public string DistanceKmText
        {
            get => _distanceKmText;
            set => SetProperty(ref _distanceKmText, value, nameof(DistanceKmText));
        }

        private string _resultText;
        public string ResultText
        {
            get => _resultText;
            set => SetProperty(ref _resultText, value, nameof(ResultText));
        }

        public ICommand CalculateCommand { get; }
        public ICommand ClearCommand { get; }

        /// <summary>
        /// 생성자
        /// </summary>
        public CheckDistanceVM()
        {
            CalculateCommand = new RelayCommand(async _ => await CalculateClick());
            ClearCommand = new RelayCommand(ClearClick);
        }

        private async Task CalculateClick()
        {
            if (string.IsNullOrEmpty(AddressA) || string.IsNullOrEmpty(AddressB))
            {
                main.ShowAlertControl("주소를 모두 입력해주세요.", "입력 오류", eDialogButtonType.Ok, null, 5000, null, true);
                return;
            }

            DistanceKmText = string.Empty;
            ResultText = string.Empty;

            _busyService.IsBusy = true;
            await CalculateDistance();
            _busyService.IsBusy = false;
        }

        private void ClearClick(object parameter)
        {
            AddressA = string.Empty;
            AddressB = string.Empty;
            DistanceKmText = string.Empty;
            ResultText = string.Empty;
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
                    client.DefaultRequestHeaders.Add("Authorization", $"KakaoAK {KAKAO_API_KEY}");

                    string url = $"https://dapi.kakao.com/v2/local/search/address.json?query={Uri.EscapeDataString(address)}";

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
                    main.ShowAlertControl(ex.ToString(), "좌표 변환 에러!!", eDialogButtonType.Ok, null, 5000, null, true);
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

        private async Task CalculateDistance()
        {
            var coord1 = await GetCoordinateAsync(AddressA);
            var coord2 = await GetCoordinateAsync(AddressB);

            if (coord1 == null || coord2 == null)
            {
                main.ShowAlertControl("주소를 위도/경도로 변환하는데 실패했습니다.", "좌표 변환 오류", eDialogButtonType.Ok, null, 5000, null, true);
                return;
            }

            double distanceMeter = GetDistanceMeter(
                coord1.Latitude, coord1.Longitude,
                coord2.Latitude, coord2.Longitude
            );

            double distanceKm = distanceMeter / 1000.0;
            DistanceKmText = $"📏 거리: {distanceKm:F2} km\n";

            if (distanceKm <= 1)
                ResultText = "✅ 반경 1km 이내\n";
            else
                ResultText = "❌ 반경 1km 초과\n";
        }
    }

    // ===============================
    // 좌표 모델
    // ===============================
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
