using System.Collections.Generic;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Carries the outcome of reading an Excel quote file back to the view model.
    // TODO: Add counts such as total rows and invalid rows once processing is implemented.
    public class QuoteImportResult
    {
        public bool Success { get; set; }
        // TODO: Prefer user-friendly messages here instead of raw exception text.
        public string? ErrorMessage { get; set; }
        public IEnumerable<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
