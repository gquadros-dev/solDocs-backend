using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using solDocs.Interfaces;
using System.Security.Cryptography;
using solDocs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace solDocs.Controllers
{
    [ApiController]
    [Route("api/{tenantSlug}/files")]
    [Authorize(Roles = "admin")]
    public class FilesController : BaseTenantController
    {
        private readonly IFtpService _ftpService;
        private readonly IConfiguration _config;
        private readonly IMediaAssetService _mediaAssetService;

        public FilesController(IFtpService ftpService, IConfiguration config, IMediaAssetService mediaAssetService, ITenantService tenantService) : base(tenantService)
        {
            _ftpService = ftpService;
            _config = config;
            _mediaAssetService = mediaAssetService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (TenantId == null) return Unauthorized();
            
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo enviado." });

            try
            {
                var tenant = await GetTenantAsync();
                if (tenant == null) return Unauthorized();
                
                using var imageStream = new MemoryStream();
                await file.CopyToAsync(imageStream);
                imageStream.Position = 0;

                using var image = await Image.LoadAsync(imageStream);

                if (image.Width > 1920 || image.Height > 1920)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(1920, 1920),
                        Mode = ResizeMode.Max 
                    }));
                }

                using var compressedStream = new MemoryStream();
                await image.SaveAsync(compressedStream, new WebpEncoder { Quality = 90 });
                compressedStream.Position = 0;

                var hash = ComputeSha256Hash(compressedStream);

                var existingAsset = await _mediaAssetService.FindByHashAsync(hash, TenantId);
                if (existingAsset != null)
                {
                    return Ok(new { url = existingAsset.PublicUrl });
                }

                var newFileName = $"{TenantId}-{hash}.webp";

                var uploadSuccess = await _ftpService.UploadFileAsync(compressedStream, newFileName);
                if (!uploadSuccess)
                {
                    return StatusCode(500, new { message = "Falha ao fazer upload." });
                }

                var publicUrl = $"{_config["FtpSettings:PublicBaseUrl"]}/{newFileName}";
                var newAsset = new MediaAssetModel
                {
                    FileHash = hash,
                    OriginalFileName = file.FileName,
                    FileSize = compressedStream.Length,
                    MimeType = "image/webp",
                    PublicUrl = publicUrl,
                    TenantId = TenantId
                };
                await _mediaAssetService.CreateAsync(newAsset);

                return Ok(new { url = publicUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno: {ex.Message}" });
            }
        }

        private static string ComputeSha256Hash(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(stream);
                stream.Position = 0;
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}