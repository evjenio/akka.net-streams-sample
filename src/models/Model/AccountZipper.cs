namespace models.Model
{
    public static class AccountZipper
    {
        public static Account Zip(User u, UserProperties up, UserAuthTypes at)
            => new Account
            {
                Id = u.Id,
                Name = u.Name,
                Phone = up.Phone,
                Email = up.Email,
                AuthTypes = at.AuthTypes,
            };
    }
}