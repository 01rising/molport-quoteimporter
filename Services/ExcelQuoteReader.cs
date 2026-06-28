using System;
using ClosedXML.Excel;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Reads the quote Excel workbook and maps product rows into QuoteItem objects.
    // TODO: Move worksheet names and row numbers into constants when parsing is implemented.
    public class ExcelQuoteReader
    {
        public ExcelQuoteReader()
        {
        }

        public QuoteImportResult ReadQuote(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path must be provided.", nameof(filePath));
            }

            // TODO: Verify the file exists and return a friendly import error when it does not.
            // TODO: Open the workbook with ClosedXML and select the "Molecules" worksheet.
            // TODO: Read the "Molecules" worksheet and parse rows starting at row 13.
            // TODO: Stop at the first row where column A no longer looks like a product line.
            // TODO: Map columns A-R to QuoteItem properties using safe nullable conversions.
            return new QuoteImportResult
            {
                Success = false,
                ErrorMessage = "Not implemented"
            };
        }
    }
}
