using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace LoyaltyServiceSqlGenerator;

public static class Parser
{
    public static List<Mcc> Parse(string filePath, string? sheetName =  null)
    {
        var entries = new List<Mcc>();

        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart!;
        var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();
        if (sheets == null)
            throw new InvalidOperationException("Файл не содержит листов");
        
        var sheet = sheets.FirstOrDefault(s => s.Name == sheetName) ?? sheets.FirstOrDefault();;
        if (sheet == null)
            throw new ArgumentException($"Лист с именем '{sheetName}' не найден.");
        
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
        var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

        foreach (var row in sheetData.Elements<Row>().Skip(1))
        {
            var cells = row.Elements<Cell>().ToList();
            if (cells.Count < 3) continue;

            string GetCellValue(Cell cell)
            {
                if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                {
                    int index = int.Parse(cell.InnerText);
                    return sharedStringTable?.ElementAt(index)?.InnerText ?? "";
                }
                return cell.InnerText;
            }

            var entry = new Mcc
            {
                Code = GetCellValue(cells[0]),
                Name = GetCellValue(cells[1]),
                Category = GetCellValue(cells[2])
            };

            entries.Add(entry);
        }

        return entries;
    }
}