using nak_kahwin_api.DTOs.Auth;
using nak_kahwin_api.DTOs.Plan;

namespace nak_kahwin_api.Services;

public interface IPlanService
{
    Task<PlanDetailsResponse?> CreatePlan(CreatePlanRequest req, string ownerId);
    Task<PlanDetailsResponse?> GetPlanForUserAsync(string userId);
}
