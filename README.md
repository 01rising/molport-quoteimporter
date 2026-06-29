## Overview

QuoteImporter is a small WPF app for loading a Molport quote spreadsheet, validating product rows, and showing the processed rows in a review grid. Selecting a row shows the full row details, including matching shipping limitation data from the workbook's second worksheet.

## Requirements

- .NET 8 SDK
- Windows, because the UI uses WPF
- ClosedXML, restored automatically from the project package reference

## How to run

```bash
dotnet build
dotnet run
```

## How to run tests

```bash
dotnet run --project Tests\QuoteImporter.Tests\QuoteImporter.Tests.csproj
```

The test project uses a console runner and currently covers validation rules plus a sample workbook read.

## Project structure

- `Models/QuoteDocument.cs` combines quote metadata, product rows, and shipping limitation rows.
- `Models/QuoteItem.cs` represents one product row from the quote spreadsheet.
- `Models/QuoteMetadata.cs` stores quote-level fields such as quotation number and addresses.
- `Models/QuoteShippingInfo.cs` stores shipping and hazard fields from the second worksheet.
- `Services/ExcelQuoteReader.cs` coordinates workbook loading.
- `Services/ExcelCellReader.cs` reads nullable text and numeric cell values.
- `Services/ExcelHeaderReader.cs` detects header rows and maps headers to columns.
- `Services/QuoteMetadataReader.cs` reads quote-level metadata from the worksheet header area.
- `Services/ShippingLimitationsReader.cs` reads the `Shipping Limitations` worksheet and matches rows by Molport ID.
- `Services/QuoteValidator.cs` applies row validation rules.
- `Services/QuoteProcessor.cs` creates summary totals for the loaded quote.
- `MainWindow.xaml` and `MainWindow.xaml.cs` provide the current WPF review screen.
- `Tests/QuoteImporter.Tests` contains automated tests.

## Features

- Select and load an `.xlsx` quote file.
- Read product rows from the `Molecules` worksheet.
- Search for the product header row and map columns by header text, with fallback columns for the known Molport layout(hardcoded).
- Read quote metadata such as quotation number, valid date, shipping address, and billing address by searching label text.
- Read the `Shipping Limitations` worksheet and attach matching data to product rows by Molport ID.
- Stop before footer totals when the line-number column no longer looks like a product line number.
- Validate required fields, prices, quantities, and net price calculations.
- Display loaded rows and validation errors in a WPF `DataGrid`.
- Show all product and shipping limitation details when a row is selected.
- Simulate the final import action with a summary dialog.

## Validation rules

- Molport ID, product ID, supplier, catalogue number, unit, quantity, unit price, and net price are required.
- Quantity should be greater than zero.
- Unit price and net price should not be negative.
- Discounts may be negative.
- Net price should match `Unit Price x Quantity + Discount` within a one-cent tolerance.

## AI usage

- AI assistance was used to:
- scaffold the initial project structure with folders, class skeletons, method signatures, and TODO comments;
- maintain explanatory comments;
- planned and seperated the application into different parts and what they should do
- generating the simple unit tests at the end for some automated testing



## Improvements with more time

- Adding more automated tests for Excel parsing, validation rules, malformed workbooks, and edge cases.
- Add visual styling for invalid grid rows, such as highlighting rows with validation errors.
- Improve UI with some more styling so it doenst look so plain, for example different window for the row details.
- Adding export functionality for invalid rows so users could review and fix quote issues more easily.
