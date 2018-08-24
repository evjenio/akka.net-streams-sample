namespace models.Model
{
    public class User : IIdentity
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}