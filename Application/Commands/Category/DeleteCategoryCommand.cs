using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class DeleteCategoryCommand : IRequest, ICacheInvalidatingCommand
{
    public int CategoryId { get; set; }

    // ✅ Инвалидируем кэш 
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.CategoriesPrefix
    };
}
