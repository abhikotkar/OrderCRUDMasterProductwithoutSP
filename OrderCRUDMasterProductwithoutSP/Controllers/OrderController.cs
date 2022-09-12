using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderCRUDMasterProductwithoutSP.Entities;
using OrderCRUDMasterProductwithoutSP.Repositories.Interfaces;

namespace OrderCRUDMasterProductwithoutSP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;

        public OrderController(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var order = await _orderRepo.GetOrders();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("id")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order=await _orderRepo.GetOrderById(id);
                return Ok(order);
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            try
            {
                var result = await _orderRepo.PlaceOrder(order);

                if (result == 0)
                {
                    return StatusCode(409, "The request could not be processed because of conflict in the request");
                }
                else
                {
                    return StatusCode(200, string.Format("Record Inserted Successfuly with total amount is {0}", result));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
