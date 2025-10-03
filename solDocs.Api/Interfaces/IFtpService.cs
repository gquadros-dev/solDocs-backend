namespace solDocs.Interfaces
{
    public interface IFtpService
    {
        Task<bool> UploadFileAsync(Stream stream, string uniqueFileName);
    }    
}

