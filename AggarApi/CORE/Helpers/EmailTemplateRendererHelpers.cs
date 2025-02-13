using Microsoft.AspNetCore.Hosting;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public class EmailTemplateRendererHelpers
    {
        private readonly IWebHostEnvironment _env;
        private readonly RazorLightEngine _engine;
        public EmailTemplateRendererHelpers(IWebHostEnvironment env)
        {
            _env = env;
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_env.WebRootPath)
                .UseMemoryCachingProvider()
                .Build();
        }
        public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
        {
            string templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", $"{templateName}.cshtml");

            if (!File.Exists(templatePath))
                return $"Email template '{templateName}' not found in '{templatePath}'.";

            string templateContent = await File.ReadAllTextAsync(templatePath);
            return await _engine.CompileRenderStringAsync(templateName, templateContent, model);
        }
    }
}
