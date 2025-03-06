using CORE.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CORE.Helpers
{
    public class EmailTemplateRendererService : IEmailTemplateRendererService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<EmailTemplateRendererService> _logger;
        private readonly RazorLightEngine _engine;
        public EmailTemplateRendererService(IWebHostEnvironment env, ILogger<EmailTemplateRendererService> logger)
        {
            _env = env;
            _logger = logger;
            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_env.WebRootPath)
                .UseMemoryCachingProvider()
                .Build();
        }
        public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
        {
            string templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", $"{templateName}.cshtml");
            _logger.LogInformation("Attempting to render email template '{TemplateName}' from path '{TemplatePath}'", templateName, templatePath);

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template '{TemplateName}' not found at path '{TemplatePath}'", templateName, templatePath);
                return $"Email template '{templateName}' not found in '{templatePath}'.";
            }

            try
            {
                string templateContent = await File.ReadAllTextAsync(templatePath);
                string renderedTemplate = await _engine.CompileRenderStringAsync(templateName, templateContent, model);

                _logger.LogInformation("Successfully rendered email template '{TemplateName}'", templateName);
                return renderedTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render email template '{TemplateName}'", templateName);
                return "";
            }
        }
    }
}
