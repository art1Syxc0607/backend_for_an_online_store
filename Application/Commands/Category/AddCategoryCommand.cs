using Application.DTOs.Category;
using MediatR;
using Application.Interfaces.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class AddCategoryCommand : IRequest<int>, ICacheInvalidatingCommand
{
    public string Name { get;  set; }
    public string? Description { get;  set; }

    // ✅ Инвалидируем кэш 
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.CategoriesPrefix
    };
}
