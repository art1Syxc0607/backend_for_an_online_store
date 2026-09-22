// Domain/Enums/ReviewSortBy.cs
namespace Domain.Enums;

public enum ReviewSortBy
{
    DateOfCreation = 0,
    Rating = 1,
    Likes = 2  // опционально, если будете добавлять лайки
}