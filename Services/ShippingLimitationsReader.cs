using System.Collections.Generic;
using ClosedXML.Excel;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Reads the optional "Shipping Limitations" worksheet and attaches rows by Molport ID.
    internal static class ShippingLimitationsReader
    {
        private const string SheetName = "Shipping Limitations";

        public static List<QuoteShippingInfo> Read(XLWorkbook workbook)
        {
            var shippingLimitations = new List<QuoteShippingInfo>();

            // Older or simplified quote files may not include the second worksheet.
            if (!workbook.Worksheets.TryGetWorksheet(SheetName, out var worksheet))
            {
                return shippingLimitations;
            }

            var headerRow = ExcelHeaderReader.FindHeaderRow(worksheet, 1, KnownShippingHeaderLabels());
            var firstDataRow = headerRow + 1;
            var columns = BuildColumnMap(worksheet, headerRow);
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? firstDataRow;

            for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                var lineNumber = ExcelCellReader.ReadText(worksheet, rowNumber, columns.LineNumber);

                if (!ExcelCellReader.IsProductRow(lineNumber))
                {
                    break;
                }

                shippingLimitations.Add(new QuoteShippingInfo
                {
                    LineNumber = lineNumber,
                    MolportId = ExcelCellReader.ReadText(worksheet, rowNumber, columns.MolportId),
                    Supplier = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Supplier),
                    CatalogueNumber = ExcelCellReader.ReadText(worksheet, rowNumber, columns.CatalogueNumber),
                    CountryOfOrigin = ExcelCellReader.ReadText(worksheet, rowNumber, columns.CountryOfOrigin),
                    Unit = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Unit),
                    UnNumber = ExcelCellReader.ReadText(worksheet, rowNumber, columns.UnNumber),
                    HazardousClass = ExcelCellReader.ReadText(worksheet, rowNumber, columns.HazardousClass),
                    PackingGroup = ExcelCellReader.ReadText(worksheet, rowNumber, columns.PackingGroup),
                    ShippingLimitations = ExcelCellReader.ReadText(worksheet, rowNumber, columns.ShippingLimitations),
                    CompoundState = ExcelCellReader.ReadText(worksheet, rowNumber, columns.CompoundState),
                    Solubility = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Solubility)
                });
            }

            return shippingLimitations;
        }

        public static void AttachToItems(
            List<QuoteItem> items,
            List<QuoteShippingInfo> shippingLimitations)
        {
            // Keep the DataGrid source as QuoteItem while still exposing second-sheet details per row.
            foreach (var item in items)
            {
                item.ShippingInfo = FindMatchingShippingInfo(item, shippingLimitations);
            }
        }

        private static QuoteShippingInfo? FindMatchingShippingInfo(
            QuoteItem item,
            IEnumerable<QuoteShippingInfo> shippingLimitations)
        {
            foreach (var shippingInfo in shippingLimitations)
            {
                // Molport ID is repeated on both worksheets and is the stable product key.
                if (ExcelCellReader.TextEquals(item.MolportId, shippingInfo.MolportId))
                {
                    return shippingInfo;
                }
            }

            return null;
        }

        private static ShippingColumnMap BuildColumnMap(IXLWorksheet worksheet, int headerRow)
        {
            // These defaults match the observed "Shipping Limitations" worksheet layout.
            return new ShippingColumnMap
            {
                LineNumber = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 1, "#", "Line", "Line number"),
                MolportId = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 2, "Molport Id", "Molport ID", "MolportId"),
                Supplier = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 3, "Supplier"),
                CatalogueNumber = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    4,
                    "Catalogue number",
                    "Catalog number",
                    "Catalogue no",
                    "Catalog no"),
                CountryOfOrigin = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 5, "Country of Origin", "Origin country"),
                Unit = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 6, "Unit"),
                UnNumber = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 7, "UN number", "UN no", "UN"),
                HazardousClass = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 8, "Hazardous class", "Hazard class"),
                PackingGroup = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 9, "Packing group"),
                ShippingLimitations = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    10,
                    "Shipping limitations",
                    "Dangerous goods"),
                CompoundState = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 11, "Compound state", "State"),
                Solubility = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, 12, "Solubility")
            };
        }

        private static IEnumerable<string> KnownShippingHeaderLabels()
        {
            return new[]
            {
                "#",
                "Molport Id",
                "Supplier",
                "Catalogue number",
                "Country of Origin",
                "Unit",
                "UN number",
                "Hazardous class",
                "Packing group",
                "Shipping limitations",
                "Compound state",
                "Solubility"
            };
        }

        private class ShippingColumnMap
        {
            public int LineNumber { get; set; }
            public int MolportId { get; set; }
            public int Supplier { get; set; }
            public int CatalogueNumber { get; set; }
            public int CountryOfOrigin { get; set; }
            public int Unit { get; set; }
            public int UnNumber { get; set; }
            public int HazardousClass { get; set; }
            public int PackingGroup { get; set; }
            public int ShippingLimitations { get; set; }
            public int CompoundState { get; set; }
            public int Solubility { get; set; }
        }
    }
}
