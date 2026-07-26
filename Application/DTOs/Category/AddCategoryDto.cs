using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Category;

public record AddCategoryDto(string Name, string? Description,
     DateTime CreatedAt);


