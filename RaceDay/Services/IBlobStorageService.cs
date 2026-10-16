namespace RaceDay.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadProfileImageAsync(
            Stream imageStream,
            string fileName,
            string contentType);
    }
}