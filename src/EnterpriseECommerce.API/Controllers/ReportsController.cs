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
[Route("api/admin/reports")]
[Authorize(
    Policy = PermissionNames.ViewReports)]
public class ReportsController :
    ControllerBase
{
    private readonly ReportService
        _reportService;

    // ========================================================
    // CONSTRUCTOR
    // ========================================================

    public ReportsController(
        ReportService reportService)
    {
        _reportService =
            reportService;
    }

    // ========================================================
    // GET DASHBOARD REPORT
    // ========================================================
    //
    // GET:
    //
    // /api/admin/reports
    //
    // Optional:
    //
    // /api/admin/reports
    // ?from=2026-08-01
    // &to=2026-08-31
    //
    // ========================================================

    [HttpGet]
    public async Task<
        ActionResult<ReportDashboardDto>>
        GetDashboard(
            [FromQuery]
            DateTime? from = null,

            [FromQuery]
            DateTime? to = null)
    {
        var validation =
            ValidateDates(
                from,
                to);

        if (validation is not null)
        {
            return validation;
        }

        var report =
            await _reportService
                .GetDashboardAsync(
                    from,
                    to);

        return Ok(
            report);
    }

    // ========================================================
    // EXPORT CSV
    // ========================================================
    //
    // GET:
    //
    // /api/admin/reports/export/csv
    //
    // ========================================================

    [HttpGet("export/csv")]
    public async Task<IActionResult>
        ExportCsv(
            [FromQuery]
            DateTime? from = null,

            [FromQuery]
            DateTime? to = null)
    {
        var validation =
            ValidateDates(
                from,
                to);

        if (validation is not null)
        {
            return validation;
        }

        var orders =
            await _reportService
                .GetOrderExportAsync(
                    from,
                    to);

        var csv =
            new StringBuilder();

        // ====================================================
        // CSV HEADERS
        // ====================================================

        csv.AppendLine(
            string.Join(
                ",",
                new[]
                {
                    "Order Number",
                    "Products",
                    "Total Quantity",
                    "Date",
                    "Order Status",
                    "Payment Status",
                    "Amount"
                }));

        // ====================================================
        // CSV DATA
        // ====================================================

        foreach (var order in orders)
        {
            csv.AppendLine(
                string.Join(
                    ",",

                    EscapeCsv(
                        order.OrderNumber),

                    EscapeCsv(
                        order.ProductNames),

                    order.TotalQuantity
                        .ToString(
                            CultureInfo.InvariantCulture),

                    EscapeCsv(
                        order.CreatedAt
                            .ToString(
                                "yyyy-MM-dd HH:mm:ss")),

                    EscapeCsv(
                        order.Status),

                    EscapeCsv(
                        order.PaymentStatus),

                    order.TotalAmount
                        .ToString(
                            "0.00",
                            CultureInfo.InvariantCulture)));
        }

        // UTF-8 BOM helps Excel display
        // Unicode characters correctly.

        var bytes =
            Encoding.UTF8.GetBytes(
                "\uFEFF" +
                csv);

        return File(
            bytes,
            "text/csv",
            BuildFileName(
                "orders-report",
                "csv",
                from,
                to));
    }

    // ========================================================
    // EXPORT EXCEL
    // ========================================================
    //
    // GET:
    //
    // /api/admin/reports/export/excel
    //
    // ========================================================

    [HttpGet("export/excel")]
    public async Task<IActionResult>
        ExportExcel(
            [FromQuery]
            DateTime? from = null,

            [FromQuery]
            DateTime? to = null)
    {
        var validation =
            ValidateDates(
                from,
                to);

        if (validation is not null)
        {
            return validation;
        }

        var report =
            await _reportService
                .GetDashboardAsync(
                    from,
                    to);

        var orders =
            await _reportService
                .GetOrderExportAsync(
                    from,
                    to);

        using var workbook =
            new XLWorkbook();

        // ====================================================
        // SUMMARY SHEET
        // ====================================================

        var summary =
            workbook
                .Worksheets
                .Add(
                    "Summary");

        summary.Cell("A1").Value =
            "Enterprise E-Commerce Report";

        summary.Range("A1:B1")
            .Merge();

        summary.Cell("A2").Value =
            BuildPeriodText(
                from,
                to);

        summary.Range("A2:B2")
            .Merge();

        summary.Cell("A4").Value =
            "Metric";

        summary.Cell("B4").Value =
            "Value";

        var summaryHeader =
            summary.Range(
                "A4:B4");

        summaryHeader.Style
            .Font
            .Bold =
            true;

        var summaryRows =
            new List<
                (string Name, object Value)>
            {
                (
                    "Total Revenue",
                    report.TotalRevenue
                ),

                (
                    "Average Order Value",
                    report.AverageOrderValue
                ),

                (
                    "Total Orders",
                    report.TotalOrders
                ),

                (
                    "Pending Orders",
                    report.PendingOrders
                ),

                (
                    "Confirmed Orders",
                    report.ConfirmedOrders
                ),

                (
                    "Processing Orders",
                    report.ProcessingOrders
                ),

                (
                    "Shipped Orders",
                    report.ShippedOrders
                ),

                (
                    "Delivered Orders",
                    report.DeliveredOrders
                ),

                (
                    "Cancelled Orders",
                    report.CancelledOrders
                ),

                (
                    "Total Payments",
                    report.TotalPayments
                ),

                (
                    "Successful Payments",
                    report.SuccessfulPayments
                ),

                (
                    "Pending Payments",
                    report.PendingPayments
                ),

                (
                    "Failed Payments",
                    report.FailedPayments
                ),

                (
                    "Refunded Payments",
                    report.RefundedPayments
                )
            };

        var summaryRow =
            5;

        foreach (
            var item in
            summaryRows)
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

        // Currency formatting.

        summary.Cell("B5")
            .Style
            .NumberFormat
            .Format =
            "₹#,##0.00";

        summary.Cell("B6")
            .Style
            .NumberFormat
            .Format =
            "₹#,##0.00";

        summary.Columns()
            .AdjustToContents();

        // ====================================================
        // ORDERS SHEET
        // ====================================================

        var orderSheet =
            workbook
                .Worksheets
                .Add(
                    "Orders");

        var orderHeaders =
            new[]
            {
                "Order Number",
                "Products",
                "Total Quantity",
                "Created At",
                "Order Status",
                "Payment Status",
                "Amount"
            };

        for (
            var column = 0;
            column <
            orderHeaders.Length;
            column++)
        {
            orderSheet.Cell(
                    1,
                    column + 1)
                .Value =
                orderHeaders[
                    column];
        }

        orderSheet.Range(
                1,
                1,
                1,
                orderHeaders.Length)
            .Style
            .Font
            .Bold =
            true;

        var orderRow =
            2;

        foreach (
            var order in
            orders)
        {
            orderSheet.Cell(
                    orderRow,
                    1)
                .Value =
                order.OrderNumber;

            orderSheet.Cell(
                    orderRow,
                    2)
                .Value =
                order.ProductNames;

            orderSheet.Cell(
                    orderRow,
                    3)
                .Value =
                order.TotalQuantity;

            orderSheet.Cell(
                    orderRow,
                    4)
                .Value =
                order.CreatedAt;

            orderSheet.Cell(
                    orderRow,
                    5)
                .Value =
                order.Status;

            orderSheet.Cell(
                    orderRow,
                    6)
                .Value =
                order.PaymentStatus;

            orderSheet.Cell(
                    orderRow,
                    7)
                .Value =
                order.TotalAmount;

            orderRow++;
        }

        orderSheet
            .Column(4)
            .Style
            .DateFormat
            .Format =
            "dd/MM/yyyy hh:mm AM/PM";

        orderSheet
            .Column(7)
            .Style
            .NumberFormat
            .Format =
            "₹#,##0.00";

        orderSheet.Columns()
            .AdjustToContents();

        // Product names may become long.
        orderSheet.Column(2)
            .Width =
            Math.Min(
                orderSheet
                    .Column(2)
                    .Width,
                60);

        // ====================================================
        // TOP PRODUCTS SHEET
        // ====================================================

        var productsSheet =
            workbook
                .Worksheets
                .Add(
                    "Top Products");

        productsSheet
            .Cell("A1")
            .Value =
            "Product";

        productsSheet
            .Cell("B1")
            .Value =
            "Quantity Sold";

        productsSheet
            .Cell("C1")
            .Value =
            "Revenue";

        productsSheet
            .Range("A1:C1")
            .Style
            .Font
            .Bold =
            true;

        var productRow =
            2;

        foreach (
            var product in
            report.TopProducts)
        {
            productsSheet.Cell(
                    productRow,
                    1)
                .Value =
                product.ProductName;

            productsSheet.Cell(
                    productRow,
                    2)
                .Value =
                product.QuantitySold;

            productsSheet.Cell(
                    productRow,
                    3)
                .Value =
                product.Revenue;

            productRow++;
        }

        productsSheet
            .Column(3)
            .Style
            .NumberFormat
            .Format =
            "₹#,##0.00";

        productsSheet.Columns()
            .AdjustToContents();

        // ====================================================
        // PAYMENT METHODS SHEET
        // ====================================================

        var paymentSheet =
            workbook
                .Worksheets
                .Add(
                    "Payment Methods");

        paymentSheet
            .Cell("A1")
            .Value =
            "Payment Method";

        paymentSheet
            .Cell("B1")
            .Value =
            "Transactions";

        paymentSheet
            .Cell("C1")
            .Value =
            "Successful Amount";

        paymentSheet
            .Range("A1:C1")
            .Style
            .Font
            .Bold =
            true;

        var paymentRow =
            2;

        foreach (
            var paymentMethod in
            report.PaymentMethods)
        {
            paymentSheet.Cell(
                    paymentRow,
                    1)
                .Value =
                paymentMethod.PaymentMethod;

            paymentSheet.Cell(
                    paymentRow,
                    2)
                .Value =
                paymentMethod.Count;

            paymentSheet.Cell(
                    paymentRow,
                    3)
                .Value =
                paymentMethod.Amount;

            paymentRow++;
        }

        paymentSheet
            .Column(3)
            .Style
            .NumberFormat
            .Format =
            "₹#,##0.00";

        paymentSheet.Columns()
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
                "business-report",
                "xlsx",
                from,
                to));
    }

    // ========================================================
    // EXPORT PDF
    // ========================================================
    //
    // GET:
    //
    // /api/admin/reports/export/pdf
    //
    // ========================================================

    [HttpGet("export/pdf")]
    public async Task<IActionResult>
        ExportPdf(
            [FromQuery]
            DateTime? from = null,

            [FromQuery]
            DateTime? to = null)
    {
        var validation =
            ValidateDates(
                from,
                to);

        if (validation is not null)
        {
            return validation;
        }

        // ====================================================
        // QUESTPDF LICENSE
        // ====================================================

        QuestPDF.Settings.License =
            LicenseType.Community;

        var report =
            await _reportService
                .GetDashboardAsync(
                    from,
                    to);

        var orders =
            await _reportService
                .GetOrderExportAsync(
                    from,
                    to);

        // ====================================================
        // GENERATE PDF
        // ====================================================

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
                                    28);

                                page.DefaultTextStyle(
                                    style =>
                                        style.FontSize(
                                            8));

                                // =================================
                                // HEADER
                                // =================================

                                page.Header()
                                    .Column(
                                        column =>
                                        {
                                            column.Item()
                                                .Text(
                                                    "Enterprise E-Commerce Business Report")
                                                .FontSize(
                                                    20)
                                                .Bold();

                                            column.Item()
                                                .PaddingTop(
                                                    4)
                                                .Text(
                                                    BuildPeriodText(
                                                        from,
                                                        to))
                                                .FontSize(
                                                    9);
                                        });

                                // =================================
                                // CONTENT
                                // =================================

                                page.Content()
                                    .PaddingVertical(
                                        15)
                                    .Column(
                                        column =>
                                        {
                                            column.Spacing(
                                                16);

                                            // =========================
                                            // SUMMARY
                                            // =========================

                                            column.Item()
                                                .Row(
                                                    row =>
                                                    {
                                                        row.RelativeItem()
                                                            .Column(
                                                                item =>
                                                                {
                                                                    item.Item()
                                                                        .Text(
                                                                            "Total Revenue")
                                                                        .FontSize(
                                                                            8);

                                                                    item.Item()
                                                                        .Text(
                                                                            $"₹{report.TotalRevenue:N2}")
                                                                        .FontSize(
                                                                            14)
                                                                        .Bold();
                                                                });

                                                        row.RelativeItem()
                                                            .Column(
                                                                item =>
                                                                {
                                                                    item.Item()
                                                                        .Text(
                                                                            "Total Orders")
                                                                        .FontSize(
                                                                            8);

                                                                    item.Item()
                                                                        .Text(
                                                                            report.TotalOrders
                                                                                .ToString())
                                                                        .FontSize(
                                                                            14)
                                                                        .Bold();
                                                                });

                                                        row.RelativeItem()
                                                            .Column(
                                                                item =>
                                                                {
                                                                    item.Item()
                                                                        .Text(
                                                                            "Successful Payments")
                                                                        .FontSize(
                                                                            8);

                                                                    item.Item()
                                                                        .Text(
                                                                            report.SuccessfulPayments
                                                                                .ToString())
                                                                        .FontSize(
                                                                            14)
                                                                        .Bold();
                                                                });

                                                        row.RelativeItem()
                                                            .Column(
                                                                item =>
                                                                {
                                                                    item.Item()
                                                                        .Text(
                                                                            "Delivered Orders")
                                                                        .FontSize(
                                                                            8);

                                                                    item.Item()
                                                                        .Text(
                                                                            report.DeliveredOrders
                                                                                .ToString())
                                                                        .FontSize(
                                                                            14)
                                                                        .Bold();
                                                                });
                                                    });

                                            // =========================
                                            // ORDERS TITLE
                                            // =========================

                                            column.Item()
                                                .PaddingTop(
                                                    6)
                                                .Text(
                                                    "Orders")
                                                .FontSize(
                                                    14)
                                                .Bold();

                                            // =========================
                                            // ORDERS TABLE
                                            // =========================

                                            column.Item()
                                                .Table(
                                                    table =>
                                                    {
                                                        // -----------------
                                                        // COLUMNS
                                                        // -----------------

                                                        table.ColumnsDefinition(
                                                            columns =>
                                                            {
                                                                columns.RelativeColumn(
                                                                    1.6f);

                                                                columns.RelativeColumn(
                                                                    3.2f);

                                                                columns.RelativeColumn(
                                                                    0.6f);

                                                                columns.RelativeColumn(
                                                                    1.5f);

                                                                columns.RelativeColumn(
                                                                    1.2f);

                                                                columns.RelativeColumn(
                                                                    1.2f);

                                                                columns.RelativeColumn(
                                                                    1.1f);
                                                            });

                                                        // -----------------
                                                        // HEADER
                                                        // -----------------

                                                        table.Header(
                                                            header =>
                                                            {
                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Order")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Products")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Qty")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Date")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Order Status")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Payment")
                                                                    .Bold();

                                                                header.Cell()
                                                                    .Background(
                                                                        Colors.Grey.Lighten2)
                                                                    .Padding(
                                                                        5)
                                                                    .Text(
                                                                        "Amount")
                                                                    .Bold();
                                                            });

                                                        // -----------------
                                                        // DATA
                                                        // -----------------

                                                        foreach (
                                                            var order in
                                                            orders)
                                                        {
                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.OrderNumber);

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.ProductNames);

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.TotalQuantity
                                                                        .ToString());

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.CreatedAt
                                                                        .ToString(
                                                                            "dd/MM/yyyy HH:mm"));

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.Status);

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    order.PaymentStatus);

                                                            table.Cell()
                                                                .BorderBottom(
                                                                    1)
                                                                .BorderColor(
                                                                    Colors.Grey.Lighten2)
                                                                .Padding(
                                                                    5)
                                                                .Text(
                                                                    $"₹{order.TotalAmount:N2}");
                                                        }
                                                    });
                                        });

                                // =================================
                                // FOOTER
                                // =================================

                                page.Footer()
                                    .AlignCenter()
                                    .Text(
                                        text =>
                                        {
                                            text.Span(
                                                "Generated: ");

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

        // ====================================================
        // RETURN PDF
        // ====================================================

        return File(
            pdf,
            "application/pdf",
            BuildFileName(
                "business-report",
                "pdf",
                from,
                to));
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
    // VALIDATE DATE RANGE
    // ========================================================

    private BadRequestObjectResult?
        ValidateDates(
            DateTime? from,
            DateTime? to)
    {
        if (
            from.HasValue &&
            to.HasValue &&
            from.Value.Date >
            to.Value.Date)
        {
            return BadRequest(
                new
                {
                    message =
                        "From date cannot be after To date."
                });
        }

        return null;
    }

    // ========================================================
    // BUILD EXPORT FILE NAME
    // ========================================================

    private static string BuildFileName(
        string prefix,
        string extension,
        DateTime? from,
        DateTime? to)
    {
        string datePart;

        if (
            from.HasValue ||
            to.HasValue)
        {
            var fromValue =
                from?.ToString(
                    "yyyyMMdd")
                ?? "start";

            var toValue =
                to?.ToString(
                    "yyyyMMdd")
                ?? "today";

            datePart =
                $"{fromValue}-{toValue}";
        }
        else
        {
            datePart =
                DateTime.UtcNow
                    .ToString(
                        "yyyyMMdd-HHmmss");
        }

        return
            $"{prefix}-{datePart}.{extension}";
    }

    // ========================================================
    // REPORT PERIOD TEXT
    // ========================================================

    private static string BuildPeriodText(
        DateTime? from,
        DateTime? to)
    {
        if (
            !from.HasValue &&
            !to.HasValue)
        {
            return
                "Period: All Time";
        }

        var fromValue =
            from?.ToString(
                "dd/MM/yyyy")
            ?? "Beginning";

        var toValue =
            to?.ToString(
                "dd/MM/yyyy")
            ?? "Today";

        return
            $"Period: {fromValue} to {toValue}";
    }
}