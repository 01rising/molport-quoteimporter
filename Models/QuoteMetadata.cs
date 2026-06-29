namespace QuoteImporter.Models
{
    // Quote-level information that applies to the whole workbook, not one product row.
    public class QuoteMetadata
    {
        public string? QuotationNumber { get; set; }

        public string? IssueDate { get; set; }

        public string? ValidUntil { get; set; }

        public string? ShippingAddress { get; set; }

        public string? BillingAddress { get; set; }
    }
}
