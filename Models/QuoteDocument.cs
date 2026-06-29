using System.Collections.Generic;

namespace QuoteImporter.Models
{
    // Combines quote-level metadata with the product rows from the file.
    public class QuoteDocument
    {
        public QuoteMetadata Metadata { get; set; } = new();

        public List<QuoteItem> Items { get; set; } = new();

        public List<QuoteShippingInfo> ShippingLimitations { get; set; } = new();
    }
}
