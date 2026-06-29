using System.Collections.Generic;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Carries read status and processed quote totals back to the UI layer.
    public class QuoteImportResult
    {
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public decimal TotalValidNetPriceUsd { get; set; }
        public bool Success { get; set; }
        // User-facing error text for file selection, workbook loading, or parsing failures.
        public string? ErrorMessage { get; set; }
        public QuoteMetadata Metadata { get; set; } = new();
        public IEnumerable<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
