using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class UpdateCategoryCommand : IRequest, ICacheInvalidatingCommand
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }

    // ✅ Инвалидируем кэш 
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.CategoriesPrefix
    };
}
