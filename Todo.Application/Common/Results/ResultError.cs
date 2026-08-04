using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace Todo.Application.Common.Results;

[JsonDerivedType(typeof(ResultError))]
[JsonDerivedType(typeof(ValidationResultError))]
public record ResultError
{
    public static ResultError NullData 
        => new(ResultErrorCodes.Null, "Data is null.", ResultErrorTypeEnum.Failure);

    public string Code { get; }

    public string Description { get; }

    public ResultErrorTypeEnum Type { get; }

    protected ResultError(string code, string description, ResultErrorTypeEnum errorType)
    {
        if (errorType is ResultErrorTypeEnum.Validation && this is not ValidationResultError)
        {
            throw new InvalidOperationException("Only validation errors can be of type Validation.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        Code = code;
        Description = description;
        Type = errorType;
    }

    public static ResultError Failure(string code, string description)
        => new(code, description, ResultErrorTypeEnum.Failure);

    public static ResultError Validation(IEnumerable<ValidationFailure> validationFailures)
        => new ValidationResultError([.. validationFailures.Select(x => new ValidationPropertyResultError(x.PropertyName, x.ErrorMessage))]);

    public static ResultError Validation(IEnumerable<ValidationPropertyResultError> validationFailures)
        => new ValidationResultError(validationFailures);

    public static ResultError Unauthorized(string code, string description)
        => new(code, description, ResultErrorTypeEnum.Unauthorized);

    public static ResultError Forbidden(string code, string description)
        => new(code, description, ResultErrorTypeEnum.Forbidden);

    public static ResultError NotFound(string code, string description)
        => new(code, description, ResultErrorTypeEnum.NotFound);

    public static ResultError Conflict(string code, string description)
        => new(code, description, ResultErrorTypeEnum.Conflict);

    public static ResultError Internal(string code, string description)
        => new(code, description, ResultErrorTypeEnum.Internal);

    public override string ToString()
    {
        return $"Code: {Code}, Description: {Description}, Type: {Type}";
    }
}

public sealed record ValidationResultError : ResultError
{
    [JsonPropertyOrder(1)] // Serialize base properties first
    public IEnumerable<ValidationPropertyResultError> ValidationErrors { get; }

    public ValidationResultError(IEnumerable<ValidationPropertyResultError> errors)
        : base(ResultErrorCodes.Validation, "One or more validation errors occurred.", ResultErrorTypeEnum.Validation)
    {
        ValidationErrors = errors ?? [];
    }
}

public sealed record ValidationPropertyResultError(string PropertyName, string ErrorMessage);
