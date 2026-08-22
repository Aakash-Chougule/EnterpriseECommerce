namespace EnterpriseECommerce.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } =
        string.Empty;

    // ========================================================
    // PRODUCT SNAPSHOT
    // ========================================================

    public string SKU { get; private set; } =
        string.Empty;

    public string HsnCode { get; private set; } =
        string.Empty;

    public int Quantity { get; private set; }

    /// <summary>
    /// GST-inclusive selling price per unit.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    public decimal GstRate { get; private set; }

    // ========================================================
    // GST BREAKDOWN
    // ========================================================

    public decimal TaxableAmount { get; private set; }

    public decimal GstAmount { get; private set; }

    public decimal CgstAmount { get; private set; }

    public decimal SgstAmount { get; private set; }

    public decimal IgstAmount { get; private set; }

    // ========================================================
    // LINE TOTAL
    // ========================================================

    /// <summary>
    /// Final GST-inclusive line total.
    /// </summary>
    public decimal TotalPrice { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(
        Guid productId,
        string productName,
        string sku,
        string? hsnCode,
        int quantity,
        decimal unitPrice,
        decimal gstRate,
        bool isInterState)
    {
        if (productId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (string.IsNullOrWhiteSpace(
            productName))
        {
            throw new ArgumentException(
                "Product name is required.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException(
                "Unit price cannot be negative.");
        }

        if (gstRate < 0 ||
            gstRate > 100)
        {
            throw new ArgumentException(
                "GST rate must be between 0 and 100.");
        }

        Id =
            Guid.NewGuid();

        ProductId =
            productId;

        ProductName =
            productName.Trim();

        SKU =
            sku?.Trim() ??
            string.Empty;

        HsnCode =
            hsnCode?.Trim() ??
            string.Empty;

        Quantity =
            quantity;

        UnitPrice =
            unitPrice;

        GstRate =
            gstRate;

        CalculateTax(
            isInterState);
    }

    // ========================================================
    // TAX CALCULATION
    // ========================================================
    //
    // Price is GST inclusive.
    //
    // Formula:
    //
    // Taxable Amount =
    // Gross × 100 / (100 + GST Rate)
    //
    // GST =
    // Gross - Taxable Amount
    //
    // ========================================================

    private void CalculateTax(
        bool isInterState)
    {
        var grossAmount =
            Math.Round(
                UnitPrice *
                Quantity,
                2,
                MidpointRounding.AwayFromZero);

        TotalPrice =
            grossAmount;

        if (GstRate <= 0)
        {
            TaxableAmount =
                grossAmount;

            GstAmount =
                0;

            CgstAmount =
                0;

            SgstAmount =
                0;

            IgstAmount =
                0;

            return;
        }

        TaxableAmount =
            Math.Round(
                grossAmount *
                100m /
                (100m + GstRate),
                2,
                MidpointRounding.AwayFromZero);

        GstAmount =
            Math.Round(
                grossAmount -
                TaxableAmount,
                2,
                MidpointRounding.AwayFromZero);

        if (isInterState)
        {
            IgstAmount =
                GstAmount;

            CgstAmount =
                0;

            SgstAmount =
                0;
        }
        else
        {
            CgstAmount =
                Math.Round(
                    GstAmount / 2m,
                    2,
                    MidpointRounding.AwayFromZero);

            SgstAmount =
                GstAmount -
                CgstAmount;

            IgstAmount =
                0;
        }
    }
}