namespace models.Model
{
    public class UserProperties : IIdentity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}