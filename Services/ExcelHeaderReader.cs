using System.Collections.Generic;
using ClosedXML.Excel;

namespace QuoteImporter.Services
{
    // Finds table headers and maps header labels to Excel column numbers.
    internal static class ExcelHeaderReader
    {
        private const int HeaderSearchRows = 20;
        private const int HeaderSearchColumns = 25;

        public static int FindHeaderRow(
            IXLWorksheet worksheet,
            int fallbackRow,
            IEnumerable<string> knownLabels,
            int minimumScore = 3)
        {
            var bestRow = fallbackRow;
            var bestScore = 0;

            // Pick the row with the most known header labels instead of relying on one fixed row number.
            for (var row = 1; row <= HeaderSearchRows; row++)
            {
                var score = CountHeaderMatches(worksheet, row, knownLabels);

                if (score > bestScore)
                {
                    bestRow = row;
                    bestScore = score;
                }
            }

            return bestScore >= minimumScore ? bestRow : fallbackRow;
        }

        public static int FindHeaderColumn(
            IXLWorksheet worksheet,
            int headerRow,
            int fallbackColumn,
            params string[] labels)
        {
            for (var column = 1; column <= HeaderSearchColumns; column++)
            {
                var text = ExcelCellReader.ReadText(worksheet, headerRow, column);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (FindMatchingHeaderLabel(text, labels) != null)
                {
                    return column;
                }
            }

            // Fall back to the known layout when a header label is missing or renamed unexpectedly.
            return fallbackColumn;
        }

        private static int CountHeaderMatches(
            IXLWorksheet worksheet,
            int row,
            IEnumerable<string> knownLabels)
        {
            var matches = 0;

            for (var column = 1; column <= HeaderSearchColumns; column++)
            {
                var text = ExcelCellReader.ReadText(worksheet, row, column);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (FindMatchingHeaderLabel(text, knownLabels) != null)
                {
                    matches++;
                }
            }

            return matches;
        }

        private static string? FindMatchingHeaderLabel(string text, IEnumerable<string> labels)
        {
            var normalizedText = ExcelCellReader.NormalizeText(text);

            foreach (var label in labels)
            {
                var normalizedLabel = ExcelCellReader.NormalizeText(label);

                if (normalizedText == normalizedLabel)
                {
                    return label;
                }

                if (CanUsePartialHeaderMatch(normalizedLabel) && normalizedText.Contains(normalizedLabel))
                {
                    return label;
                }
            }

            return null;
        }

        private static bool CanUsePartialHeaderMatch(string normalizedLabel)
        {
            // Very short labels are too easy to match accidentally inside longer column names.
            return normalizedLabel.Length > 3
                && normalizedLabel != "line"
                && normalizedLabel != "unit";
        }
    }
}
