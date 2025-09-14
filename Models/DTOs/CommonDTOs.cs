namespace visita_booking_api.Models.DTOs
{
    // API Response Wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    // Paginated Response
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public static PaginatedResponse<T> Create(List<T> items, int totalCount, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return new PaginatedResponse<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }

    // Validation Error Response
    public class ValidationErrorResponse
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? AttemptedValue { get; set; }
    }

    // File Upload Response
    public class FileUploadResponse
    {
        public bool Success { get; set; }
        public string? FileUrl { get; set; }
        public string? S3Key { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public string? Error { get; set; }

        public static FileUploadResponse CreateSuccess(string fileUrl, string s3Key, string fileName, long fileSize, string contentType)
        {
            return new FileUploadResponse
            {
                Success = true,
                FileUrl = fileUrl,
                S3Key = s3Key,
                FileName = fileName,
                FileSize = fileSize,
                ContentType = contentType
            };
        }

        public static FileUploadResponse CreateError(string error)
        {
            return new FileUploadResponse
            {
                Success = false,
                Error = error
            };
        }
    }

    // Batch Operation Response
    public class BatchOperationResponse
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<object> SuccessfulItems { get; set; } = new();
        public bool AllSuccessful => FailureCount == 0;
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    }

    // Cache Statistics Response
    public class CacheStatisticsResponse
    {
        public int TotalKeys { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double HitRatio { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public Dictionary<string, int> KeysByPattern { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Health Check Response
    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}