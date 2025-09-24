using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NGBills.Interface.Service;
using static NGBills.DTOs.UtilityBillDtos;
using System.Security.Claims;

namespace NGBills.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityBillsController : ControllerBase
    {
        private readonly IUtilityBillService _utilityBillService;

        public UtilityBillsController(IUtilityBillService utilityBillService)
        {
            _utilityBillService = utilityBillService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBills()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var bills = await _utilityBillService.GetUserBillsAsync(userId);
            return Ok(bills);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBill(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var bill = await _utilityBillService.GetBillByIdAsync(userId, id);
            return Ok(bill);
        }

        [HttpPost]
        public async Task<IActionResult> AddBill([FromBody] AddBillDto addBillDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var bill = await _utilityBillService.AddBillAsync(userId, addBillDto);
            return CreatedAtAction(nameof(GetBill), new { id = bill.Id }, bill);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayBill([FromBody] PayBillDto payBillDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var transaction = await _utilityBillService.PayBillAsync(userId, payBillDto);
            return Ok(new { message = "Bill paid successfully", transaction });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _utilityBillService.DeleteBillAsync(userId, id);

            if (!result)
                return NotFound();

            return Ok(new { message = "Bill deleted successfully" });
        }

        // ✅ NEW: Retrieve bill from provider
        [HttpPost("retrieve")]
        public async Task<IActionResult> RetrieveBill([FromBody] RetrieveBillDto retrieveDto)
        {
            var result = await _utilityBillService.RetrieveBillFromProviderAsync(retrieveDto);

            if (!result.Success)
            {
                return BadRequest(new { result.Success, result.Message });
            }

            return Ok(new { result.Success, result.Message, result.BillId, result.Amount, result.DueDate });
        }
    }
}
