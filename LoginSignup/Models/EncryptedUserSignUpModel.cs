using System.ComponentModel.DataAnnotations;

namespace LoginSignup.Models
{
    public class EncryptedUserSignUpModel
    {
        public int Id { get; set; }
        public byte[] EncryptedData { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

    }
}
