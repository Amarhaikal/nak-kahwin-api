using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Plan;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class PlanService(AppDbContext db) : IPlanService
{
    public async Task<PlanDetailsResponse?> CreatePlan(CreatePlanRequest req, string ownerId)
    {
        var owner = await db.Users.FindAsync(ownerId);
        if (owner is null) return null;

        var plan = new Plan
        {
            Title = req.Title,
            OwnerId = ownerId,
            IsEngagementEnabled = req.IsEngagementEnabled
        };

        db.Plans.Add(plan);

        // Always create a default marriage event
        var marriageEvent = new Event
        {
            PlanId = plan.Id,
            Type = "marriage",
            Date = req.WeddingDate
        };
        db.Events.Add(marriageEvent);

        // If engagement is enabled, create an engagement event
        if (req.IsEngagementEnabled)
        {
            var engagementEvent = new Event
            {
                PlanId = plan.Id,
                Type = "engagement",
                Date = req.EngagementDate
            };
            db.Events.Add(engagementEvent);
        }

        await db.SaveChangesAsync();

        return new PlanDetailsResponse(
            Id: plan.Id,
            Title: plan.Title,
            IsEngagementEnabled: plan.IsEngagementEnabled,
            OwnerName: owner.Name,
            PartnerName: null
        );
    }

    public async Task<PlanDetailsResponse?> GetPlanForUserAsync(string userId)
    {
        var plan = await db.Plans
            .FirstOrDefaultAsync(p => p.OwnerId == userId || p.PartnerId == userId);

        if (plan is null) return null;

        var owner = await db.Users.FindAsync(plan.OwnerId);
        var partner = plan.PartnerId != null ? await db.Users.FindAsync(plan.PartnerId) : null;

        return new PlanDetailsResponse(
            Id: plan.Id,
            Title: plan.Title,
            IsEngagementEnabled: plan.IsEngagementEnabled,
            OwnerName: owner?.Name ?? "Unknown",
            PartnerName: partner?.Name
        );
    }
}