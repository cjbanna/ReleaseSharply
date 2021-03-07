using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ReleaseSharply.Server.Webapp.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FeatureHub _featureHub;

        public IndexModel(ILogger<IndexModel> logger, FeatureHub featureHub)
        {
            _logger = logger;
            _featureHub = featureHub;
        }

        public void OnGet()
        {
            var featureGroup = "ConsoleFeatures";
            var features = default(Feature[]);

            if (DateTime.Now.Ticks % 2 == 0)
            {
                features = new Feature[]
                {
                    new Feature("foo", false),
                    new Feature("bar", true)
                };
            }
            else
            {
                features = new Feature[]
                {
                    new Feature("biz", false),
                    new Feature("baz", true)
                };
            }

            _featureHub.SendUpdateAsync(featureGroup, features).GetAwaiter().GetResult();
        }
    }
}
