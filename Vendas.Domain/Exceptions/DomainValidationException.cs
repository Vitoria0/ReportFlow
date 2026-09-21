namespace Vendas.Domain.Exceptions;

public sealed class DomainValidationException(string message) : Exception(message);
