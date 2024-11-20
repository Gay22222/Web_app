// KHOITD-EV SECTION
// ** BEGIN **
using BlueSports.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlueSports.Services
{
    public class MoMoPaymentService
    {
        private readonly HttpClient _client;
        private readonly string _accessKey = "F8BBA842ECF85";
        private readonly string _secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";

        public MoMoPaymentService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Dictionary<string, object>> CreatePaymentRequest(MomoPaymentRequest request)
        {
            Guid myuuid = Guid.NewGuid();
            request.OrderId = myuuid.ToString(); // TODO: Change with Order ID
            request.RequestId = myuuid.ToString();
            request.RequestType = "payWithATM";
            request.PartnerCode = "MOMO";
            request.PartnerName = "MoMo Payment";
            request.StoreId = "Test Store";
            request.AutoCapture = true;
            request.Lang = "vi";

            // Create Signature
            var rawSignature = $"accessKey={_accessKey}&amount={request.Amount}&extraData={request.ExtraData}&ipnUrl={request.IpnUrl}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&partnerCode={request.PartnerCode}&redirectUrl={request.RedirectUrl}&requestId={request.RequestId}&requestType={request.RequestType}";
            request.Signature = GetSignature(rawSignature, _secretKey);

            StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", httpContent);
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
        }

        private string GetSignature(string text, string key)
        {
            var encoding = new UTF8Encoding();
            byte[] textBytes = encoding.GetBytes(text);
            byte[] keyBytes = encoding.GetBytes(key);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(textBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
// ** END **