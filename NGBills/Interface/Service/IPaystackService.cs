using static NGBills.DTOs.WalletDtos;

namespace NGBills.Interface.Service
{
    public interface IPaystackService
    {
        Task<PaystackResponseDto> InitializeTransaction(FundWalletDto fundWalletDto);
        Task<bool> VerifyTransaction(string reference);
    }
}
