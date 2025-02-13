using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Services.IServices
{
    public interface IEmailTemplateRendererService
    {
        Task<string> RenderTemplateAsync<T>(string templateName, T model);
    }

}
