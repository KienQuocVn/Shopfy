using Newtonsoft.Json;

namespace Shofy.Services.MoMo
{
    public class MoMoPaymentRequest
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonProperty("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonProperty("ipnUrl")]
        public string IpnUrl { get; set; } = string.Empty;

        [JsonProperty("requestType")]
        public string RequestType { get; set; } = "captureWallet";

        [JsonProperty("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonProperty("lang")]
        public string Lang { get; set; } = "vi";

        [JsonProperty("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    public class MoMoPaymentResponse
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonProperty("payUrl")]
        public string PayUrl { get; set; } = string.Empty;

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }
}