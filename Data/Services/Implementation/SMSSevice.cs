using Core.Models;
using Data.Services.Interface;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Data.Services.Implementation
{
    public class SMSSevice : ISMSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _senderId;
        public SMSSevice(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient("Dojah");
            _senderId = Environment.GetEnvironmentVariable("DOJAH_SENDER_ID") ?? "";

        }

        public async Task<BaseResponse> SendSmS(IEnumerable<long> destinations, string message)
        {
            using StringContent JsonContent = new(
                JsonSerializer.Serialize(new
                {
                    to = string.Join(",", destinations),
                    message = message,
                    channel = "sms",
                    sender_id = _senderId,
                    priority = 0
                }),
            Encoding.UTF8,
            "application/json"

                );


            //var requestBody = new
            //{
            //    to = string.Join(",", destinations), // Join the destinations into a comma-separated string
            //    message = message,
            //    from = _senderId,
            //    priority = priority ? "priority" : "normal"
            //};

            var httpResponse = await _httpClient.PostAsync("api/v1/messaging/sms", JsonContent);
            if (httpResponse.IsSuccessStatusCode)
                return new BaseResponse { Status = true, Message = "Message Sent Successfully" };
            else
                return new BaseResponse { Status = false, Message = "Failed to send message" };
        }
    }
}
