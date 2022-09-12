using Dapper;
using OrderCRUDMasterProductwithoutSP.Context;
using OrderCRUDMasterProductwithoutSP.Entities;
using OrderCRUDMasterProductwithoutSP.Repositories.Interfaces;

namespace OrderCRUDMasterProductwithoutSP.Repositories
{
    public class OrderRepository:IOrderRepository
    {
        private readonly DapperContext _context;

        public OrderRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Order> GetOrderById(int id)
        {
            Order order = null;
            var query = "Select * from DMartBill where orderId=@orderId";
            using (var connection = _context.CreateConnection())
            {
                var ordersraw = await connection.QueryAsync<Order>(query, new { orderId = id });
                order = ordersraw.FirstOrDefault();
                if (order != null)
                {
                    var orderdetailsrow = await connection.QueryAsync<OrderDetails>(@"select o.detailsId,p.productId,p.productName,p.productPrice,
                    o.quentity,o.totalAmount,o.orderId from Product p inner join Odetails o on 
                    p.productId=o.productId  where o.detailsId =any (select detailsId from Odetails where orderId=@orderId)", new { orderId = id });
                    order.OrderDetails = orderdetailsrow.ToList();
                }
                return order;
            }
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            List<Order> orders = new List<Order>();
            var query = "Select * from DMartBill";
            using (var connection = _context.CreateConnection())
            {
                var ordersraw = await connection.QueryAsync<Order>(query);
                orders = ordersraw.ToList();
                foreach (var order in orders)
                {
                    var orderdetailsrow = await connection.QueryAsync<OrderDetails>(@"select o.detailsId,p.productId,p.productName,p.productPrice,
                    o.quentity,o.totalAmount,o.orderId from Product p inner join Odetails o on 
                    p.productId=o.productId  where o.detailsId =any (select detailsId from Odetails where orderId=@orderId)",new { orderId = order.orderId });
                    order.OrderDetails = orderdetailsrow.ToList();
                }
                return orders;
            }
        }

        public async Task<double> PlaceOrder(Order order)
        {
            double result1 = 0;
            int result = 0;
            var query = @"insert into DMartBill(orderCode,custName,mobileNumber,shippingAddress,billingAddress)
                          VALUES (@orderCode,@custName,@mobileNumber,@shippingAddress,@billingAddress);
                          SELECT CAST(SCOPE_IDENTITY() as int)";
            List<OrderDetails> odlist = new List<OrderDetails>();
            odlist = order.OrderDetails.ToList();

            using (var connection = _context.CreateConnection())
            {
                result = await connection.ExecuteScalarAsync<int>(query, order);
                // if (result != 0)
                // {
                result1 = await AddProduct(odlist, result);
                order.totalAmount = result1;
                var qry1 = "update DMartBill set totalAmount=@totalAmount where orderId=@orderId";

                var result3 = await connection.ExecuteAsync(qry1, new { totalAmount = order.totalAmount, orderId = result });


                return result1;
            }

        }

        private async Task<double> AddProduct(List<OrderDetails> orders, int result2)
        {
            int result = 0;
            double grandtotal = 0;
            using (var connection = _context.CreateConnection())
            {
                if (orders.Count > 0)
                {
                    foreach (OrderDetails order in orders)
                    {
                        order.orderId = result2;

                        var query = @"insert into Odetails(productId,quentity,orderId)
                                      VALUES(@productId,@quentity,@orderId);
                                      SELECT CAST(SCOPE_IDENTITY() as int)";

                        var result1 = await connection.ExecuteScalarAsync<int>(query, order);
                        result = result + result1;
                        var pquery = "select productName,productPrice from Product where productId=@productId";
                        orders = (List<OrderDetails>)await connection.QueryAsync<OrderDetails>(pquery, new { productId = order.productId });
                        order.productPrice = orders[0].productPrice;
                        order.productName = orders[0].productName;
                        order.totalAmount =  order.productPrice* order.quentity;

                        var qry1 = "update Odetails set totalAmount=@totalAmount where detailsId=@detailsId";

                        var result3 = await connection.ExecuteAsync(qry1, new { totalAmount = order.totalAmount, detailsId = result1});
                        grandtotal = grandtotal + order.totalAmount;
                    }

                }
                return grandtotal;
            }
        }
    }
}
