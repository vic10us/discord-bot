﻿using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace v10.Services.Images;

public class ImageApiService : IImageApiService
{
    private readonly HttpClient _client;

    public ImageApiService(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("vic10usApi");
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

    public async Task<Stream> CreateRankCard(RankCardRequest rankCardRequest)
    {
        var json = JsonConvert.SerializeObject(rankCardRequest);
        using var message = await _client.PostAsync("images/rankCard", new StringContent(json, Encoding.UTF8, "application/json"));
        //check that the response was successful
        message.EnsureSuccessStatusCode();
        var stream = await message.Content.ReadAsStreamAsync();
        var resultStream = new MemoryStream();
        await stream.CopyToAsync(resultStream);
        resultStream.Position = 0;
        return resultStream;
    }

    public Task<Stream> CreateQRCode()
    {
        var qrGenerator = new QRCodeGenerator();
        var wifiPayload = new PayloadGenerator.WiFi("MyWiFi-SSID", "MyWiFi-Pass", PayloadGenerator.WiFi.Authentication.WPA);
        var qrCodeData = qrGenerator.CreateQrCode(wifiPayload);
        // QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
        var qrCode = new BitmapByteQRCode(qrCodeData);
        var graphic = qrCode.GetGraphic(20);
        var bm = new Bitmap(new MemoryStream(graphic));
        _ = Graphics.FromImage(bm);
        var stream = new MemoryStream();
        bm.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        return Task.FromResult(stream as Stream);
    }
}
