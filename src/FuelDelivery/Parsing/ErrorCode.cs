namespace FuelDelivery.Parsing;

public enum ErrorCode
{
    MissingField,
    UnknownType,
    UnknownClient,
    InvalidDate,
    InvalidValue,
    DuplicateId
}

public static class ErrorCodeExtensions
{
    public static string ToCode(this ErrorCode code) => code switch
    {
        ErrorCode.MissingField => "MISSING_FIELD",
        ErrorCode.UnknownType => "UNKNOWN_TYPE",
        ErrorCode.UnknownClient => "UNKNOWN_CLIENT",
        ErrorCode.InvalidDate => "INVALID_DATE",
        ErrorCode.InvalidValue => "INVALID_VALUE",
        ErrorCode.DuplicateId => "DUPLICATE_ID",
        _ => throw new ArgumentOutOfRangeException(nameof(code))
    };
}
