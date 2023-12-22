namespace v10.Services.Images;

public interface IImageApiService
{
    Task<Stream> ConvertSvgImage(string svg);
    Task<Stream> CreateQRCode();
    Task<Stream> CreateRankCard(RankCardRequest rankCardRequest);
}