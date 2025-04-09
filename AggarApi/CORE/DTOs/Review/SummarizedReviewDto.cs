using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.DTOs.Review
{
    public class SummarizedReviewDto
    {
        public int ReviewId { get; set; }
        public ReviewerDto Reviewer { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Rate { get; set; }
        public string? Comments { get; set; }
    }
}
