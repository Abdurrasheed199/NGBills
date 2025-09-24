namespace NGBills.DTOs
{
    public class AuthDtos
    {
        public class UserRegistrationDto
        {
            public  string Email { get; set; }
            public  string FirstName { get; set; }
            public  string LastName { get; set; }
            public  string Password { get; set; }
        }

        public class UserLoginDto
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
        }

        public class AuthResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public UserDto User { get; set; } = new UserDto();
        }

        public class UserDto
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; } 
        }
    }
}
