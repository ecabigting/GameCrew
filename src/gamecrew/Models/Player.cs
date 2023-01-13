using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace gamecrew.Models
{
    public class Player : BaseModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(12,999,ErrorMessage = "Age must be more than 12")]
        public int Age { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression("^(?=\\D*\\d)(?=[^a-z]*[a-z])[\\w~@#$%^&*+=`|{}:;!.?\"()\\[\\]-]{4,8}$",
            ErrorMessage = "The password must contain 4 to 8 characters containing letters and numbers. You can also use special characters. ")]
        public string Password { get; set; }
        public string Image { get; set; }

        public string HashP(string access, string key)
        {
            string retPass = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: access.ToString(),
                                salt: Encoding.UTF8.GetBytes(key),
                                prf: KeyDerivationPrf.HMACSHA1,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8
                            ));
            return retPass;
        }
    }

    public class PlayerLogin 
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class PlayerContext
    { 
        public Player Profile { get; set; }
    }

    public class PlayerAccessToken : BaseModel
    {
        public string PlayerID { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}