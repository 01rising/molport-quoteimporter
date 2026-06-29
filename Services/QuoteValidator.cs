using System;
using System.Collections.Generic;
using QuoteImporter.Models;

namespace QuoteImporter.Services
{
    // Applies row-level validation rules before the quote is displayed or imported.
    public class QuoteValidator
    {
        public QuoteValidator()
        {
        }

        public void ValidateAll(IEnumerable<QuoteItem> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                Validate(item);
            }
        }

        public void Validate(QuoteItem item)
        {
            if (item == null)
            {
                return;
            }

            // Revalidation starts from a clean slate so corrected rows do not keep stale errors.
            item.Errors.Clear();

            if (string.IsNullOrWhiteSpace(item.MolportId))
            {
                item.Errors.Add("Molport ID is required.");
            }

            if (string.IsNullOrWhiteSpace(item.ProductId))
            {
                item.Errors.Add("Product ID is required.");
            }

            if (string.IsNullOrWhiteSpace(item.Supplier))
            {
                item.Errors.Add("Supplier is required.");
            }

            if (string.IsNullOrWhiteSpace(item.CatalogueNumber))
            {
                item.Errors.Add("Catalogue number is required.");
            }

            if (string.IsNullOrWhiteSpace(item.Unit))
            {
                item.Errors.Add("Unit is required.");
            }

            if (item.Quantity == null)
            {
                item.Errors.Add("Quantity is required.");
            }
            else if (item.Quantity <= 0)
            {
                item.Errors.Add("Quantity must be greater than 0.");
            }

            if (item.UnitPriceUsd == null)
            {
                item.Errors.Add("Unit price is required.");
            }
            else if (item.UnitPriceUsd < 0)
            {
                item.Errors.Add("Unit price cannot be negative.");
            }

            if (item.DiscountUsd == null)
            {
                item.Errors.Add("Discount is required.");
            }

            if (item.NetPriceUsd == null)
            {
                item.Errors.Add("Net price is required.");
            }
            else if (item.NetPriceUsd < 0)
            {
                item.Errors.Add("Net price cannot be negative.");
            }

            ValidateNetPrice(item);
        }

        private void ValidateNetPrice(QuoteItem item)
        {
            if (item.CalculatedNetPriceUsd == null || item.NetPriceUsd == null)
            {
                return;
            }

            // Allow a one-cent tolerance for spreadsheet rounding.
            var difference = Math.Abs(
                item.NetPriceUsd.Value - item.CalculatedNetPriceUsd.Value);

            if (difference > 0.01m)
            {
                item.Errors.Add(
                    $"Net price mismatch. Expected {item.CalculatedNetPriceUsd.Value}, actual {item.NetPriceUsd.Value}.");
            }
        }
    }
}
