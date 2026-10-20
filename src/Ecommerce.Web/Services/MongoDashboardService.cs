using Ecommerce.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;
namespace Ecommerce.Web.Services;

public sealed class MongoDashboardService(IMongoDatabase db) : IDashboardService {
    public async Task<DashboardView> Report(DateOnly from,DateOnly to,string group) {
        if(group is not ("day" or "month")) throw new BusinessException("Chỉ hỗ trợ nhóm theo ngày hoặc tháng.");
        var (start,end)=ReportRange.Utc(from,to);
        var match=new BsonDocument("$match",new BsonDocument { {"status","Completed"},{"createdAt",new BsonDocument {{"$gte",start},{"$lt",end}}} });
        BsonDocument[] revenue=[match,
            new("$group",new BsonDocument { {"_id",new BsonDocument("$dateToString",new BsonDocument {{"format",group=="day"?"%Y-%m-%d":"%Y-%m"},{"date","$createdAt"},{"timezone","+07:00"}})},
                {"revenue",new BsonDocument("$sum","$total")},{"orders",new BsonDocument("$sum",1)} }),
            new("$sort",new BsonDocument("_id",1))];
        BsonDocument[] top=[match,new("$sort",new BsonDocument {{"createdAt",-1},{"_id",-1}}),new("$unwind","$items"),
            new("$group",new BsonDocument {{"_id","$items.productId"},{"name",new BsonDocument("$first","$items.name")},
                {"quantity",new BsonDocument("$sum","$items.quantity")},{"revenue",new BsonDocument("$sum",new BsonDocument("$multiply",new BsonArray {"$items.unitPrice","$items.quantity"}))}}),
            new("$sort",new BsonDocument {{"quantity",-1},{"revenue",-1},{"_id",1}}),new("$limit",5)];
        var coll=db.GetCollection<BsonDocument>("hoadon");
        var rev=await coll.Aggregate<BsonDocument>(revenue).ToListAsync();
        var best=await coll.Aggregate<BsonDocument>(top).ToListAsync();
        return new(from,to,group,rev.Select(x=>new RevenuePoint(x["_id"].AsString,x["revenue"].ToInt64(),x["orders"].ToInt32())).ToList(),
            best.Select(x=>new TopProduct(x["_id"].AsObjectId.ToString(),x["name"].AsString,x["quantity"].ToInt64(),x["revenue"].ToInt64())).ToList());
    }
}
