namespace models.Model
{
    public class UserAuthTypes : IIdentity
    {
        public int Id { get; set; }
        public string[] AuthTypes { get; set; }
    }
}