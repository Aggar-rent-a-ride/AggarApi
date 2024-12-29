namespace DATA.Models
{
    public class RecommendedType
    {
        public int TypeId { get; set; }
        public int CustomerId { get; set; }
        public int Points { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
