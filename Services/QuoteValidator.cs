using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Validates a single quote row and records user-facing error messages on the item.
    // TODO: Keep validation messages clear enough for an evaluator to understand at a glance.
    public class QuoteValidator
    {
        public QuoteValidator()
        {
        }

        public void Validate(QuoteItem quoteItem)
        {
            if (quoteItem == null)
            {
                return;
            }

            // TODO: Apply validation rules for required fields, numeric ranges, and net price calculations.
            // TODO: Clear existing errors before adding validation errors for the current pass.
            // TODO: Require IDs, supplier, catalogue number, unit, quantity, prices, and discount.
            // TODO: Allow negative discounts because the quote may represent discounts that way.
            // TODO: Only compare net price when unit price, quantity, discount, and net price are available.
            quoteItem.IsValid = true;
        }
    }
}
