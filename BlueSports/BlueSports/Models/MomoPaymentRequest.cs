// NHAN NGUYEN SECTION
// ** BEGIN **
using System.Text.Json.Serialization;

namespace BlueSports.Models
{
    public class MomoPaymentRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; }

        [JsonPropertyName("partnerName")]
        public string PartnerName { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; }

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonPropertyName("ipnUrl")]
        public string IpnUrl { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = "vi";

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; }

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = "";

        [JsonPropertyName("orderGroupId")]
        public string OrderGroupId { get; set; } = "";

        [JsonPropertyName("autoCapture")]
        public bool AutoCapture { get; set; } = true;

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

    }
}

// ** END **
