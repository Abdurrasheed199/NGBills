using static NGBills.DTOs.WalletDtos;

namespace NGBills.Interface.Service
{
    public interface IPaystackService
    {
        
        Task<PaystackVerifyResponse> VerifyTransaction(string reference);
        Task<InitiateResponse> InitializePayment(FundWalletDto fundWalletDto);
    }
}
