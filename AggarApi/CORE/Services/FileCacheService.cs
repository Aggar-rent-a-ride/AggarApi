using CORE.Services.IServices;
using DATA.DataAccess.Repositories.UnitOfWork;
using DATA.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services
{
    public class FileCacheService : IFileCacheService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public FileCacheService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        public async Task<bool> AddAsync(string filePath)
        {
            var file = new FileCache
            {
                Path = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/')),
                ExpiresOn = DateTime.UtcNow.AddSeconds(10)
            };
            
            _unitOfWork.FileCache.AddOrUpdateAsync(file);
            int changes = await _unitOfWork.CommitAsync();

            return changes > 0;
        }
    }
}
