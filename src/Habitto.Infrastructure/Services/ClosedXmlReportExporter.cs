using Habitto.Application.Reports;
using ClosedXML.Excel;

namespace Habitto.Infrastructure.Services;

public interface IExcelReportExporter
{
    byte[] Export(IReadOnlyList<BookingReportRow> rows);
}

public class ClosedXmlReportExporter : IExcelReportExporter
{
    public byte[] Export(IReadOnlyList<BookingReportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Reservas");

        var headers = new[] { "Check-In", "Check-Out", "Precio pagado", "Huésped", "Email huésped", "Inmueble" };
        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        sheet.Row(1).Style.Font.Bold = true;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var excelRow = i + 2;
            sheet.Cell(excelRow, 1).Value = row.CheckIn.ToString("yyyy-MM-dd");
            sheet.Cell(excelRow, 2).Value = row.CheckOut.ToString("yyyy-MM-dd");
            sheet.Cell(excelRow, 3).Value = row.PricePaid;
            sheet.Cell(excelRow, 4).Value = row.GuestFullName;
            sheet.Cell(excelRow, 5).Value = row.GuestEmail;
            sheet.Cell(excelRow, 6).Value = row.PropertyTitle;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
