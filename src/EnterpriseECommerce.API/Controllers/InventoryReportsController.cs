using System.Globalization;
using System.Text;

using ClosedXML.Excel;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Security;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/admin/inventory-report")]
[Authorize(
    Policy = PermissionNames.ManageInventory)]
public class InventoryReportsController :
    ControllerBase
{
    private readonly InventoryReportService
        _inventoryReportService;

    public InventoryReportsController(
        InventoryReportService inventoryReportService)
    {
        _inventoryReportService =
            inventoryReportService;
    }

    // ========================================================
    // GET INVENTORY REPORT
    // ========================================================

    [HttpGet]
    public async Task<
        ActionResult<InventoryReportDto>>
        GetReport(
            [FromQuery]
            int threshold = 5)
    {
        try
        {
            var report =
                await _inventoryReportService
                    .GetInventoryReportAsync(
                        threshold);

            return Ok(
                report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ========================================================
    // EXPORT CSV
    // ========================================================

    [HttpGet("export/csv")]
    public async Task<IActionResult>
        ExportCsv(
            [FromQuery]
            int threshold = 5)
    {
        try
        {
            var report =
                await _inventoryReportService
                    .GetInventoryReportAsync(
                        threshold);

            var csv =
                new StringBuilder();

            // UTF-8 BOM
            csv.Append(
                '\uFEFF');

            csv.AppendLine(
                "Product Name,SKU,Category,Unit Price,Current Stock,Stock Value,Stock Status,Product Status");

            foreach (
                var product in
                report.Products)
            {
                csv.AppendLine(
                    string.Join(
                        ",",
                        EscapeCsv(
                            product.ProductName),
                        EscapeCsv(
                            product.SKU),
                        EscapeCsv(
                            product.CategoryName),
                        product.UnitPrice
                            .ToString(
                                "0.00",
                                CultureInfo.InvariantCulture),
                        product.StockQuantity
                            .ToString(
                                CultureInfo.InvariantCulture),
                        product.StockValue
                            .ToString(
                                "0.00",
                                CultureInfo.InvariantCulture),
                        EscapeCsv(
                            product.StockStatus),
                        EscapeCsv(
                            product.IsActive
                                ? "Active"
                                : "Inactive")));
            }

            var bytes =
                Encoding.UTF8.GetBytes(
                    csv.ToString());

            return File(
                bytes,
                "text/csv",
                BuildFileName(
                    "inventory-report",
                    "csv"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ========================================================
    // EXPORT EXCEL
    // ========================================================

    [HttpGet("export/excel")]
    public async Task<IActionResult>
        ExportExcel(
            [FromQuery]
            int threshold = 5)
    {
        try
        {
            var report =
                await _inventoryReportService
                    .GetInventoryReportAsync(
                        threshold);

            using var workbook =
                new XLWorkbook();

            // ====================================================
            // SHEET 1: INVENTORY SUMMARY
            // ====================================================

            var summary =
                workbook
                    .Worksheets
                    .Add(
                        "Inventory Summary");

            summary.Cell("A1").Value =
                "Enterprise E-Commerce Inventory Report";

            summary.Range("A1:B1")
                .Merge();

            summary.Cell("A1")
                .Style
                .Font
                .Bold =
                true;

            summary.Cell("A1")
                .Style
                .Font
                .FontSize =
                16;

            summary.Cell("A2").Value =
                $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}";

            summary.Range("A2:B2")
                .Merge();

            summary.Cell("A4").Value =
                "Metric";

            summary.Cell("B4").Value =
                "Value";

            summary.Range("A4:B4")
                .Style
                .Font
                .Bold =
                true;

            var summaryData =
                new List<
                    (string Name, object Value)>
                {
                    (
                        "Total Products",
                        report.TotalProducts
                    ),

                    (
                        "Active Products",
                        report.ActiveProducts
                    ),

                    (
                        "Inactive Products",
                        report.InactiveProducts
                    ),

                    (
                        "Total Units",
                        report.TotalUnits
                    ),

                    (
                        "Inventory Value",
                        report.TotalInventoryValue
                    ),

                    (
                        "In Stock Products",
                        report.InStockProducts
                    ),

                    (
                        "Low Stock Products",
                        report.LowStockProducts
                    ),

                    (
                        "Out of Stock Products",
                        report.OutOfStockProducts
                    ),

                    (
                        "Low Stock Threshold",
                        report.Threshold
                    )
                };

            var summaryRow =
                5;

            foreach (
                var item in
                summaryData)
            {
                summary.Cell(
                        summaryRow,
                        1)
                    .Value =
                    item.Name;

                summary.Cell(
                        summaryRow,
                        2)
                    .Value =
                    XLCellValue.FromObject(
                        item.Value);

                summaryRow++;
            }

            // Inventory Value row.
            summary.Cell("B9")
                .Style
                .NumberFormat
                .Format =
                "₹#,##0.00";

            summary.Columns()
                .AdjustToContents();

            // ====================================================
            // SHEET 2: PRODUCT INVENTORY
            // ====================================================

            var products =
                workbook
                    .Worksheets
                    .Add(
                        "Product Inventory");

            var productHeaders =
                new[]
                {
                    "Product Name",
                    "SKU",
                    "Category",
                    "Unit Price",
                    "Current Stock",
                    "Stock Value",
                    "Stock Status",
                    "Product Status"
                };

            for (
                var column = 0;
                column <
                productHeaders.Length;
                column++)
            {
                products.Cell(
                        1,
                        column + 1)
                    .Value =
                    productHeaders[
                        column];
            }

            products.Range(
                    1,
                    1,
                    1,
                    productHeaders.Length)
                .Style
                .Font
                .Bold =
                true;

            var productRow =
                2;

            foreach (
                var product in
                report.Products)
            {
                products.Cell(
                        productRow,
                        1)
                    .Value =
                    product.ProductName;

                products.Cell(
                        productRow,
                        2)
                    .Value =
                    product.SKU;

                products.Cell(
                        productRow,
                        3)
                    .Value =
                    product.CategoryName;

                products.Cell(
                        productRow,
                        4)
                    .Value =
                    product.UnitPrice;

                products.Cell(
                        productRow,
                        5)
                    .Value =
                    product.StockQuantity;

                products.Cell(
                        productRow,
                        6)
                    .Value =
                    product.StockValue;

                products.Cell(
                        productRow,
                        7)
                    .Value =
                    product.StockStatus;

                products.Cell(
                        productRow,
                        8)
                    .Value =
                    product.IsActive
                        ? "Active"
                        : "Inactive";

                productRow++;
            }

            products.Column(4)
                .Style
                .NumberFormat
                .Format =
                "₹#,##0.00";

            products.Column(6)
                .Style
                .NumberFormat
                .Format =
                "₹#,##0.00";

            products.Columns()
                .AdjustToContents();

            // ====================================================
            // SHEET 3: CATEGORY SUMMARY
            // ====================================================

            var categories =
                workbook
                    .Worksheets
                    .Add(
                        "Category Summary");

            var categoryHeaders =
                new[]
                {
                    "Category",
                    "Products",
                    "Total Units",
                    "Inventory Value",
                    "In Stock",
                    "Low Stock",
                    "Out of Stock"
                };

            for (
                var column = 0;
                column <
                categoryHeaders.Length;
                column++)
            {
                categories.Cell(
                        1,
                        column + 1)
                    .Value =
                    categoryHeaders[
                        column];
            }

            categories.Range(
                    1,
                    1,
                    1,
                    categoryHeaders.Length)
                .Style
                .Font
                .Bold =
                true;

            var categoryRow =
                2;

            foreach (
                var category in
                report.Categories)
            {
                categories.Cell(
                        categoryRow,
                        1)
                    .Value =
                    category.CategoryName;

                categories.Cell(
                        categoryRow,
                        2)
                    .Value =
                    category.ProductCount;

                categories.Cell(
                        categoryRow,
                        3)
                    .Value =
                    category.TotalUnits;

                categories.Cell(
                        categoryRow,
                        4)
                    .Value =
                    category.InventoryValue;

                categories.Cell(
                        categoryRow,
                        5)
                    .Value =
                    category.InStockProducts;

                categories.Cell(
                        categoryRow,
                        6)
                    .Value =
                    category.LowStockProducts;

                categories.Cell(
                        categoryRow,
                        7)
                    .Value =
                    category.OutOfStockProducts;

                categoryRow++;
            }

            categories.Column(4)
                .Style
                .NumberFormat
                .Format =
                "₹#,##0.00";

            categories.Columns()
                .AdjustToContents();

            // ====================================================
            // SAVE EXCEL
            // ====================================================

            using var stream =
                new MemoryStream();

            workbook.SaveAs(
                stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                BuildFileName(
                    "inventory-report",
                    "xlsx"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ========================================================
    // EXPORT PDF
    // ========================================================

    [HttpGet("export/pdf")]
    public async Task<IActionResult>
        ExportPdf(
            [FromQuery]
            int threshold = 5)
    {
        try
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            var report =
                await _inventoryReportService
                    .GetInventoryReportAsync(
                        threshold);

            var pdf =
                Document
                    .Create(
                        container =>
                        {
                            container.Page(
                                page =>
                                {
                                    page.Size(
                                        PageSizes
                                            .A4
                                            .Landscape());

                                    page.Margin(
                                        25);

                                    page.DefaultTextStyle(
                                        style =>
                                            style.FontSize(
                                                8));

                                    // =================================================
                                    // HEADER
                                    // =================================================

                                    page.Header()
                                        .Column(
                                            column =>
                                            {
                                                column
                                                    .Item()
                                                    .Text(
                                                        "Enterprise E-Commerce Inventory Report")
                                                    .FontSize(
                                                        19)
                                                    .Bold();

                                                column
                                                    .Item()
                                                    .PaddingTop(
                                                        3)
                                                    .Text(
                                                        $"Generated: {DateTime.Now:dd/MM/yyyy HH:mm} | Low Stock Threshold: {report.Threshold}")
                                                    .FontSize(
                                                        8);
                                            });

                                    // =================================================
                                    // CONTENT
                                    // =================================================

                                    page.Content()
                                        .PaddingVertical(
                                            12)
                                        .Column(
                                            column =>
                                            {
                                                column.Spacing(
                                                    14);

                                                // =====================================
                                                // SUMMARY
                                                // =====================================

                                                column
                                                    .Item()
                                                    .Row(
                                                        row =>
                                                        {
                                                            row
                                                                .RelativeItem()
                                                                .Column(
                                                                    item =>
                                                                    {
                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                "Total Products");

                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                report.TotalProducts
                                                                                    .ToString())
                                                                            .FontSize(
                                                                                13)
                                                                            .Bold();
                                                                    });

                                                            row
                                                                .RelativeItem()
                                                                .Column(
                                                                    item =>
                                                                    {
                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                "Total Units");

                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                report.TotalUnits
                                                                                    .ToString())
                                                                            .FontSize(
                                                                                13)
                                                                            .Bold();
                                                                    });

                                                            row
                                                                .RelativeItem()
                                                                .Column(
                                                                    item =>
                                                                    {
                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                "Inventory Value");

                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                $"INR {report.TotalInventoryValue:N2}")
                                                                            .FontSize(
                                                                                13)
                                                                            .Bold();
                                                                    });

                                                            row
                                                                .RelativeItem()
                                                                .Column(
                                                                    item =>
                                                                    {
                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                "Low Stock");

                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                report.LowStockProducts
                                                                                    .ToString())
                                                                            .FontSize(
                                                                                13)
                                                                            .Bold();
                                                                    });

                                                            row
                                                                .RelativeItem()
                                                                .Column(
                                                                    item =>
                                                                    {
                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                "Out of Stock");

                                                                        item
                                                                            .Item()
                                                                            .Text(
                                                                                report.OutOfStockProducts
                                                                                    .ToString())
                                                                            .FontSize(
                                                                                13)
                                                                            .Bold();
                                                                    });
                                                        });

                                                // =====================================
                                                // PRODUCT-WISE TITLE
                                                // =====================================

                                                column
                                                    .Item()
                                                    .PaddingTop(
                                                        4)
                                                    .Text(
                                                        "Product-wise Inventory")
                                                    .FontSize(
                                                        13)
                                                    .Bold();

                                                // =====================================
                                                // PRODUCT TABLE
                                                // =====================================

                                                column
                                                    .Item()
                                                    .Table(
                                                        table =>
                                                        {
                                                            table
                                                                .ColumnsDefinition(
                                                                    columns =>
                                                                    {
                                                                        columns
                                                                            .RelativeColumn(
                                                                                2);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1.2f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1.6f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                .8f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1.2f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1.1f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                .9f);
                                                                    });

                                                            // =================================
                                                            // PRODUCT TABLE HEADER
                                                            // =================================

                                                            table.Header(
                                                                header =>
                                                                {
                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Product");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "SKU");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Category");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Unit Price");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Stock");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Stock Value");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Stock Status");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Status");
                                                                });

                                                            // =================================
                                                            // PRODUCT TABLE ROWS
                                                            // =================================

                                                            foreach (
                                                                var product in
                                                                report.Products)
                                                            {
                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.ProductName);

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.SKU);

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.CategoryName);

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    $"INR {product.UnitPrice:N2}");

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.StockQuantity
                                                                        .ToString());

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    $"INR {product.StockValue:N2}");

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.StockStatus);

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    product.IsActive
                                                                        ? "Active"
                                                                        : "Inactive");
                                                            }
                                                        });

                                                // =====================================
                                                // CATEGORY SUMMARY TITLE
                                                // =====================================

                                                column
                                                    .Item()
                                                    .PaddingTop(
                                                        5)
                                                    .Text(
                                                        "Category Summary")
                                                    .FontSize(
                                                        13)
                                                    .Bold();

                                                // =====================================
                                                // CATEGORY TABLE
                                                // =====================================

                                                column
                                                    .Item()
                                                    .Table(
                                                        table =>
                                                        {
                                                            table
                                                                .ColumnsDefinition(
                                                                    columns =>
                                                                    {
                                                                        columns
                                                                            .RelativeColumn(
                                                                                2);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1.5f);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);

                                                                        columns
                                                                            .RelativeColumn(
                                                                                1);
                                                                    });

                                                            // =================================
                                                            // CATEGORY HEADER
                                                            // =================================

                                                            table.Header(
                                                                header =>
                                                                {
                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Category");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Products");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Units");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Inventory Value");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "In Stock");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Low Stock");

                                                                    AddPdfHeaderCell(
                                                                        header.Cell(),
                                                                        "Out of Stock");
                                                                });

                                                            // =================================
                                                            // CATEGORY ROWS
                                                            // =================================

                                                            foreach (
                                                                var category in
                                                                report.Categories)
                                                            {
                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.CategoryName);

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.ProductCount
                                                                        .ToString());

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.TotalUnits
                                                                        .ToString());

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    $"INR {category.InventoryValue:N2}");

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.InStockProducts
                                                                        .ToString());

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.LowStockProducts
                                                                        .ToString());

                                                                AddPdfBodyCell(
                                                                    table.Cell(),
                                                                    category.OutOfStockProducts
                                                                        .ToString());
                                                            }
                                                        });
                                            });

                                    // =================================================
                                    // FOOTER
                                    // =================================================

                                    page.Footer()
                                        .AlignCenter()
                                        .Text(
                                            text =>
                                            {
                                                text.Span(
                                                    "Generated ");

                                                text.Span(
                                                    DateTime.Now
                                                        .ToString(
                                                            "dd/MM/yyyy HH:mm"));

                                                text.Span(
                                                    "    |    Page ");

                                                text.CurrentPageNumber();

                                                text.Span(
                                                    " of ");

                                                text.TotalPages();
                                            });
                                });
                        })
                    .GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                BuildFileName(
                    "inventory-report",
                    "pdf"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ========================================================
    // PDF HEADER CELL
    // ========================================================
    //
    // IMPORTANT:
    //
    // table.Cell() and header.Cell() return an
    // ITableCellContainer in this QuestPDF version.
    //
    // Do NOT use TableCellDescriptor here.
    // ========================================================

    private static void AddPdfHeaderCell(
        QuestPDF.Elements.Table.ITableCellContainer cell,
        string value)
    {
        cell
            .Background(
                Colors.Grey.Lighten2)
            .Padding(
                5)
            .Text(
                value)
            .Bold();
    }

    // ========================================================
    // PDF BODY CELL
    // ========================================================

    private static void AddPdfBodyCell(
        QuestPDF.Elements.Table.ITableCellContainer cell,
        string value)
    {
        cell
            .BorderBottom(
                1)
            .BorderColor(
                Colors.Grey.Lighten2)
            .Padding(
                5)
            .Text(
                value);
    }

    // ========================================================
    // CSV ESCAPE
    // ========================================================

    private static string EscapeCsv(
        string? value)
    {
        var safe =
            value ??
            string.Empty;

        safe =
            safe.Replace(
                "\"",
                "\"\"");

        return
            $"\"{safe}\"";
    }

    // ========================================================
    // BUILD FILE NAME
    // ========================================================

    private static string BuildFileName(
        string prefix,
        string extension)
    {
        return
            $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";
    }
}