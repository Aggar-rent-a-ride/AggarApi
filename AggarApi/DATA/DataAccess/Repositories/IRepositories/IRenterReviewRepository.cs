using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DATA.Models;

namespace DATA.DataAccess.Repositories.IRepositories
{
    public interface IRenterReviewRepository: IBaseRepository<RenterReview>
    {
        Task<IEnumerable<(double Behavior, double Punctuality, double Care)>> GetAllRatesAsync(Expression<Func<RenterReview, bool>> criteria);
    }
}
