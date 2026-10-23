using Ecommerce.Web.Models;
using MongoDB.Driver;

namespace Ecommerce.Web.Repositories;

public class OrderRepository
{
    private readonly IMongoCollection<Order> orders;

    public OrderRepository(IMongoDatabase database)
    {
        orders = database.GetCollection<Order>("hoadon");
    }

    public Task<Order?> GetById(string id, IClientSessionHandle? session = null, CancellationToken cancellationToken = default)
    {
        var query = session == null ? orders.Find(x => x.Id == id) : orders.Find(session, x => x.Id == id);
        return query.FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task<Order?> FindCheckout(string key, string token, IClientSessionHandle? session = null, CancellationToken cancellationToken = default)
    {
        var query = session == null
            ? orders.Find(x => x.CheckoutKey == key && x.CustomerToken == token)
            : orders.Find(session, x => x.CheckoutKey == key && x.CustomerToken == token);
        return query.FirstOrDefaultAsync(cancellationToken)!;
    }

    public async Task<OrderPage> GetPage(int page, string? status)
    {
        var filter = string.IsNullOrEmpty(status) ? Builders<Order>.Filter.Empty : Builders<Order>.Filter.Eq(x => x.Status, status);
        const int pageSize = 15;
        long total = await orders.CountDocumentsAsync(filter);
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        var items = await orders.Find(filter).SortByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        return new OrderPage(items, total, page, pageSize);
    }

    public Task Create(Order order, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        return orders.InsertOneAsync(session, order, cancellationToken: cancellationToken);
    }

    public async Task<bool> ChangePendingStatus(string id, string status, IClientSessionHandle session, CancellationToken cancellationToken)
    {
        var result = await orders.UpdateOneAsync(session, x => x.Id == id && x.Status == "Pending",
            Builders<Order>.Update.Set(x => x.Status, status), cancellationToken: cancellationToken);
        return result.ModifiedCount == 1;
    }
}
