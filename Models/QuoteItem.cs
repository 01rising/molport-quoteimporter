using System.Collections.Generic;

namespace QuoteImporter.Models
{
    // Represents one product row read from the quote spreadsheet.
    // TODO: Add display-friendly formatting or notification support if the grid needs live updates.
    public class QuoteItem
    {
        // Source row metadata helps users trace validation errors back to Excel.
        public int? ExcelRowNumber { get; set; }
        public string? LineNumber { get; set; }

        // Quote identity and supplier details copied from the Molecules worksheet.
        public string? MolportId { get; set; }
        public string? ProductId { get; set; }
        public string? Supplier { get; set; }
        public string? CatalogueNumber { get; set; }
        public int? DeliveryTimeBusinessDays { get; set; }
        public string? SearchCriteria { get; set; }
        public string? MatchType { get; set; }
        public string? Smiles { get; set; }
        public decimal? MolecularWeight { get; set; }

        // Pricing inputs from Excel; validation and processing will fill derived values below.
        public string? Unit { get; set; }
        public decimal? UnitPriceUsd { get; set; }
        public int? Quantity { get; set; }
        public decimal? DiscountUsd { get; set; }
        public decimal? NetPriceUsd { get; set; }
        public string? Purity { get; set; }
        public string? Iupac { get; set; }
        public string? Compliance { get; set; }
        public decimal? CalculatedNetPriceUsd { get; set; }
        public decimal? NetPriceDifferenceUsd { get; set; }

        // Validation state used by the UI to explain and highlight invalid rows.
        // TODO: Keep ErrorText synchronized with Errors after validation is implemented.
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid { get; set; } = true;
        public string? Status { get; set; }
        public string? ErrorText { get; set; }
    }
}
