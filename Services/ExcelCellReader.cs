using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace QuoteImporter.Services
{
    // Shared helpers for safe nullable reads from Excel cells.
    internal static class ExcelCellReader
    {
        public static string? ReadText(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            var text = worksheet.Cell(rowNumber, columnNumber).GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text.Trim();
        }

        public static decimal? ReadDecimal(IXLWorksheet worksheet, int rowNumber, int columnNumber)
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

        public static int? ReadInt(IXLWorksheet worksheet, int rowNumber, int columnNumber)
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

        public static bool IsProductRow(string? lineNumber)
        {
            if (string.IsNullOrWhiteSpace(lineNumber))
            {
                return false;
            }

            // Product rows use numbering like "1."; footer or total rows do not.
            return Regex.IsMatch(lineNumber.Trim(), @"^\d+\.$");
        }

        public static bool TextEquals(string? first, string? second)
        {
            return string.Equals(
                first?.Trim(),
                second?.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeText(string text)
        {
            // Header matching ignores punctuation and spaces, for example "Molport ID" vs "MolportId".
            return Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9]+", string.Empty);
        }
    }
}
