using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Persistance;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security
{
    public class IsHostRequirement : IAuthorizationRequirement
    {}


    public class IsHostRequirementHandler : AuthorizationHandler<IsHostRequirement>
    {
        public DataContext _dbContext { get; }
        public IHttpContextAccessor _httpContextAccessor { get; }
        public IsHostRequirementHandler(DataContext dbContext, IHttpContextAccessor httpContextAccessor){
            this._httpContextAccessor   = httpContextAccessor;
            this._dbContext             = dbContext;

        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsHostRequirement requirement)
        {
            var userId      = authContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null) return Task.CompletedTask;

            var activityId  = Guid.Parse(_httpContextAccessor.HttpContext?.Request.RouteValues.SingleOrDefault(x => x.Key == "id").Value?.ToString());

            var attendee    = _dbContext.ActivityAttendees.AsNoTracking()
            .SingleOrDefaultAsync(x => x.AppUserId == userId && x.ActivityId == activityId).Result;

            if(attendee == null) return Task.CompletedTask;

            if(attendee.IsHost) authContext.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}