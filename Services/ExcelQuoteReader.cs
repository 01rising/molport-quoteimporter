using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Reads the quote Excel workbook and maps product rows into QuoteItem objects.
    public class ExcelQuoteReader
    {
        public ExcelQuoteReader()
        {
        }

        private const string MoleculesSheetName = "Molecules";
        private const int FirstDataRow = 13;

        private const int LineNumberColumn = 1;
        private const int MolportIdColumn = 2;
        private const int ProductIdColumn = 3;
        private const int SupplierColumn = 4;
        private const int CatalogueNumberColumn = 5;
        private const int DeliveryTimeColumn = 6;
        private const int SearchCriteriaColumn = 7;
        private const int MatchTypeColumn = 8;
        private const int SmilesColumn = 9;
        private const int MolecularWeightColumn = 10;
        private const int UnitColumn = 11;
        private const int UnitPriceColumn = 12;
        private const int QuantityColumn = 13;
        private const int DiscountColumn = 14;
        private const int NetPriceColumn = 15;
        private const int PurityColumn = 16;
        private const int IupacColumn = 17;
        private const int ComplianceColumn = 18;

        public QuoteImportResult ReadQuote(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = "File path must be provided."
                };
            }

            if (!File.Exists(filePath))
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = "Quote file was not found."
                };
            }

            try
            {
                var quoteItems = Read(filePath);

                return new QuoteImportResult
                {
                    Success = true,
                    Items = quoteItems
                };
            }
            catch (Exception ex)
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = $"Could not read quote file: {ex.Message}"
                };
            }
        }

        public List<QuoteItem> Read(string filePath)
        {
            var quoteItems = new List<QuoteItem>();

            using var workbook = new XLWorkbook(filePath);

            var worksheet = workbook.Worksheet(MoleculesSheetName);

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? FirstDataRow;

            for (var rowNumber = FirstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                var lineNumber = ReadText(worksheet, rowNumber, LineNumberColumn);

                // Product rows are numbered like "1."; the first non-product line starts the footer.
                if (!IsProductRow(lineNumber))
                {
                    break;
                }

                var quoteItem = new QuoteItem
                {
                    ExcelRowNumber = rowNumber,
                    LineNumber = lineNumber,
                    MolportId = ReadText(worksheet, rowNumber, MolportIdColumn),
                    ProductId = ReadText(worksheet, rowNumber, ProductIdColumn),
                    Supplier = ReadText(worksheet, rowNumber, SupplierColumn),
                    CatalogueNumber = ReadText(worksheet, rowNumber, CatalogueNumberColumn),
                    DeliveryTimeBusinessDays = ReadInt(worksheet, rowNumber, DeliveryTimeColumn),
                    SearchCriteria = ReadText(worksheet, rowNumber, SearchCriteriaColumn),
                    MatchType = ReadText(worksheet, rowNumber, MatchTypeColumn),
                    Smiles = ReadText(worksheet, rowNumber, SmilesColumn),
                    MolecularWeight = ReadDecimal(worksheet, rowNumber, MolecularWeightColumn),
                    Unit = ReadText(worksheet, rowNumber, UnitColumn),
                    UnitPriceUsd = ReadDecimal(worksheet, rowNumber, UnitPriceColumn),
                    Quantity = ReadInt(worksheet, rowNumber, QuantityColumn),
                    DiscountUsd = ReadDecimal(worksheet, rowNumber, DiscountColumn),
                    NetPriceUsd = ReadDecimal(worksheet, rowNumber, NetPriceColumn),
                    Purity = ReadText(worksheet, rowNumber, PurityColumn),
                    Iupac = ReadText(worksheet, rowNumber, IupacColumn),
                    Compliance = ReadText(worksheet, rowNumber, ComplianceColumn)
                };

                quoteItems.Add(quoteItem);
            }

            return quoteItems;
        }

        private static string? ReadText(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            var text = worksheet.Cell(rowNumber, columnNumber).GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text.Trim();
        }

        private static decimal? ReadDecimal(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            var cell = worksheet.Cell(rowNumber, columnNumber);

            if (cell.IsEmpty())
            {
                return null;
            }

            // ClosedXML handles numeric cells directly; text parsing covers values pasted as strings.
            if (cell.TryGetValue<decimal>(out var decimalValue))
            {
                return decimalValue;
            }

            var text = cell.GetString();

            if (decimal.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        private static int? ReadInt(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            var cell = worksheet.Cell(rowNumber, columnNumber);

            if (cell.IsEmpty())
            {
                return null;
            }

            // Delivery time and quantity may come from Excel as either integers or numeric cells.
            if (cell.TryGetValue<int>(out var intValue))
            {
                return intValue;
            }

            if (cell.TryGetValue<decimal>(out var decimalValue))
            {
                return (int)decimalValue;
            }

            var text = cell.GetString();

            if (int.TryParse(
                    text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        private static bool IsProductRow(string? lineNumber)
        {
            if (string.IsNullOrWhiteSpace(lineNumber))
            {
                return false;
            }

            return Regex.IsMatch(lineNumber.Trim(), @"^\d+\.$");
        }
    }
}
