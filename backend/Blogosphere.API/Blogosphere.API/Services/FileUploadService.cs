using Blogosphere.API.Models.Dtos;

namespace Blogosphere.API.Services;

public interface IFileUploadService
{
   Task<FileUploadDto> UploadFileAsync(IFormFile file);
   Task<List<FileUploadDto>> UploadMultipleFilesAsync(IFormFileCollection files);
}

public class FileUploadService : IFileUploadService
{
   private readonly IWebHostEnvironment _environment;
   private readonly IConfiguration _configuration;
   private const int MaxFileSizeInMB = 10;
   private const long MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024; // 10MB

   public FileUploadService(IWebHostEnvironment environment, IConfiguration configuration)
   {
      _environment = environment;
      _configuration = configuration;
   }

   public async Task<FileUploadDto> UploadFileAsync(IFormFile file)
   {
      string allowedExtensions = ".jpg,.jpeg,.png,.gif";

      if (file == null || file.Length == 0)
         throw new ArgumentException("No file was provided");

      ValidateFile(file, allowedExtensions);

      var fileName = GetUniqueFileName(file.FileName);

      if (string.IsNullOrEmpty(_environment.WebRootPath))
      {
         _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
      }

      var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

      // Create uploads directory if it doesn't exist
      if (!Directory.Exists(uploadsFolder))
         Directory.CreateDirectory(uploadsFolder);

      var filePath = Path.Combine(uploadsFolder, fileName);

      using (var stream = new FileStream(filePath, FileMode.Create))
      {
         await file.CopyToAsync(stream);
      }

      var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5200";
      var fileUrl = $"{baseUrl}/uploads/{fileName}";

      return new FileUploadDto(
          FileName: fileName,
          FileUrl: fileUrl,
          FileSize: file.Length,
          ContentType: file.ContentType
      );
   }

   public async Task<List<FileUploadDto>> UploadMultipleFilesAsync(IFormFileCollection files)
   {
      if (files == null || !files.Any())
         throw new ArgumentException("No files were provided");

      var uploadTasks = files.Select(file => UploadFileAsync(file));
      return await Task.WhenAll(uploadTasks).ContinueWith(t => t.Result.ToList());
   }

   private void ValidateFile(IFormFile file, string allowedExtensions)
   {
      // Check file size
      if (file.Length > MaxFileSizeInBytes)
         throw new ArgumentException($"File size exceeds the limit of {MaxFileSizeInMB}MB");

      // Check file extension
      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      var allowedExtensionsList = allowedExtensions.Split(',').Select(e => e.ToLowerInvariant());

      if (!allowedExtensionsList.Contains(extension))
         throw new ArgumentException($"File type {extension} is not allowed. Allowed types: {allowedExtensions}");
   }

   private string GetUniqueFileName(string fileName)
   {
      // Add timestamp to ensure uniqueness
      var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
      return $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
   }
}