using NGBills.Entities;
using static NGBills.DTOs.AuthDtos;

namespace NGBills.Interface.Service
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(UserRegistrationDto registrationDto);
        Task<AuthResponseDto> Login(UserLoginDto loginDto);
        string GenerateJwtToken(User user);
    }
}
