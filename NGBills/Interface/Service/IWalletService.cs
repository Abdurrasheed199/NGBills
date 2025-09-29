using NGBills.Entities;
using static NGBills.DTOs.TransactionDtos;
using static NGBills.DTOs.WalletDtos;

namespace NGBills.Interface.Service
{
    public interface IWalletService
    {
        Task<Wallet> GetWalletByUserIdAsync(int userId);
        Task<WalletDto> GetWalletDtoByUserIdAsync(int userId);
        Task ProcessPaymentVerification(string reference, int userId);
        Task<decimal> GetWalletBalanceAsync(int userId);
        //Task<string> InitializeFundingAsync(int userId, FundWalletDto fundWalletDto);
        Task<bool> VerifyPaymentAsync(string reference);
        Task<InitiateResponse> FundWalletAsync(decimal amount, string email, int userId);
        //Task<WalletResponseDto> FundWalletAsync(decimal amount, string email, int userId);
        Task<IEnumerable<TransactionResponseDto>> GetWalletTransactionsAsync(int userId, int pageNumber = 1, int pageSize = 10);
      
    }
}
