using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuoteImporter.Models;
using QuoteImporter.Services;

namespace QuoteImporter
{
    // Code-behind for the main WPF window and the simplified import workflow.
    public partial class MainWindow : Window
    {
        private List<QuoteItem> _quoteItems = new();
        private QuoteImportResult _summary = new();
        private QuoteMetadata _metadata = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadQuote_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Select quote file"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            // Clear the previous load before starting a new file so stale details are not displayed.
            SummaryText.Text = "Loading quote file...";
            QuoteGrid.ItemsSource = null;
            RowDetailsText.Text = "Select a quote row to see all columns.";
            _metadata = new QuoteMetadata();

            try
            {
                // Excel parsing and validation run on a background thread so the WPF UI stays responsive.
                var result = await Task.Run(() =>
                {
                    var reader = new ExcelQuoteReader();
                    var validator = new QuoteValidator();
                    var processor = new QuoteProcessor();

                    var document = reader.ReadDocument(dialog.FileName);
                    var items = document.Items;

                    validator.ValidateAll(items);

                    var summary = processor.CreateSummary(items);

                    return new QuoteLoadResult
                    {
                        Items = items,
                        Summary = summary,
                        Metadata = document.Metadata
                    };
                });

                _quoteItems = result.Items;
                _summary = result.Summary;
                _metadata = result.Metadata;

                QuoteGrid.ItemsSource = _quoteItems;

                // Selecting the first row immediately populates the detail panel after a successful load.
                QuoteGrid.SelectedIndex = _quoteItems.Count > 0 ? 0 : -1;

                UpdateSummaryText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load quote file.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                SummaryText.Text = "Failed to load quote file.";
            }
        }

        private void ImportQuote_Click(object sender, RoutedEventArgs e)
        {
            if (_quoteItems.Count == 0)
            {
                MessageBox.Show(
                    "Please load a quote file first.",
                    "Import Quote",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            // The technical task only asks for a visual import action, so this reports the prepared data.
            MessageBox.Show(
                $"Import simulated.\n\n" +
                $"Quotation: {FormatMetadataValue(_metadata.QuotationNumber)}\n" +
                $"Issue date: {FormatMetadataValue(_metadata.IssueDate)}\n" +
                $"Valid until: {FormatMetadataValue(_metadata.ValidUntil)}\n\n" +
                $"Shipping address:\n{FormatMetadataValue(_metadata.ShippingAddress)}\n\n" +
                $"Billing address:\n{FormatMetadataValue(_metadata.BillingAddress)}\n\n" +
                $"Total rows: {_summary.TotalRows}\n" +
                $"Valid rows: {_summary.ValidRows}\n" +
                $"Invalid rows: {_summary.InvalidRows}\n" +
                $"Total valid net price: {_summary.TotalValidNetPriceUsd:N2} USD",
                "Import Quote",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void QuoteGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clicking a row in the compact grid updates the full-property detail view.
            if (QuoteGrid.SelectedItem is not QuoteItem selectedItem)
            {
                RowDetailsText.Text = "Select a quote row to see all columns.";
                return;
            }

            RowDetailsText.Text = BuildRowDetailsText(selectedItem);
        }

        private void UpdateSummaryText()
        {
            SummaryText.Text =
                $"Quote: {FormatMetadataValue(_metadata.QuotationNumber)} | " +
                $"Rows: {_summary.TotalRows} | " +
                $"Valid: {_summary.ValidRows} | " +
                $"Invalid: {_summary.InvalidRows} | " +
                $"Total valid net: {_summary.TotalValidNetPriceUsd:N2} USD";
        }

        private static string FormatMetadataValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(not found)" : value;
        }

        private static string BuildRowDetailsText(QuoteItem item)
        {
            var details = new StringBuilder();

            // Keep the detail view explicit so the field order matches the Excel quote layout.
            AppendDetail(details, "Excel row", item.ExcelRowNumber);
            AppendDetail(details, "Line number", item.LineNumber);
            AppendDetail(details, "Molport ID", item.MolportId);
            AppendDetail(details, "Product ID", item.ProductId);
            AppendDetail(details, "Supplier", item.Supplier);
            AppendDetail(details, "Catalogue number", item.CatalogueNumber);
            AppendDetail(details, "Delivery time business days", item.DeliveryTimeBusinessDays);
            AppendDetail(details, "Search criteria", item.SearchCriteria);
            AppendDetail(details, "Match type", item.MatchType);
            AppendDetail(details, "SMILES", item.Smiles);
            AppendDetail(details, "Molecular weight", item.MolecularWeight);
            AppendDetail(details, "Unit", item.Unit);
            AppendDetail(details, "Unit price USD", item.UnitPriceUsd);
            AppendDetail(details, "Quantity", item.Quantity);
            AppendDetail(details, "Discount USD", item.DiscountUsd);
            AppendDetail(details, "Net price USD", item.NetPriceUsd);
            AppendDetail(details, "Purity", item.Purity);
            AppendDetail(details, "IUPAC", item.Iupac);
            AppendDetail(details, "Compliance", item.Compliance);
            AppendDetail(details, "Calculated net price USD", item.CalculatedNetPriceUsd);
            AppendDetail(details, "Net price difference USD", item.NetPriceDifferenceUsd);
            AppendDetail(details, "Status", item.Status);
            AppendDetail(details, "Errors", item.ErrorText);
            AppendShippingDetails(details, item.ShippingInfo);

            return details.ToString();
        }

        private static void AppendShippingDetails(StringBuilder details, QuoteShippingInfo? shippingInfo)
        {
            details.AppendLine();
            details.AppendLine("Shipping limitations worksheet:");

            if (shippingInfo == null)
            {
                details.AppendLine("Shipping details: (not found)");
                return;
            }

            // Molport ID confirms the match; repeated product fields from the main sheet stay hidden.
            AppendDetail(details, "Shipping Molport ID", shippingInfo.MolportId);
            AppendDetail(details, "Country of origin", shippingInfo.CountryOfOrigin);
            AppendDetail(details, "UN number", shippingInfo.UnNumber);
            AppendDetail(details, "Hazardous class", shippingInfo.HazardousClass);
            AppendDetail(details, "Packing group", shippingInfo.PackingGroup);
            AppendDetail(details, "Shipping limitations", shippingInfo.ShippingLimitations);
            AppendDetail(details, "Compound state", shippingInfo.CompoundState);
            AppendDetail(details, "Solubility", shippingInfo.Solubility);
        }

        private static void AppendDetail(StringBuilder details, string label, object? value)
        {
            // Null or whitespace values are shown explicitly so missing spreadsheet data is obvious.
            var displayValue = value switch
            {
                null => "(empty)",
                string text when string.IsNullOrWhiteSpace(text) => "(empty)",
                decimal decimalValue => decimalValue.ToString("0.##"),
                _ => value.ToString()
            };

            details.AppendLine($"{label}: {displayValue}");
        }

        private class QuoteLoadResult
        {
            // Bundles background-thread results so UI controls are updated only after await returns.
            public List<QuoteItem> Items { get; set; } = new();

            public QuoteImportResult Summary { get; set; } = new();

            public QuoteMetadata Metadata { get; set; } = new();
        }
    }
}
