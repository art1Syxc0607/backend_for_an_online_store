using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Category;

public class DeleteCategoryCommand : IRequest
{
    public int CategoryId { get; set; }
}
