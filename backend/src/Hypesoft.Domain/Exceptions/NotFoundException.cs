namespace Hypesoft.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, string id)
        : base($"{entityName} with id '{id}' was not found.") { }
}
