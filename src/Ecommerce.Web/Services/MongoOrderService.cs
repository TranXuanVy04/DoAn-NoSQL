using Ecommerce.Web.Repositories;
using Ecommerce.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Ecommerce.Web.Services;

public sealed class MongoOrderService : IOrderService
{
    private readonly OrderRepository orders;
    private readonly ProductRepository products;
    private readonly CategoryRepository categories;
    private readonly IMongoClient client;

    public MongoOrderService(OrderRepository orders, ProductRepository products, CategoryRepository categories, IMongoClient client)
    {
        this.orders = orders;
        this.products = products;
        this.categories = categories;
        this.client = client;
    }

    public Task<Order?> Get(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return Task.FromResult<Order?>(null);
        return orders.GetById(id);
    }

    public Task<Order?> FindCheckout(string key, string customerToken)
    {
        return orders.FindCheckout(key, customerToken);
    }

    public Task<OrderPage> List(int page, string? status)
    {
        return orders.GetPage(page, status);
    }

    public async Task<Order> Place(IReadOnlyList<CartLine> cart,Customer customer,string key,string token) {
        var existing=await FindCheckout(key,token); if(existing is not null) return existing;
        if(cart.Count is <1 or >50 || cart.Any(x=>x.Quantity is <1 or >99 || !ObjectId.TryParse(x.ProductId,out _)) || cart.Select(x=>x.ProductId).Distinct().Count()!=cart.Count)
            throw new BusinessException("Giỏ hàng không hợp lệ (1–50 dòng, mỗi dòng 1–99 sản phẩm).");
        using var session=await client.StartSessionAsync();
        try {
            return await session.WithTransactionAsync(async (s,ct) => {
                var duplicate=await orders.FindCheckout(key, token, s, ct);
                if(duplicate is not null) return duplicate;
                var order=new Order {CheckoutKey=key,CustomerToken=token,Customer=customer};
                order.Number="EC-"+order.Id.ToUpperInvariant();
                foreach(var row in cart.OrderBy(x=>x.ProductId,StringComparer.Ordinal)) {
                    var p=await products.GetById(row.ProductId, s, ct);
                    if(p is null || !p.IsActive) throw new BusinessException("Một sản phẩm trong giỏ đã ngừng bán.");
                    var cat=await categories.GetById(p.CategoryId, s, ct);
                    if(cat is null) throw new BusinessException("Danh mục sản phẩm không còn tồn tại.");
                    bool stockChanged = await products.DecreaseStock(p.Id, row.Quantity, s, ct);
                    if(!stockChanged) throw new BusinessException($"{p.Name} không đủ tồn kho. Hãy cập nhật giỏ hàng.");
                    order.Items.Add(new OrderItem {ProductId=p.Id,Sku=p.Sku,Name=p.Name,CategoryName=cat.Name,Kind=p.Kind,
                        Attributes=p.Attributes.DeepClone().AsBsonDocument,UnitPrice=p.Price,Quantity=row.Quantity,LineTotal=checked(p.Price*row.Quantity)});
                }
                order.Total=order.Items.Sum(x=>x.LineTotal);
                await orders.Create(order, s, ct);
                return order;
            },cancellationToken:CancellationToken.None);
        } catch (MongoWriteException ex) when (ex.WriteError.Category==ServerErrorCategory.DuplicateKey) {
            return await FindCheckout(key,token) ?? throw new BusinessException("Mã đặt hàng trùng. Hãy tải lại trang.");
        }
    }
    public async Task ChangeStatus(string id,string status) {
        if(!ObjectId.TryParse(id,out _) || status is not ("Completed" or "Cancelled")) throw new BusinessException("Trạng thái không hợp lệ.");
        using var session=await client.StartSessionAsync();
        await session.WithTransactionAsync(async(s,ct)=> {
            var order=await orders.GetById(id, s, ct) ?? throw new BusinessException("Không tìm thấy đơn.");
            if(order.Status==status) return true;
            if(order.Status!="Pending") throw new BusinessException("Chỉ đơn Pending mới có thể hoàn tất hoặc hủy.");
            bool changed = await orders.ChangePendingStatus(id, status, s, ct);
            if(!changed) throw new BusinessException("Đơn vừa được cập nhật. Hãy tải lại.");
            if(status=="Cancelled") foreach(var item in order.Items) {
                bool restored = await products.RestoreStock(item.ProductId, item.Quantity, s, ct);
                if(!restored) throw new BusinessException("Không tìm thấy sản phẩm để hoàn kho; transaction được hủy.");
            }
            return true;
        },cancellationToken:CancellationToken.None);
    }
}
