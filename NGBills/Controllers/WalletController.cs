using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NGBills.Interface.Service;
using static NGBills.DTOs.UtilityBillDtos;
using System.Security.Claims;
using static NGBills.DTOs.WalletDtos;
using NGBills.Interface.Repository;

namespace NGBills.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IPaystackService _paystackService;
        private readonly IWalletService _walletService;
        private readonly IUtilityBillService _utilityBillService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IPaystackService paystackService,
            IWalletService walletService,
            IUtilityBillService utilizationBillService,
            IUserRepository userRepository,
            ILogger<WalletController> logger)
        {
            _paystackService = paystackService;
            _walletService = walletService;
            _utilityBillService = utilizationBillService;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance(int id)
        {
            try
            {
                var userId = await GetUserIdFromToken(id);
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
        public async Task<IActionResult> GetWallet(int id)
        {
            try
            {
                var userId = await GetUserIdFromToken(id);
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
        public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto, int id)
        {
            try
            {
                var userId = await GetUserIdFromToken(id);
                var response = await _walletService.FundWalletAsync(fundWalletDto.Amount, fundWalletDto.Email, userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing wallet funding");
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("fund")]
        //public async Task<IActionResult> FundWallet([FromBody] FundWalletDto fundWalletDto, int id)
        //{
        //    try
        //    {
        //        // Get user ID from token (you'll need to implement this)
        //        var userId = GetUserIdFromToken(id);

        //        if (userId <= 0)
        //        {
        //            return Unauthorized(new { message = "Invalid user token" });
        //        }

        //        var response = await _walletService.FundWalletAsync(
        //            fundWalletDto.Amount,
        //            fundWalletDto.Email,
        //            userId);

        //        if (!response.Success)
        //        {
        //            return BadRequest(new { message = response.Message });
        //        }

        //        return Ok(new
        //        {
        //            success = response.Success,
        //            message = response.Message,
        //            authorizationUrl = response.AuthorizationUrl,
        //            reference = response.Reference
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error initializing wallet funding");
        //        return BadRequest(new { message = "An error occurred while processing your request" });
        //    }
        //}



        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyTransactionDto verifyDto, int id)
        {
            try
            {
                var userId = await GetUserIdFromToken(id);
                var isVerified = await _paystackService.VerifyTransaction(verifyDto.Reference);

              
                    await _walletService.ProcessPaymentVerification(verifyDto.Reference, userId);
                    return Ok(new { message = "Payment verified successfully" });
                

                return BadRequest(new { message = "Payment verification failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        //private int GetUserIdFromToken(int id)
        //{


        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        //             ?? User.FindFirst("sub")?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        //    {
        //        throw new UnauthorizedAccessException("Invalid user token");
        //    }

        //    return userId;
        //}

        private async Task<int> GetUserIdFromToken(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User Not Found");
            }
            return user.Id; // Assuming your User entity has an Id property
        }
    }
}
