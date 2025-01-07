namespace DATA.Models
{
    public class Admin : AppUser
    {
        public ICollection<AdminAction>? Actions { get; set; }
    }
}
