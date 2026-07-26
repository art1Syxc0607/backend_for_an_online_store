using Application.DTOs.Category;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class AddCategoryCommand : IRequest<int>
{
    public string Name { get;  set; }
    public string? Description { get;  set; }
}
