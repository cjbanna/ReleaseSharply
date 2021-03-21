using Microsoft.Extensions.Logging;
using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReleaseSharply.Server
{
    public class FeatureManager : IFeatureManager
    {
        private ILogger<FeatureManager> _logger;
        private readonly FeatureHub _hub;
        private readonly FeaturesDbContext _dbContext;

        public FeatureManager(
            ILogger<FeatureManager> logger,
            FeatureHub hub,
            FeaturesDbContext dbContext)
        {
            if (hub == null)
            {
                throw new ArgumentNullException(nameof(hub));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            _logger = logger;
            _hub = hub;
            _dbContext = dbContext;

            _logger.LogInformation(_dbContext.ContextId.InstanceId.ToString());
        }

        public async Task<FeatureGroup> AddFeatureGroupAsync(FeatureGroup featureGroup)
        {
            var value = await _dbContext.FeatureGroups.AddAsync(featureGroup);
            await _dbContext.SaveChangesAsync();
            return value.Entity;
        }

        public async Task<Feature> AddFeatureAsync(Feature feature)
        {
            var value = await _dbContext.Features.AddAsync(feature);
            await _dbContext.SaveChangesAsync();
            await SendFeatureUpdate(feature);
            return value.Entity;
        }

        public async Task<Feature> UpdateFeatureAsync(Feature feature)
        {
            var value = _dbContext.Features.Update(feature);
            await _dbContext.SaveChangesAsync();
            await SendFeatureUpdate(feature);
            return await Task.FromResult(value.Entity);
        }

        public async Task RemoveFeatureAsync(Feature feature)
        {
            _dbContext.Remove(feature);
            await _dbContext.SaveChangesAsync();
            // TODO: send SignalR message
        }

        private async Task SendFeatureUpdate(params Feature[] features)
        {
            var groupId = features.First().FeatureGroupId;
            var group = _dbContext.FeatureGroups.SingleOrDefault(g => g.Id == groupId);
            if (group != null)
            {
                await _hub.SendUpdateAsync(group.Name, features);
            }
        }

        //public async Task AddOrUpdateFeaturesAsync(string featureGroup, params Feature[] features)
        //{
        //    var existingGroup = _dbContext.FeatureGroups.SingleOrDefault(g => g.Name == featureGroup);
        //    if (existingGroup == null)
        //    {
        //        await _dbContext.FeatureGroups.AddAsync(new FeatureGroup { Name = featureGroup });
        //    }

        //    if (existingGroup == null)
        //    {
        //        await _dbContext.Features.AddRangeAsync(features);
        //    }
        //    else
        //    {
        //        foreach (var feature in features)
        //        {
        //            var existingFeature = existingGroup.Features.SingleOrDefault(f => f.Name == feature.Name);
        //            if (existingFeature == null)
        //            {
        //                await _dbContext.Features.AddAsync(feature);
        //            }
        //            else
        //            {
        //                _dbContext.Features.Update(feature);
        //            }
        //        }
        //    }

        //    await _dbContext.SaveChangesAsync();
        //    await _hub.SendUpdateAsync(featureGroup, features);
        //}

        public async Task<IEnumerable<Feature>> GetFeaturesAsync(string featureGroup)
        {
            var group = _dbContext
                .FeatureGroups
                .SingleOrDefault(g => g.Name == featureGroup);

            if (group != null)
            {
                return await Task.FromResult(_dbContext
                    .Features
                    .Where(f => f.FeatureGroupId == group.Id)
                    .ToList());
            }

            return await Task.FromResult(Enumerable.Empty<Feature>());
        }

        //public async Task RemoveFeatureAsync(string featureGroup, Feature feature)
        //{
        //    //await _featureGroupRepository.RemoveFeatureAsync(featureGroup, feature);
        //}
    }
}
