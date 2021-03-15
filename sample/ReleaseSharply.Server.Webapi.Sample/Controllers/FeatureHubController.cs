using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReleaseSharply.Server.Webapi.Sample.Controllers
{
    //[ApiController]
    //[Route("features")]
    //public class FeatureHubController : ControllerBase
    //{
    //    private readonly FeatureHub _featureHub;

    //    public FeatureHubController(FeatureHub featureHub)
    //    {
    //        _featureHub = featureHub;
    //    }

    //    [HttpPost("{featureGroup}")]
    //    public async Task SendUpdateAsync(string featureGroup)
    //    {
    //        //var featureGroup = "ConsoleFeatures";
    //        var features = default(Feature[]);

    //        if (DateTime.Now.Ticks % 2 == 0)
    //        {
    //            features = new Feature[]
    //            {
    //                new Feature("foo", false),
    //                new Feature("bar", true)
    //            };
    //        }
    //        else
    //        {
    //            features = new Feature[]
    //            {
    //                new Feature("biz", false),
    //                new Feature("baz", true)
    //            };
    //        }

    //        await _featureHub.SendUpdateAsync(featureGroup, features);
    //    }

    //    [HttpPost("{featureGroup}/subscribe")]
    //    public async Task SubscribeAsync(string featureGroup)
    //    {
    //        await _featureHub.AddToGroup(featureGroup);
    //    }
    //}
}
