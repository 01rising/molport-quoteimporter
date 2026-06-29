using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Reads the quote Excel workbook and maps product rows into QuoteItem objects.
    public class ExcelQuoteReader
    {
        public ExcelQuoteReader()
        {
        }

        private const string MoleculesSheetName = "Molecules";
        private const int FirstDataRow = 13;

        // Fallback columns match the known Molport layout if header search cannot identify a column.
        private const int DefaultLineNumberColumn = 1;
        private const int DefaultMolportIdColumn = 2;
        private const int DefaultProductIdColumn = 3;
        private const int DefaultSupplierColumn = 4;
        private const int DefaultCatalogueNumberColumn = 5;
        private const int DefaultDeliveryTimeColumn = 6;
        private const int DefaultSearchCriteriaColumn = 7;
        private const int DefaultMatchTypeColumn = 8;
        private const int DefaultSmilesColumn = 9;
        private const int DefaultMolecularWeightColumn = 10;
        private const int DefaultUnitColumn = 11;
        private const int DefaultUnitPriceColumn = 12;
        private const int DefaultQuantityColumn = 13;
        private const int DefaultDiscountColumn = 14;
        private const int DefaultNetPriceColumn = 15;
        private const int DefaultPurityColumn = 16;
        private const int DefaultIupacColumn = 17;
        private const int DefaultComplianceColumn = 18;

        public QuoteImportResult ReadQuote(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = "File path must be provided."
                };
            }

            if (!File.Exists(filePath))
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = "Quote file was not found."
                };
            }

            try
            {
                var quoteDocument = ReadDocument(filePath);

                return new QuoteImportResult
                {
                    Success = true,
                    Metadata = quoteDocument.Metadata,
                    Items = quoteDocument.Items
                };
            }
            catch (Exception ex)
            {
                return new QuoteImportResult
                {
                    Success = false,
                    ErrorMessage = $"Could not read quote file: {ex.Message}"
                };
            }
        }

        public List<QuoteItem> Read(string filePath)
        {
            // Preserve the simpler row-only read for callers that do not need metadata.
            return ReadDocument(filePath).Items;
        }

        public QuoteDocument ReadDocument(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);

            var worksheet = workbook.Worksheet(MoleculesSheetName);
            var items = ReadItems(worksheet);
            var shippingLimitations = ShippingLimitationsReader.Read(workbook);

            // Shipping data lives on a separate worksheet, so attach matching rows after both sheets are read.
            ShippingLimitationsReader.AttachToItems(items, shippingLimitations);

            return new QuoteDocument
            {
                Metadata = QuoteMetadataReader.Read(worksheet),
                Items = items,
                ShippingLimitations = shippingLimitations
            };
        }

        private static List<QuoteItem> ReadItems(IXLWorksheet worksheet)
        {
            var quoteItems = new List<QuoteItem>();
            var headerRow = FindQuoteHeaderRow(worksheet);
            var firstDataRow = headerRow + 1;
            var columns = BuildQuoteColumnMap(worksheet, headerRow);

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? firstDataRow;

            for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                var lineNumber = ExcelCellReader.ReadText(worksheet, rowNumber, columns.LineNumber);

                // Product rows are numbered like "1."; the first non-product line starts the footer.
                if (!ExcelCellReader.IsProductRow(lineNumber))
                {
                    break;
                }

                var quoteItem = new QuoteItem
                {
                    ExcelRowNumber = rowNumber,
                    LineNumber = lineNumber,
                    MolportId = ExcelCellReader.ReadText(worksheet, rowNumber, columns.MolportId),
                    ProductId = ExcelCellReader.ReadText(worksheet, rowNumber, columns.ProductId),
                    Supplier = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Supplier),
                    CatalogueNumber = ExcelCellReader.ReadText(worksheet, rowNumber, columns.CatalogueNumber),
                    DeliveryTimeBusinessDays = ExcelCellReader.ReadInt(worksheet, rowNumber, columns.DeliveryTime),
                    SearchCriteria = ExcelCellReader.ReadText(worksheet, rowNumber, columns.SearchCriteria),
                    MatchType = ExcelCellReader.ReadText(worksheet, rowNumber, columns.MatchType),
                    Smiles = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Smiles),
                    MolecularWeight = ExcelCellReader.ReadDecimal(worksheet, rowNumber, columns.MolecularWeight),
                    Unit = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Unit),
                    UnitPriceUsd = ExcelCellReader.ReadDecimal(worksheet, rowNumber, columns.UnitPrice),
                    Quantity = ExcelCellReader.ReadInt(worksheet, rowNumber, columns.Quantity),
                    DiscountUsd = ExcelCellReader.ReadDecimal(worksheet, rowNumber, columns.Discount),
                    NetPriceUsd = ExcelCellReader.ReadDecimal(worksheet, rowNumber, columns.NetPrice),
                    Purity = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Purity),
                    Iupac = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Iupac),
                    Compliance = ExcelCellReader.ReadText(worksheet, rowNumber, columns.Compliance)
                };

                quoteItems.Add(quoteItem);
            }

            return quoteItems;
        }

        private static int FindQuoteHeaderRow(IXLWorksheet worksheet)
        {
            // Find the row that looks most like the product table header.
            return ExcelHeaderReader.FindHeaderRow(
                worksheet,
                FirstDataRow - 1,
                KnownQuoteHeaderLabels());
        }

        private static QuoteColumnMap BuildQuoteColumnMap(IXLWorksheet worksheet, int headerRow)
        {
            // Each field searches possible header names first and falls back to the fixed columns.
            return new QuoteColumnMap
            {
                LineNumber = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultLineNumberColumn, "Line", "Line number", "#"),
                MolportId = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultMolportIdColumn, "Molport ID", "MolportId"),
                ProductId = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultProductIdColumn, "Product ID", "ProductId"),
                Supplier = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultSupplierColumn, "Supplier"),
                CatalogueNumber = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultCatalogueNumberColumn,
                    "Catalogue number",
                    "Catalog number",
                    "Catalogue no",
                    "Catalog no"),
                DeliveryTime = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultDeliveryTimeColumn,
                    "Delivery time",
                    "Delivery time business days",
                    "Delivery"),
                SearchCriteria = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultSearchCriteriaColumn, "Search criteria"),
                MatchType = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultMatchTypeColumn, "Match type"),
                Smiles = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultSmilesColumn, "SMILES", "Smiles"),
                MolecularWeight = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultMolecularWeightColumn,
                    "Molecular weight",
                    "Mol weight",
                    "MW"),
                Unit = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultUnitColumn, "Unit"),
                UnitPrice = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultUnitPriceColumn,
                    "Unit price",
                    "Unit price USD",
                    "Unit price, USD"),
                Quantity = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultQuantityColumn, "Quantity", "Qty", "QTY"),
                Discount = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultDiscountColumn,
                    "Discount",
                    "Discount USD",
                    "Discount, USD"),
                NetPrice = ExcelHeaderReader.FindHeaderColumn(
                    worksheet,
                    headerRow,
                    DefaultNetPriceColumn,
                    "Net price",
                    "Net price USD",
                    "Net price, USD"),
                Purity = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultPurityColumn, "Purity"),
                Iupac = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultIupacColumn, "IUPAC", "Iupac"),
                Compliance = ExcelHeaderReader.FindHeaderColumn(worksheet, headerRow, DefaultComplianceColumn, "Compliance")
            };
        }

        private static IEnumerable<string> KnownQuoteHeaderLabels()
        {
            // Used to score candidate header rows before individual columns are mapped.
            return new[]
            {
                "Line",
                "Molport ID",
                "Product ID",
                "Supplier",
                "Catalogue number",
                "Delivery time",
                "Search criteria",
                "Match type",
                "SMILES",
                "Molecular weight",
                "Unit",
                "Unit price",
                "Quantity",
                "Discount",
                "Net price",
                "Purity",
                "IUPAC",
                "Compliance"
            };
        }

        private class QuoteColumnMap
        {
            public int LineNumber { get; set; }
            public int MolportId { get; set; }
            public int ProductId { get; set; }
            public int Supplier { get; set; }
            public int CatalogueNumber { get; set; }
            public int DeliveryTime { get; set; }
            public int SearchCriteria { get; set; }
            public int MatchType { get; set; }
            public int Smiles { get; set; }
            public int MolecularWeight { get; set; }
            public int Unit { get; set; }
            public int UnitPrice { get; set; }
            public int Quantity { get; set; }
            public int Discount { get; set; }
            public int NetPrice { get; set; }
            public int Purity { get; set; }
            public int Iupac { get; set; }
            public int Compliance { get; set; }
        }

    }
}
