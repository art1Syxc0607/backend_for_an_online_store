using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Interfaces;

public interface IProductImageService
{
    Task<string> SaveImageAsync(int productId, IFormFile file);
}
