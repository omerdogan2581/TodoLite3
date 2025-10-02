namespace TodoLite.Models
{
    public class UserUpdateDto
    {
        public string? Role { get; set; }
        public bool? Status { get; set; }
        public string? Password { get; set; }
    }
}
