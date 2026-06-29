using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using QuoteImporter.Models;
using QuoteImporter.Services;

namespace QuoteImporter
{
    // Code-behind for the main WPF window and the simplified import demo workflow.
    public partial class MainWindow : Window
    {
        private List<QuoteItem> _quoteItems = new();
        private QuoteImportResult _summary = new();

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

            SummaryText.Text = "Loading quote file...";
            QuoteGrid.ItemsSource = null;

            try
            {
                // Excel parsing and validation run on a background thread so the WPF UI stays responsive.
                var result = await Task.Run(() =>
                {
                    var reader = new ExcelQuoteReader();
                    var validator = new QuoteValidator();
                    var processor = new QuoteProcessor();

                    var items = reader.Read(dialog.FileName);

                    validator.ValidateAll(items);

                    var summary = processor.CreateSummary(items);

                    return new QuoteLoadResult
                    {
                        Items = items,
                        Summary = summary
                    };
                });

                _quoteItems = result.Items;
                _summary = result.Summary;

                QuoteGrid.ItemsSource = _quoteItems;

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
                $"Total rows: {_summary.TotalRows}\n" +
                $"Valid rows: {_summary.ValidRows}\n" +
                $"Invalid rows: {_summary.InvalidRows}\n" +
                $"Total valid net price: {_summary.TotalValidNetPriceUsd:N2} USD",
                "Import Quote",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateSummaryText()
        {
            SummaryText.Text =
                $"Rows: {_summary.TotalRows} | " +
                $"Valid: {_summary.ValidRows} | " +
                $"Invalid: {_summary.InvalidRows} | " +
                $"Total valid net: {_summary.TotalValidNetPriceUsd:N2} USD";
        }

        private class QuoteLoadResult
        {
            public List<QuoteItem> Items { get; set; } = new();

            public QuoteImportResult Summary { get; set; } = new();
        }
    }
}
