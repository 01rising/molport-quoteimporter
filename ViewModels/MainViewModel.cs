using System.Collections.ObjectModel;
using QuoteImporter.Models;
using QuoteImporter.Services;

namespace QuoteImporter.ViewModels
{
    // Coordinates the import workflow between the WPF view and service classes.
    // TODO: Implement INotifyPropertyChanged before binding mutable status values in XAML.
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
            // TODO: Make this async so larger Excel files do not block the UI thread.
            // TODO: Update StatusMessage before and after each import stage.
            // TODO: Wire up Excel reading, validation, and processing in this method.
            var result = _excelQuoteReader.ReadQuote(filePath);

            QuoteItems.Clear();
            foreach (var item in result.Items)
            {
                _quoteValidator.Validate(item);
                QuoteItems.Add(item);
            }

            _quoteProcessor.ProcessQuoteItems(QuoteItems);
            // TODO: Surface result.ErrorMessage when the read fails.
            StatusMessage = result.Success ? "Import complete" : "Import not implemented";
        }
    }
}
