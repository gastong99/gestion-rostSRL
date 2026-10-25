
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Data;
using TransformadoresApp.Models;

public class BomService
{
    private readonly ApplicationDbContext _db;

    public BomService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<(Material material, decimal? totalQty)>> CalculateForProductAsync(int productId, decimal? qty)
    {
        var lines = await _db.BomItems.Include(b => b.Material).ThenInclude(m => m!.UnitOfMeasure).Where(b => b.ProductId == productId).ToListAsync();

        var result = new List<(Material material, decimal? totalQty)>();

        foreach (var line in lines) {

            if (line.Material is null) continue;

            var total = line.QuantityPerUnit * qty;

            result.Add((line.Material, total));
        }

        return result;
    }
}