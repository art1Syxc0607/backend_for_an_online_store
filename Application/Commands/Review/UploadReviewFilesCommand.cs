using Application.Common.Caching;
using Application.DTOs.File;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Commands.Review;

public class UploadReviewFilesCommand : IRequest<List<FileUploadResponseDto>>, ICacheInvalidatingCommand
{
    public int ReviewId { get; init; }
    public int UserId { get; init; }  // ✅ добавили для проверки владельца
    public List<FileUploadDto> Files { get; init; } = new();


    private readonly CacheInvalidationContext _cacheContext = new();

    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}
