using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace bot.Services.vic10usAPI
{
    public class Vic10UsApiService
    {
        private readonly HttpClient _client;

        public Vic10UsApiService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("vic10usapi");
        }
        
        public async Task<Stream> ConvertSvgImage(string svg)
        {
            using var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            var imageContent = new StringContent(svg);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType("rank.svg"));
            content.Add(imageContent, "image", "rank.svg");
            content.Add(new StringContent("144"), "dpi");
            content.Add(new StringContent("false"), "keep");
            using var message =
                await _client.PostAsync("images/conversions", content);
            var stream = await message.Content.ReadAsStreamAsync();
            var resultStream = new MemoryStream();
            await stream.CopyToAsync(resultStream);
            resultStream.Position = 0;
            return resultStream;
        }
    }
}