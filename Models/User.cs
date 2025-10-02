namespace TodoLite.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool Status { get; set; } = false; // kayıt olunca false
        public string Role { get; set; } = "User"; // default User
    }
}
