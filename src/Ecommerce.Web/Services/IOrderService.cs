using Ecommerce.Web.Models;

namespace Ecommerce.Web.Services;

public interface IOrderService {
    Task<Order?> Get(string id);
    Task<OrderPage> ListForCustomer(string token, int page);
    Task<Order?> FindCheckout(string key, string customerToken);
    Task<OrderPage> List(int page, string? status);
    Task<Order> Place(IReadOnlyList<CartLine> cart, Customer customer, string key, string customerToken);
    Task ChangeStatus(string id, string status);
}
