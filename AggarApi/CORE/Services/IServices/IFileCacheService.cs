using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IFileCacheService
    {
        Task<bool> AddAsync(string filePath);
    }
}
