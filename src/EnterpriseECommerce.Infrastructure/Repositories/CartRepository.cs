using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseECommerce.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET CART BY USER
    // ============================================================

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.UserId == userId);
    }

    // ============================================================
    // GET CART BY ID
    // ============================================================

    public async Task<Cart?> GetByIdAsync(Guid id)
    {
        return await _context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.Id == id);
    }

    // ============================================================
    // ADD CART
    // ============================================================

    public async Task AddAsync(Cart cart)
    {
        if (cart == null)
        {
            throw new ArgumentNullException(nameof(cart));
        }

        await _context.Carts.AddAsync(cart);

        await _context.SaveChangesAsync();
    }

    // ============================================================
    // UPDATE CART
    // ============================================================

    public async Task UpdateAsync(Cart cart)
    {
        if (cart == null)
        {
            throw new ArgumentNullException(nameof(cart));
        }

        Console.WriteLine();
        Console.WriteLine("========== CART UPDATE DEBUG ==========");

        Console.WriteLine($"Cart Id: {cart.Id}");
        Console.WriteLine($"Cart UserId: {cart.UserId}");
        Console.WriteLine($"Cart Items Count: {cart.Items.Count}");

        // --------------------------------------------------------
        // Show EF Core tracking state BEFORE SaveChanges.
        // --------------------------------------------------------

        foreach (var entry in _context.ChangeTracker.Entries())
        {
            Console.WriteLine(
                $"ENTITY: {entry.Entity.GetType().Name} | " +
                $"STATE: {entry.State}");

            if (entry.Entity is Cart trackedCart)
            {
                Console.WriteLine(
                    $"  Cart Id: {trackedCart.Id}");
            }

            if (entry.Entity is CartItem trackedItem)
            {
                Console.WriteLine(
                    $"  CartItem Id: {trackedItem.Id}");

                Console.WriteLine(
                    $"  CartId: {trackedItem.CartId}");

                Console.WriteLine(
                    $"  ProductId: {trackedItem.ProductId}");

                Console.WriteLine(
                    $"  Quantity: {trackedItem.Quantity}");
            }
        }

        Console.WriteLine("========================================");
        Console.WriteLine();

        // --------------------------------------------------------
        // Cart was loaded by GetByUserIdAsync(), therefore it is
        // already tracked by this DbContext.
        //
        // Cart.AddItem() modifies the tracked entity graph.
        // EF Core detects those changes automatically.
        // --------------------------------------------------------

        await _context.SaveChangesAsync();
    }
}
