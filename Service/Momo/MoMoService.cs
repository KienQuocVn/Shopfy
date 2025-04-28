using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using Shofy.Models;
using Microsoft.Extensions.Logging;

namespace Shofy.Services.MoMo
{
    public class MoMoService
    {
        private readonly MoMoConfig _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(IOptions<MoMoConfig> config, HttpClient httpClient, ILogger<MoMoService> logger)
        {
            _config = config.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MoMoPaymentResponse> CreatePaymentAsync(Order order)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var orderId = $"SHOFY-{order.OrderID}-{DateTime.Now.Ticks}";
                var orderInfo = $"Payment for order #{order.OrderID}";
                var amount = (long)(order.TotalPrice * 23000); // Convert to VND

                var request = new MoMoPaymentRequest
                {
                    PartnerCode = _config.PartnerCode,
                    RequestId = requestId,
                    Amount = amount,
                    OrderId = orderId,
                    OrderInfo = orderInfo,
                    RedirectUrl = "https://localhost:7234/Client/Payment?handler=Return",
                    IpnUrl = "https://localhost:7234/Client/Payment?handler=Notify",
                    ExtraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"orderId={order.OrderID}")),
                    RequestType = "captureWallet",
                    Lang = "vi"
                };

                // Generate signature
                var rawSignature =
                    $"accessKey={_config.AccessKey}" +
                    $"&amount={request.Amount}" +
                    $"&extraData={request.ExtraData}" +
                    $"&ipnUrl={request.IpnUrl}" +
                    $"&orderId={request.OrderId}" +
                    $"&orderInfo={request.OrderInfo}" +
                    $"&partnerCode={request.PartnerCode}" +
                    $"&redirectUrl={request.RedirectUrl}" +
                    $"&requestId={request.RequestId}" +
                    $"&requestType={request.RequestType}";

                _logger.LogInformation("Raw signature string: {RawSignature}", rawSignature);

                request.Signature = GenerateSignature(rawSignature, _config.SecretKey);

                var jsonRequest = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Request to MoMo: {Request}", jsonRequest);

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_config.PaymentUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo response: {Response}", responseContent);

                var paymentResponse = JsonConvert.DeserializeObject<MoMoPaymentResponse>(responseContent);
                return paymentResponse ?? new MoMoPaymentResponse
                {
                    ErrorCode = -1,
                    Message = "Invalid response from MoMo"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment for order {OrderId}", order.OrderID);
                return new MoMoPaymentResponse
                {
                    ErrorCode = -1,
                    Message = "Internal error occurred"
                };
            }
        }

        private string GenerateSignature(string rawData, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}