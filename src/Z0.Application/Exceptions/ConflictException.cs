namespace Z0.Application.Exceptions;

public class ConflictException : Exception
{
    public string EntityName { get; }
    public string PropertyName { get; }
    public object Value { get; }

    public ConflictException(string entityName, string propertyName, object value)
        : base($"Entity '{entityName}' with {propertyName} '{value}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
        Value = value;
    }

    public ConflictException(string message) : base(message)
    {
        EntityName = string.Empty;
        PropertyName = string.Empty;
        Value = string.Empty;
    }
}
