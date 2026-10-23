using Ecommerce.Web.Models;
using MongoDB.Driver;

namespace Ecommerce.Web.Repositories;

public class CategoryRepository
{
    private readonly IMongoCollection<Category> categories;

    public CategoryRepository(IMongoDatabase database)
    {
        categories = database.GetCollection<Category>("danhmuc");
    }

    public Task<List<Category>> GetAll()
    {
        return categories.Find(FilterDefinition<Category>.Empty).SortBy(x => x.Name).ToListAsync();
    }

    public Task<Category?> GetById(string id, IClientSessionHandle? session = null, CancellationToken cancellationToken = default)
    {
        var query = session == null
            ? categories.Find(x => x.Id == id)
            : categories.Find(session, x => x.Id == id);
        return query.FirstOrDefaultAsync(cancellationToken)!;
    }

    public Task<bool> NameExists(string name, string? excludedId)
    {
        return categories.Find(x => x.Name == name && x.Id != excludedId).AnyAsync();
    }

    public Task<bool> HasChildren(string id)
    {
        return categories.Find(x => x.ParentId == id).AnyAsync();
    }

    public Task Create(Category category)
    {
        return categories.InsertOneAsync(category);
    }

    public async Task Update(Category category)
    {
        await categories.ReplaceOneAsync(x => x.Id == category.Id, category);
    }

    public async Task Delete(string id)
    {
        await categories.DeleteOneAsync(x => x.Id == id);
    }
}
