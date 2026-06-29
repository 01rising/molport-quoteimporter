using System.Collections.ObjectModel;
using QuoteImporter.Models;
using QuoteImporter.Services;

namespace QuoteImporter.ViewModels
{
    // Coordinates the import workflow between the WPF view and service classes.
    public class MainViewModel
    {
        private readonly ExcelQuoteReader _excelQuoteReader;
        private readonly QuoteValidator _quoteValidator;
        private readonly QuoteProcessor _quoteProcessor;

        public MainViewModel()
        {
            _excelQuoteReader = new ExcelQuoteReader();
            _quoteValidator = new QuoteValidator();
            _quoteProcessor = new QuoteProcessor();

            QuoteItems = new ObservableCollection<QuoteItem>();
            StatusMessage = "Ready";
        }

        public ObservableCollection<QuoteItem> QuoteItems { get; }

        public string StatusMessage { get; set; }

        public void ImportQuote(string filePath)
        {
            // This view-model workflow mirrors the code-behind import path for future binding work.
            var result = _excelQuoteReader.ReadQuote(filePath);

            QuoteItems.Clear();
            foreach (var item in result.Items)
            {
                _quoteValidator.Validate(item);
                QuoteItems.Add(item);
            }

            _quoteProcessor.CreateSummary(QuoteItems);
            // Keep the status simple for now; detailed read errors are carried on the result object.
            StatusMessage = result.Success ? "Import complete" : "Import not implemented";
        }
    }
}
