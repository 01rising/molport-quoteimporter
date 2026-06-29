using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Reads quote-level fields from the header area of the Molecules worksheet.
    internal static class QuoteMetadataReader
    {
        private const int MetadataSearchRows = 12;
        private const int MetadataSearchColumns = 18;

        public static QuoteMetadata Read(IXLWorksheet worksheet)
        {
            // Metadata labels live above the product table and may vary slightly between quote files.
            return new QuoteMetadata
            {
                QuotationNumber = FindMetadataValueByLabels(
                    worksheet,
                    "Quotation"),
                IssueDate = FindMetadataValueByLabels(
                    worksheet,
                    "Issue date",
                    "Date issued",
                    "Quote date"),
                ValidUntil = FindMetadataValueByLabels(
                    worksheet,
                    "Valid until",
                    "Valid till",
                    "Valid to",
                    "Valid through",
                    "Validity"),
                ShippingAddress = FindMetadataValueByLabels(
                    worksheet,
                    "Shipping address"),
                BillingAddress = FindMetadataValueByLabels(
                    worksheet,
                    "Billing address")
            };
        }

        private static string? FindMetadataValueByLabels(IXLWorksheet worksheet, params string[] labels)
        {
            // Search only the quote header area so product rows do not accidentally match metadata labels.
            for (var row = 1; row <= MetadataSearchRows; row++)
            {
                for (var column = 1; column <= MetadataSearchColumns; column++)
                {
                    var text = ExcelCellReader.ReadText(worksheet, row, column);

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    var matchedLabel = FindMatchingMetadataLabel(text, labels);

                    if (matchedLabel == null)
                    {
                        continue;
                    }

                    var sameCellValue = CleanMetadataText(text, matchedLabel);

                    // Some metadata appears as "Label: value" in one cell.
                    if (!string.IsNullOrWhiteSpace(sameCellValue))
                    {
                        return sameCellValue;
                    }

                    // Other metadata uses the label cell followed by the value in the same row.
                    return FindNextCellTextInRow(worksheet, row, column + 1);
                }
            }

            return null;
        }

        private static string? FindMatchingMetadataLabel(string text, IEnumerable<string> labels)
        {
            foreach (var label in labels)
            {
                if (text.Contains(label, StringComparison.OrdinalIgnoreCase))
                {
                    return label;
                }

                // Normalized matching handles labels like "Quotation No.", "Quotation no:", or "Quotation #".
                if (ExcelCellReader.NormalizeText(text).Contains(ExcelCellReader.NormalizeText(label)))
                {
                    return label;
                }
            }

            return null;
        }

        private static string? FindNextCellTextInRow(IXLWorksheet worksheet, int row, int startColumn)
        {
            // Skip blank cells between a label and its value, which can happen with merged-looking layouts.
            for (var column = startColumn; column <= MetadataSearchColumns; column++)
            {
                var text = ExcelCellReader.ReadText(worksheet, row, column);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return null;
        }

        private static string? CleanMetadataText(string? text, string labelToRemove)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var cleanedText = text
                .Replace(labelToRemove, "", StringComparison.OrdinalIgnoreCase)
                .TrimStart(' ', '\t', ':', '-', '#', '.')
                .Trim();

            if (string.Equals(cleanedText, text, StringComparison.Ordinal))
            {
                // If punctuation prevented direct label removal, fall back to text after ":" or "-".
                cleanedText = ExtractTextAfterSeparator(text) ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(cleanedText) ? null : cleanedText;
        }

        private static string? ExtractTextAfterSeparator(string text)
        {
            var separatorIndex = text.IndexOf(':');

            if (separatorIndex < 0)
            {
                separatorIndex = text.IndexOf('-');
            }

            if (separatorIndex < 0 || separatorIndex == text.Length - 1)
            {
                return null;
            }

            return text[(separatorIndex + 1)..].Trim();
        }
    }
}
