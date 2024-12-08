namespace AggarApi.Models
{
    public class Admin : AppUser
    {
        public ICollection<AdminAction>? Actions { get; set; }
    }
}
