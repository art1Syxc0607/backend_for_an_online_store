namespace Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException() : base("Ресурс не найден") { }

    public NotFoundException(string message) : base(message) { }

    //public NotFoundException(string message, Exception innerException)
    //    : base(message, innerException) { }

    public NotFoundException(string entityName, int id)
     : base($"{entityName} with ID {id} not found", "NOT_FOUND", 404)
    { }
}
