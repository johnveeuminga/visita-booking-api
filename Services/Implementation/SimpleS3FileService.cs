using Amazon.S3;
using Amazon.S3.Model;
using visita_booking_api.Services.Interfaces;
using visita_booking_api.Models.DTOs;

namespace visita_booking_api.Services.Implementation
{
    public class SimpleS3FileService : IS3FileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SimpleS3FileService> _logger;

        public SimpleS3FileService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<SimpleS3FileService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return FileUploadResponse.CreateError("File is empty or null");
                }

                // For now, return a mock response until S3 is configured
                var mockUrl = $"https://mock-s3-bucket.s3.amazonaws.com/{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                
                _logger.LogInformation("Mock file upload: {FileName} -> {MockUrl}", file.FileName, mockUrl);
                
                return FileUploadResponse.CreateSuccess(
                    mockUrl, 
                    $"{folder}/{file.FileName}", 
                    file.FileName, 
                    file.Length, 
                    file.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file?.FileName);
                return FileUploadResponse.CreateError($"Upload failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string s3Key)
        {
            try
            {
                _logger.LogInformation("Mock file deletion: {S3Key}", s3Key);
                // Mock successful deletion
                await Task.Delay(10); // Simulate async operation
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {S3Key}", s3Key);
                return false;
            }
        }

        public async Task<string> GetPresignedUrlAsync(string s3Key, TimeSpan expiration)
        {
            try
            {
                // Return mock presigned URL
                await Task.Delay(10); // Simulate async operation
                var mockUrl = $"https://mock-s3-bucket.s3.amazonaws.com/{s3Key}?expires={DateTimeOffset.UtcNow.Add(expiration).ToUnixTimeSeconds()}";
                _logger.LogDebug("Generated mock presigned URL for {S3Key}", s3Key);
                return mockUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL for {S3Key}", s3Key);
                return string.Empty;
            }
        }

        public async Task<List<FileUploadResponse>> UploadMultipleFilesAsync(List<IFormFile> files, string folder)
        {
            var responses = new List<FileUploadResponse>();

            foreach (var file in files)
            {
                var response = await UploadFileAsync(file, folder);
                responses.Add(response);
            }

            return responses;
        }
    }
}