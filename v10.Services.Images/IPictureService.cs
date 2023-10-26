namespace v10.Services.Images;

public interface IPictureService
{
    Task<Stream> GetCatPictureAsync();
    Task<(string fileName, Stream fileStream)> GetPictureFromCategory(string category);
}