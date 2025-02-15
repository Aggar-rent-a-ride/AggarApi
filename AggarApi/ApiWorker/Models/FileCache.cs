using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWorker.Models
{
    public class FileCache
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
