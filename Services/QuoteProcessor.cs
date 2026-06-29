using System.Collections.Generic;
using System.Linq;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Applies derived calculations after rows have been read and validated.
    public class QuoteProcessor
    {
        public QuoteProcessor()
        {
        }

        public QuoteImportResult CreateSummary(IEnumerable<QuoteItem> quoteItems)
        {
            if (quoteItems == null)
            {
                return new QuoteImportResult();
            }

            var items = quoteItems.ToList();

            // Summary values are calculated from the already-validated rows.
            return new QuoteImportResult
            {
                TotalRows = items.Count,

                ValidRows = items.Count(item => item.IsValid),

                InvalidRows = items.Count(item => !item.IsValid),

                TotalValidNetPriceUsd = items
                    .Where(item => item.IsValid)
                    .Sum(item => item.NetPriceUsd ?? 0m)
            };
        }
    }
}
