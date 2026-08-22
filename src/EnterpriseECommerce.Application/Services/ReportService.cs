using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

public class ReportService
{
    private readonly IOrderRepository
        _orderRepository;

    private readonly IPaymentRepository
        _paymentRepository;

    public ReportService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository)
    {
        _orderRepository =
            orderRepository;

        _paymentRepository =
            paymentRepository;
    }

    // ========================================================
    // DASHBOARD REPORT
    // ========================================================

    public async Task<ReportDashboardDto>
        GetDashboardAsync(
            DateTime? from = null,
            DateTime? to = null)
    {
        var orders =
            (
                await _orderRepository
                    .GetAllAsync()
            )
            .ToList();

        var payments =
            (
                await _paymentRepository
                    .GetAllAsync()
            )
            .ToList();

        // ====================================================
        // DATE FILTER
        // ====================================================

        FilterByDate(
            ref orders,
            ref payments,
            from,
            to);

        // ====================================================
        // PAYMENT GROUPS
        // ====================================================

        var successfulPayments =
            payments
                .Where(
                    payment =>
                        IsPaymentStatus(
                            payment,
                            "Success",
                            "Successful"))
                .ToList();

        var pendingPayments =
            payments
                .Where(
                    payment =>
                        IsPaymentStatus(
                            payment,
                            "Pending"))
                .ToList();

        var failedPayments =
            payments
                .Where(
                    payment =>
                        IsPaymentStatus(
                            payment,
                            "Failed"))
                .ToList();

        var refundedPayments =
            payments
                .Where(
                    payment =>
                        IsPaymentStatus(
                            payment,
                            "Refunded"))
                .ToList();

        // ====================================================
        // REVENUE
        // ====================================================

        var totalRevenue =
            successfulPayments.Sum(
                payment =>
                    payment.Amount);

        // ====================================================
        // AVERAGE ORDER VALUE
        // ====================================================

        var averageOrderValue =
            orders.Count == 0
                ? 0
                : orders.Average(
                    order =>
                        order.TotalAmount);

        // ====================================================
        // TOP PRODUCTS
        // ====================================================

        var topProducts =
            orders
                .Where(
                    order =>
                        !IsOrderStatus(
                            order,
                            "Cancelled"))
                .SelectMany(
                    order =>
                        order.OrderItems)
                .GroupBy(
                    item =>
                        new
                        {
                            item.ProductId,
                            item.ProductName
                        })
                .Select(
                    group =>
                        new TopProductReportDto
                        {
                            ProductId =
                                group.Key.ProductId,

                            ProductName =
                                group.Key.ProductName,

                            QuantitySold =
                                group.Sum(
                                    item =>
                                        item.Quantity),

                            Revenue =
                                group.Sum(
                                    item =>
                                        item.TotalPrice)
                        })
                .OrderByDescending(
                    item =>
                        item.QuantitySold)
                .ThenByDescending(
                    item =>
                        item.Revenue)
                .Take(10)
                .ToList();

        // ====================================================
        // PAYMENT METHODS
        // ====================================================

        var paymentMethods =
            payments
                .GroupBy(
                    payment =>
                        string.IsNullOrWhiteSpace(
                            payment.PaymentMethod)
                            ? "Unknown"
                            : payment.PaymentMethod)
                .Select(
                    group =>
                        new PaymentMethodReportDto
                        {
                            PaymentMethod =
                                group.Key,

                            Count =
                                group.Count(),

                            Amount =
                                group
                                    .Where(
                                        payment =>
                                            IsPaymentStatus(
                                                payment,
                                                "Success",
                                                "Successful"))
                                    .Sum(
                                        payment =>
                                            payment.Amount)
                        })
                .OrderByDescending(
                    item =>
                        item.Count)
                .ToList();

        // ====================================================
        // RECENT ORDERS
        // ====================================================

        var recentOrders =
            orders
                .OrderByDescending(
                    order =>
                        order.CreatedAt)
                .Take(50)
                .Select(
                    order =>
                        MapRecentOrder(
                            order))
                .ToList();

        // ====================================================
        // RESULT
        // ====================================================

        return new ReportDashboardDto
        {
            TotalRevenue =
                totalRevenue,

            AverageOrderValue =
                averageOrderValue,

            TotalOrders =
                orders.Count,

            PendingOrders =
                CountOrders(
                    orders,
                    "Pending"),

            ConfirmedOrders =
                CountOrders(
                    orders,
                    "Confirmed"),

            ProcessingOrders =
                CountOrders(
                    orders,
                    "Processing"),

            ShippedOrders =
                CountOrders(
                    orders,
                    "Shipped"),

            DeliveredOrders =
                CountOrders(
                    orders,
                    "Delivered"),

            CancelledOrders =
                CountOrders(
                    orders,
                    "Cancelled"),

            TotalPayments =
                payments.Count,

            SuccessfulPayments =
                successfulPayments.Count,

            PendingPayments =
                pendingPayments.Count,

            FailedPayments =
                failedPayments.Count,

            RefundedPayments =
                refundedPayments.Count,

            TopProducts =
                topProducts,

            PaymentMethods =
                paymentMethods,

            RecentOrders =
                recentOrders
        };
    }

    // ========================================================
    // GET EXPORT ORDERS
    // ========================================================

    public async Task<
        IReadOnlyList<RecentOrderReportDto>>
        GetOrderExportAsync(
            DateTime? from = null,
            DateTime? to = null)
    {
        var orders =
            (
                await _orderRepository
                    .GetAllAsync()
            )
            .ToList();

        var payments =
            (
                await _paymentRepository
                    .GetAllAsync()
            )
            .ToList();

        FilterByDate(
            ref orders,
            ref payments,
            from,
            to);

        return orders
            .OrderByDescending(
                order =>
                    order.CreatedAt)
            .Select(
                MapRecentOrder)
            .ToList();
    }

    // ========================================================
    // MAP ORDER
    // ========================================================

    private static RecentOrderReportDto
        MapRecentOrder(
            Order order)
    {
        var items =
            order.OrderItems
                .Select(
                    item =>
                        new ReportOrderItemDto
                        {
                            ProductId =
                                item.ProductId,

                            ProductName =
                                item.ProductName,

                            Quantity =
                                item.Quantity,

                            UnitPrice =
                                item.UnitPrice,

                            TotalPrice =
                                item.TotalPrice
                        })
                .ToList();

        var productNames =
            items.Count == 0
                ? "-"
                : string.Join(
                    ", ",
                    items.Select(
                        item =>
                            item.Quantity > 1
                                ? $"{item.ProductName} × {item.Quantity}"
                                : item.ProductName));

        return new RecentOrderReportDto
        {
            OrderId =
                order.Id,

            OrderNumber =
                order.OrderNumber,

            ProductNames =
                productNames,

            TotalQuantity =
                items.Sum(
                    item =>
                        item.Quantity),

            Items =
                items,

            TotalAmount =
                order.TotalAmount,

            Status =
                order.Status
                    .ToString(),

            PaymentStatus =
                order.PaymentStatus
                    .ToString(),

            CreatedAt =
                order.CreatedAt
        };
    }

    // ========================================================
    // DATE FILTER
    // ========================================================

    private static void FilterByDate(
        ref List<Order> orders,
        ref List<Payment> payments,
        DateTime? from,
        DateTime? to)
    {
        if (from.HasValue)
        {
            var fromUtc =
                NormalizeUtc(
                    from.Value);

            orders =
                orders
                    .Where(
                        order =>
                            order.CreatedAt >=
                            fromUtc)
                    .ToList();

            payments =
                payments
                    .Where(
                        payment =>
                            payment.CreatedAt >=
                            fromUtc)
                    .ToList();
        }

        if (to.HasValue)
        {
            var toUtc =
                NormalizeUtc(
                    to.Value);

            var exclusiveEnd =
                toUtc.Date
                    .AddDays(1);

            orders =
                orders
                    .Where(
                        order =>
                            order.CreatedAt <
                            exclusiveEnd)
                    .ToList();

            payments =
                payments
                    .Where(
                        payment =>
                            payment.CreatedAt <
                            exclusiveEnd)
                    .ToList();
        }
    }

    // ========================================================
    // ORDER STATUS
    // ========================================================

    private static int CountOrders(
        IEnumerable<Order> orders,
        string status)
    {
        return orders.Count(
            order =>
                IsOrderStatus(
                    order,
                    status));
    }

    private static bool IsOrderStatus(
        Order order,
        string status)
    {
        return string.Equals(
            order.Status.ToString(),
            status,
            StringComparison.OrdinalIgnoreCase);
    }

    // ========================================================
    // PAYMENT STATUS
    // ========================================================

    private static bool IsPaymentStatus(
        Payment payment,
        params string[] statuses)
    {
        var value =
            payment.Status
                .ToString();

        return statuses.Any(
            status =>
                string.Equals(
                    value,
                    status,
                    StringComparison.OrdinalIgnoreCase));
    }

    // ========================================================
    // UTC
    // ========================================================

    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }
}