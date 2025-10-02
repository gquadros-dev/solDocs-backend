using FluentFTP;
using solDocs.Interfaces;
using FluentFTP.Helpers;

namespace solDocs.Services
{
    public class FtpService : IFtpService
    {
        private readonly IConfiguration _config;

        public FtpService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> UploadFileAsync(Stream stream, string uniqueFileName)
        {
            var host = _config["FtpSettings:Host"];
            var user = _config["FtpSettings:Username"];
            var pass = _config["FtpSettings:Password"];
            var remotePath = $"{_config["FtpSettings:UploadPath"]}/{uniqueFileName}";

            using var client = new AsyncFtpClient(host, user, pass);
            await client.Connect();
        
            var status = await client.UploadStream(stream, remotePath, FtpRemoteExists.Overwrite);

            await client.Disconnect();
            return status.IsSuccess();
        }
    }
}