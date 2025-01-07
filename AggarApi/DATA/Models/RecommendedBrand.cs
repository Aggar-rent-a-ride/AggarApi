namespace DATA.Models
{
    public class RecommendedBrand
    {
        public int BrandId { get; set; }
        public int CustomerId { get; set; }
        public int Points { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }
        public Customer Customer { get; set; } = null!;
    }
}
