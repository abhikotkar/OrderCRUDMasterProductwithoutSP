using OrderCRUDMasterProductwithoutSP.Entities;

namespace OrderCRUDMasterProductwithoutSP.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        public Task<IEnumerable<Order>> GetOrders();
        public Task<Order> GetOrderById(int id);

        public Task<double> PlaceOrder(Order order);
    }
}
