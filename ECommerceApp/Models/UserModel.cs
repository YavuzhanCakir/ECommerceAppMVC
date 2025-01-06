namespace ECommerceApp.Models
{
    public class UserModel 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }

        public bool IsConfirmed { get; set; } // Kullanıcı onay durumu
        public string ConfirmationToken { get; set; }
    }
}
