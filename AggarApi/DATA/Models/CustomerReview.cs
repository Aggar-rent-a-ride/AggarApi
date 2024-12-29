namespace DATA.Models
{
    public class CustomerReview : Review
    {
        public double Truthfulness { get; set; }
        public double Price { get; set; }
        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
