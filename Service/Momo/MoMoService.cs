using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Shofy.Models;

namespace Shofy.Services.MoMo
{
    public class MoMoService
    {
        private readonly MoMoConfig _config;
        private readonly HttpClient _httpClient;

        public MoMoService(IOptions<MoMoConfig> config, HttpClient httpClient)
        {
            _config = config.Value;
            _httpClient = httpClient;
        }

        public async Task<MoMoPaymentResponse> CreatePaymentAsync(Order order)
        {
            var requestId = Guid.NewGuid().ToString();
            var orderId = $"SHOFY-{order.OrderID}-{DateTime.Now.Ticks}";
            var orderInfo = $"Payment for order #{order.OrderID}";
            var amount = (long)(order.TotalPrice * 1000); // Convert to VND (assuming TotalPrice is in USD)

            var request = new MoMoPaymentRequest
            {
                PartnerCode = _config.PartnerCode,
                RequestId = requestId,
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                RedirectUrl = _config.ReturnUrl,
                IpnUrl = _config.NotifyUrl,
                ExtraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"orderId={order.OrderID}")),
                Lang = "vi"
            };

            // Generate signature
            var rawData = $"accessKey={_config.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}&ipnUrl={request.IpnUrl}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&partnerCode={request.PartnerCode}&redirectUrl={request.RedirectUrl}&requestId={request.RequestId}&requestType={request.RequestType}";
            request.Signature = GenerateSignature(rawData, _config.SecretKey);

            // Send request to MoMo
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.PaymentUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var paymentResponse = JsonConvert.DeserializeObject<MoMoPaymentResponse>(responseContent);

            return paymentResponse;
        }

        private string GenerateSignature(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}