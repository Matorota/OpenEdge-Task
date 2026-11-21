using FuelDelivery.Tasks.Parsing;

namespace FuelDelivery.Tasks.Output;

// errors.csv: one row per rejected record with its error code.
public class ErrorReport(CsvWriter csv)
{
    public void Write(IEnumerable<Rejection> rejections) =>
        csv.Write("errors.csv", "ShipmentID,ErrorCode",
            rejections.Select(r => csv.Row(r.ShipmentId, ToCode(r.Code))));

    private string ToCode(ErrorCode code) => code switch
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
