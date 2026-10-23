using Ecommerce.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Ecommerce.Web.Repositories;

public class ProductRepository
{
    private readonly IMongoCollection<Product> products;

    public ProductRepository(IMongoDatabase database)
    {
        products = database.GetCollection<Product>("sanpham");
    }

    public Task<bool> HasCategory(string categoryId)
    {
        return products.Find(x => x.CategoryId == categoryId).AnyAsync();
    }

    public Task<bool> SkuExists(string sku, string? excludedId)
    {
        return products.Find(x => x.Sku == sku && x.Id != excludedId).AnyAsync();
    }

    public async Task<ProductPage> GetPage(string? query, string? categoryId, int page, bool includeInactive)
    {
        var builder = Builders<Product>.Filter;
        var filter = includeInactive ? builder.Empty : builder.Eq(x => x.IsActive, true);
        if (!string.IsNullOrWhiteSpace(query))
        {
            string text = query.Trim();
            text = text.Substring(0, Math.Min(text.Length, 100));
            filter &= builder.Regex(x => x.Name, new BsonRegularExpression(Regex.Escape(text), "i"));
        }
        if (!string.IsNullOrEmpty(categoryId))
            filter &= builder.Eq(x => x.CategoryId, categoryId);

        const int pageSize = 12;
        long total = await products.CountDocumentsAsync(filter);
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        var items = await products.Find(filter).SortBy(x => x.Name).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        return new ProductPage(items, total, page, pageSize);
    }

    public Task<Product?> GetById(string id, IClientSessionHandle? session = null, CancellationToken cancellationToken = default)
    {
        var query = session == null
            ? products.Find(x => x.Id == id)
            : products.Find(session, x => x.Id == id);
        return query.FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task Create(Product product)
    {
        return products.InsertOneAsync(product);
    }

    public async Task<bool> Update(Product product, int originalStock)
    {
        var result = await products.ReplaceOneAsync(x => x.Id == product.Id && x.Stock == originalStock, product);
        return result.MatchedCount == 1;
    }

    public async Task Deactivate(string id)
    {
        await products.UpdateOneAsync(x => x.Id == id, Builders<Product>.Update.Set(x => x.IsActive, false));
    }

    // Các thao tác tồn kho dùng cùng session với đơn hàng để rollback cùng nhau.
    public async Task<bool> DecreaseStock(string id, int quantity, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        var result = await products.UpdateOneAsync(session,
            x => x.Id == id && x.IsActive && x.Stock >= quantity,
            Builders<Product>.Update.Inc(x => x.Stock, -quantity), cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }

    public async Task<bool> RestoreStock(string id, int quantity, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        var result = await products.UpdateOneAsync(session, x => x.Id == id,
            Builders<Product>.Update.Inc(x => x.Stock, quantity), cancellationToken: cancellationToken);
        return result.MatchedCount == 1;
    }
}
