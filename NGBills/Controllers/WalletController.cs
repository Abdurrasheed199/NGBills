using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NGBills.Interface.Service;
using static NGBills.DTOs.UtilityBillDtos;
using System.Security.Claims;
using static NGBills.DTOs.WalletDtos;

namespace NGBills.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IPaystackService _paystackService;
        private readonly IWalletService _walletService;
        private readonly IUtilityBillService _utilityBillService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IPaystackService paystackService,
            IWalletService walletService,
            IUtilityBillService utilizationBillService,
            ILogger<WalletController> logger)
        {
            _paystackService = paystackService;
            _walletService = walletService;
            _utilityBillService = utilizationBillService;
            _logger = logger;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var balance = await _walletService.GetWalletBalanceAsync(userId);

                return Ok(new { Balance = balance, Currency = "NGN" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet balance");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var wallet = await _walletService.GetWalletDtoByUserIdAsync(userId);

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet");
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("pay-bill")]
        //public async Task<IActionResult> PayBill([FromBody] PayBillDto payBillDto)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        //    var transaction = await _utilityBillService.PayBillAsync(userId, payBillDto);
        //    return Ok(transaction);
        //}

        [HttpPost("fund")]
        public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var response = await _walletService.FundWalletAsync(fundWalletDto.Amount, fundWalletDto.Email,userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing wallet funding");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyTransactionDto verifyDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var isVerified = await _paystackService.VerifyTransaction(verifyDto.Reference);

                if (isVerified)
                {
                    await _walletService.ProcessPaymentVerification(verifyDto.Reference, userId);
                    return Ok(new { message = "Payment verified successfully" });
                }

                return BadRequest(new { message = "Payment verification failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new Exception("Invalid user ID in token");
            }
            return userId;
        }
    }
}
