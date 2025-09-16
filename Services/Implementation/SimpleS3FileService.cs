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

                // Validate file size (max 5MB for logos)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    return FileUploadResponse.CreateError("File size exceeds 5MB limit");
                }

                // Validate file type for logos
                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                {
                    return FileUploadResponse.CreateError("Invalid file type. Only JPEG, PNG, GIF, and WebP are allowed");
                }

                var bucketName = _configuration["AWS:S3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    return FileUploadResponse.CreateError("S3 bucket configuration is missing");
                }

                // Generate unique file name
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var s3Key = $"{folder}/{uniqueFileName}";

                // Create the upload request
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead, // Make the file publicly readable
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                    Metadata = 
                    {
                        ["original-filename"] = file.FileName,
                        ["upload-date"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    }
                };

                // Upload to S3
                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Construct the public URL
                    var region = _configuration["AWS:Region"] ?? "us-east-1";
                    var fileUrl = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";
                    
                    _logger.LogInformation("Successfully uploaded file {FileName} to S3: {S3Key}", file.FileName, s3Key);
                    
                    return FileUploadResponse.CreateSuccess(
                        fileUrl, 
                        s3Key, 
                        file.FileName, 
                        file.Length, 
                        file.ContentType);
                }
                else
                {
                    _logger.LogError("S3 upload failed with status code: {StatusCode}", response.HttpStatusCode);
                    return FileUploadResponse.CreateError($"S3 upload failed with status: {response.HttpStatusCode}");
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3 error uploading file {FileName}: {ErrorCode} - {Message}", 
                    file?.FileName, s3Ex.ErrorCode, s3Ex.Message);
                return FileUploadResponse.CreateError($"S3 error: {s3Ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file?.FileName);
                return FileUploadResponse.CreateError($"Upload failed: {ex.Message}");
            }
        }

        public async Task<FileUploadResponse> UploadPrivateFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return FileUploadResponse.CreateError("File is empty or null");
                }

                var bucketName = _configuration["AWS:S3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    return FileUploadResponse.CreateError("S3 bucket configuration is missing");
                }

                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var s3Key = $"{folder}/{uniqueFileName}";

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.Private,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                    Metadata =
                    {
                        ["original-filename"] = file.FileName,
                        ["upload-date"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                    }
                };

                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Successfully uploaded private file {FileName} to S3: {S3Key}", file.FileName, s3Key);
                    return FileUploadResponse.CreateSuccess(string.Empty, s3Key, file.FileName, file.Length, file.ContentType ?? "application/octet-stream");
                }

                return FileUploadResponse.CreateError($"S3 upload failed with status: {response.HttpStatusCode}");
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3 error uploading private file {FileName}: {ErrorCode} - {Message}", file?.FileName, s3Ex.ErrorCode, s3Ex.Message);
                return FileUploadResponse.CreateError($"S3 error: {s3Ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading private file {FileName}", file?.FileName);
                return FileUploadResponse.CreateError($"Upload failed: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string s3Key)
        {
            try
            {
                if (string.IsNullOrEmpty(s3Key))
                {
                    _logger.LogWarning("Attempted to delete empty S3 key");
                    return false;
                }

                var bucketName = _configuration["AWS:S3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    _logger.LogError("S3 bucket configuration is missing");
                    return false;
                }

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = s3Key
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);

                _logger.LogInformation("Successfully deleted file from S3: {S3Key}", s3Key);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3 error deleting file {S3Key}: {ErrorCode} - {Message}", 
                    s3Key, s3Ex.ErrorCode, s3Ex.Message);
                return false;
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
                if (string.IsNullOrEmpty(s3Key))
                {
                    _logger.LogWarning("Attempted to generate presigned URL for empty S3 key");
                    return string.Empty;
                }

                var bucketName = _configuration["AWS:S3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                {
                    _logger.LogError("S3 bucket configuration is missing");
                    return string.Empty;
                }

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = s3Key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.Add(expiration)
                };

                var presignedUrl = await _s3Client.GetPreSignedURLAsync(request);
                
                _logger.LogDebug("Generated presigned URL for {S3Key} with expiration {Expiration}", 
                    s3Key, expiration);
                
                return presignedUrl;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "S3 error generating presigned URL for {S3Key}: {ErrorCode} - {Message}", 
                    s3Key, s3Ex.ErrorCode, s3Ex.Message);
                return string.Empty;
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

        public string ExtractS3KeyFromUrl(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return string.Empty;

                var bucketName = _configuration["AWS:S3:BucketName"];
                if (string.IsNullOrEmpty(bucketName))
                    return string.Empty;

                // Handle different URL formats:
                // https://bucket-name.s3.region.amazonaws.com/folder/file.ext
                // https://s3.region.amazonaws.com/bucket-name/folder/file.ext

                var uri = new Uri(fileUrl);
                
                if (uri.Host.StartsWith($"{bucketName}.s3."))
                {
                    // bucket-name.s3.region.amazonaws.com format
                    return uri.AbsolutePath.TrimStart('/');
                }
                else if (uri.Host.StartsWith("s3.") && uri.AbsolutePath.StartsWith($"/{bucketName}/"))
                {
                    // s3.region.amazonaws.com/bucket-name format
                    return uri.AbsolutePath.Substring($"/{bucketName}/".Length);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting S3 key from URL: {FileUrl}", fileUrl);
                return string.Empty;
            }
        }
    }
}