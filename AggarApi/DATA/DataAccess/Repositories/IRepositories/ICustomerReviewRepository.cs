using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DATA.Models;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface ICustomerReviewRepository : IBaseRepository<CustomerReview>
    {
        Task<IEnumerable<(double Behavior, double Punctuality, double Truthfulness)>> GetAllRatesAsync(Expression<Func<CustomerReview, bool>> criteria);
    }
}
