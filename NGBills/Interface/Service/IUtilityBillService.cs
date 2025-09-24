using NGBills.Entities;
using static NGBills.DTOs.TransactionDtos;
using static NGBills.DTOs.UtilityBillDtos;

namespace NGBills.Interface.Service
{
    public interface IUtilityBillService
    {
        Task<UtilityBillResponseDto> GetBillByIdAsync(int userId, int billId);
        Task<IEnumerable<UtilityBillResponseDto>> GetUserBillsAsync(int userId);
        Task<UtilityBillResponseDto> AddBillAsync(int userId, AddBillDto addBillDto);
        Task<TransactionResponseDto> PayBillAsync(int userId, PayBillDto payBillDto);
        Task<bool> DeleteBillAsync(int userId, int billId);


        Task<BillRetrievalResult> RetrieveBillFromProviderAsync(RetrieveBillDto retrieveDto);
        Task<IEnumerable<UtilityProvider>> GetActiveProvidersAsync();








    }
}
