using System.Collections.Generic;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Applies derived calculations after rows have been read and validated.
    // TODO: Keep this class focused on calculations, not file reading or UI updates.
    public class QuoteProcessor
    {
        public QuoteProcessor()
        {
        }

        public void ProcessQuoteItems(IEnumerable<QuoteItem> quoteItems)
        {
            if (quoteItems == null)
            {
                return;
            }

            // TODO: Compute derived values for quote rows and prepare them for display.
            // TODO: Calculate UnitPriceUsd * Quantity + DiscountUsd when all inputs exist.
            // TODO: Store the difference between Excel net price and calculated net price.
            // TODO: Set a clear Status value such as "Valid" or "Needs review" for the grid.
        }
    }
}
