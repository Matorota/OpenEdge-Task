using FuelDelivery.Parsing;

namespace FuelDelivery.Output;

public static class ErrorReport
{
    public static void Write(IEnumerable<Rejection> rejections) =>
        CsvFile.Write("errors.csv", "ShipmentID,ErrorCode",
            rejections.Select(r => CsvFile.Row(r.ShipmentId, r.Code.ToCode())));
}
