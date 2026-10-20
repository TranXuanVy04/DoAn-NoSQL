using Ecommerce.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Ecommerce.Web.Services;

public sealed class MongoOrderService(IMongoDatabase db,IMongoClient client) : IOrderService {
    private IMongoCollection<Order> Orders => db.GetCollection<Order>("hoadon");
    private IMongoCollection<Product> Goods => db.GetCollection<Product>("sanpham");
    private IMongoCollection<Category> Cats => db.GetCollection<Category>("danhmuc");
    public async Task<Order?> Get(string id) => ObjectId.TryParse(id,out _) ? await Orders.Find(x=>x.Id==id).FirstOrDefaultAsync() : null;
    public async Task<Order?> FindCheckout(string key,string customerToken) => await Orders.Find(x=>x.CheckoutKey==key && x.CustomerToken==customerToken).FirstOrDefaultAsync();
    public async Task<OrderPage> List(int page,string? status) {
        var f=string.IsNullOrEmpty(status) ? Builders<Order>.Filter.Empty : Builders<Order>.Filter.Eq(x=>x.Status,status);
        const int size=15; var total=await Orders.CountDocumentsAsync(f);
        page=Math.Clamp(page,1,Math.Max(1,(int)Math.Ceiling(total/(double)size)));
        return new(await Orders.Find(f).SortByDescending(x=>x.CreatedAt).ThenByDescending(x=>x.Id).Skip((page-1)*size).Limit(size).ToListAsync(),total,page,size);
    }
    public async Task<Order> Place(IReadOnlyList<CartLine> cart,Customer customer,string key,string token) {
        var existing=await FindCheckout(key,token); if(existing is not null) return existing;
        if(cart.Count is <1 or >50 || cart.Any(x=>x.Quantity is <1 or >99 || !ObjectId.TryParse(x.ProductId,out _)) || cart.Select(x=>x.ProductId).Distinct().Count()!=cart.Count)
            throw new BusinessException("Giỏ hàng không hợp lệ (1–50 dòng, mỗi dòng 1–99 sản phẩm).");
        using var session=await client.StartSessionAsync();
        try {
            return await session.WithTransactionAsync(async (s,ct) => {
                var duplicate=await Orders.Find(s,x=>x.CheckoutKey==key && x.CustomerToken==token).FirstOrDefaultAsync(ct);
                if(duplicate is not null) return duplicate;
                var order=new Order {CheckoutKey=key,CustomerToken=token,Customer=customer};
                order.Number="EC-"+order.Id.ToUpperInvariant();
                foreach(var row in cart.OrderBy(x=>x.ProductId,StringComparer.Ordinal)) {
                    var p=await Goods.Find(s,x=>x.Id==row.ProductId).FirstOrDefaultAsync(ct);
                    if(p is null || !p.IsActive) throw new BusinessException("Một sản phẩm trong giỏ đã ngừng bán.");
                    var cat=await Cats.Find(s,x=>x.Id==p.CategoryId).FirstOrDefaultAsync(ct);
                    if(cat is null) throw new BusinessException("Danh mục sản phẩm không còn tồn tại.");
                    var result=await Goods.UpdateOneAsync(s,x=>x.Id==p.Id && x.IsActive && x.Stock>=row.Quantity,
                        Builders<Product>.Update.Inc(x=>x.Stock,-row.Quantity),cancellationToken:ct);
                    if(result.ModifiedCount!=1) throw new BusinessException($"{p.Name} không đủ tồn kho. Hãy cập nhật giỏ hàng.");
                    order.Items.Add(new OrderItem {ProductId=p.Id,Sku=p.Sku,Name=p.Name,CategoryName=cat.Name,Kind=p.Kind,
                        Attributes=p.Attributes.DeepClone().AsBsonDocument,UnitPrice=p.Price,Quantity=row.Quantity,LineTotal=checked(p.Price*row.Quantity)});
                }
                order.Total=order.Items.Sum(x=>x.LineTotal);
                await Orders.InsertOneAsync(s,order,cancellationToken:ct);
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
            var order=await Orders.Find(s,x=>x.Id==id).FirstOrDefaultAsync(ct) ?? throw new BusinessException("Không tìm thấy đơn.");
            if(order.Status==status) return true;
            if(order.Status!="Pending") throw new BusinessException("Chỉ đơn Pending mới có thể hoàn tất hoặc hủy.");
            var changed=await Orders.UpdateOneAsync(s,x=>x.Id==id && x.Status=="Pending",Builders<Order>.Update.Set(x=>x.Status,status),cancellationToken:ct);
            if(changed.ModifiedCount!=1) throw new BusinessException("Đơn vừa được cập nhật. Hãy tải lại.");
            if(status=="Cancelled") foreach(var item in order.Items) {
                var restored=await Goods.UpdateOneAsync(s,x=>x.Id==item.ProductId,Builders<Product>.Update.Inc(x=>x.Stock,item.Quantity),cancellationToken:ct);
                if(restored.MatchedCount!=1) throw new BusinessException("Không tìm thấy sản phẩm để hoàn kho; transaction được hủy.");
            }
            return true;
        },cancellationToken:CancellationToken.None);
    }
}
