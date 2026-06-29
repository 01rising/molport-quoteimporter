using System;
using System.Collections.Generic;
using System.IO;
using QuoteImporter.Models;
using QuoteImporter.Services;

namespace QuoteImporter.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            // Simple no-dependency test runner: each tuple names and executes one test case.
            var tests = new List<(string Name, Action Test)>
            {
                ("Validator reports missing required fields", ValidatorReportsMissingRequiredFields),
                ("Validator rejects zero quantity", ValidatorRejectsZeroQuantity),
                ("Validator rejects negative unit price", ValidatorRejectsNegativeUnitPrice),
                ("Validator rejects negative net price", ValidatorRejectsNegativeNetPrice),
                ("Validator reports net price mismatch", ValidatorReportsNetPriceMismatch),
                ("Validator skips mismatch when calculation inputs are missing", ValidatorSkipsMismatchWhenCalculationInputsAreMissing),
                ("Excel reader loads sample quote when present", ExcelReaderLoadsSampleQuoteWhenPresent)
            };

            var failures = 0;

            foreach (var test in tests)
            {
                try
                {
                    test.Test();
                    Console.WriteLine($"PASS: {test.Name}");
                }
                catch (Exception ex)
                {
                    failures++;
                    Console.WriteLine($"FAIL: {test.Name}");
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine(failures == 0
                ? $"All {tests.Count} tests passed."
                : $"{failures} of {tests.Count} tests failed.");

            return failures == 0 ? 0 : 1;
        }

        private static void ValidatorReportsMissingRequiredFields()
        {
            // Sample input: a completely empty row should trigger required-field errors.
            var item = new QuoteItem();
            var validator = new QuoteValidator();

            validator.Validate(item);

            AssertFalse(item.IsValid, "Empty quote item should be invalid.");
            AssertContains(item.Errors, "Molport ID is required.");
            AssertContains(item.Errors, "Quantity is required.");
            AssertContains(item.Errors, "Net price is required.");
        }

        private static void ValidatorRejectsZeroQuantity()
        {
            var item = CreateValidQuoteItem();
            item.Quantity = 0;

            ValidateAndExpectError(item, "Quantity must be greater than 0.");
        }

        private static void ValidatorRejectsNegativeUnitPrice()
        {
            var item = CreateValidQuoteItem();
            item.UnitPriceUsd = -1m;

            ValidateAndExpectError(item, "Unit price cannot be negative.");
        }

        private static void ValidatorRejectsNegativeNetPrice()
        {
            var item = CreateValidQuoteItem();
            item.NetPriceUsd = -1m;

            ValidateAndExpectError(item, "Net price cannot be negative.");
        }

        private static void ValidatorReportsNetPriceMismatch()
        {
            var item = CreateValidQuoteItem();
            item.NetPriceUsd = 99m;

            ValidateAndExpectError(item, "Net price mismatch. Expected 15, actual 99.");
        }

        private static void ValidatorSkipsMismatchWhenCalculationInputsAreMissing()
        {
            // Missing unit price should produce the required-field error, not a misleading mismatch.
            var item = CreateValidQuoteItem();
            item.UnitPriceUsd = null;
            item.NetPriceUsd = 99m;
            var validator = new QuoteValidator();

            validator.Validate(item);

            AssertContains(item.Errors, "Unit price is required.");
            AssertDoesNotContain(item.Errors, "Net price mismatch. Expected");
        }

        private static void ExcelReaderLoadsSampleQuoteWhenPresent()
        {
            // The sample workbook lives at the repository root; test output runs from bin/Debug.
            var samplePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "Test_Quote_LPB03N2557924.xlsx"));

            if (!File.Exists(samplePath))
            {
                Console.WriteLine("SKIP: sample quote workbook was not found.");
                return;
            }

            var reader = new ExcelQuoteReader();
            var document = reader.ReadDocument(samplePath);

            AssertTrue(document.Items.Count > 0, "Sample quote should contain product rows.");
            AssertTrue(
                document.Items[0].ShippingInfo != null,
                "First sample quote row should have matching shipping limitation details.");
            AssertTrue(
                document.Metadata.TariffSurchargeUsd != null,
                "Sample quote should include tariff surcharge metadata.");
            AssertTrue(
                document.Metadata.MolportShippingUsd != null,
                "Sample quote should include Molport shipping metadata.");
            AssertTrue(
                document.Metadata.TotalOrderValueUsd != null,
                "Sample quote should include total order value metadata.");
        }

        private static QuoteItem CreateValidQuoteItem()
        {
            return new QuoteItem
            {
                MolportId = "Molport-001",
                ProductId = "Product-001",
                Supplier = "Sample Supplier",
                CatalogueNumber = "CAT-001",
                Unit = "1 g",
                UnitPriceUsd = 10m,
                Quantity = 2,
                DiscountUsd = -5m,
                NetPriceUsd = 15m
            };
        }

        private static void ValidateAndExpectError(QuoteItem item, string expectedError)
        {
            var validator = new QuoteValidator();

            validator.Validate(item);

            AssertFalse(item.IsValid, "Quote item should be invalid.");
            AssertContains(item.Errors, expectedError);
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertFalse(bool condition, string message)
        {
            AssertTrue(!condition, message);
        }

        private static void AssertContains(IEnumerable<string> values, string expected)
        {
            foreach (var value in values)
            {
                if (string.Equals(value, expected, StringComparison.Ordinal))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Expected error was not found: {expected}");
        }

        private static void AssertDoesNotContain(IEnumerable<string> values, string unexpected)
        {
            foreach (var value in values)
            {
                if (value.Contains(unexpected, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Unexpected error was found: {value}");
                }
            }
        }
    }
}
