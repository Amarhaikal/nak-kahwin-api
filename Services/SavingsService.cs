using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Savings;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class SavingsService(AppDbContext db) : ISavingsService
{
    private async Task<Event?> GetUserEventAsync(string userId)
    {
        return await db.Events
            .FirstOrDefaultAsync(e => e.OwnerId == userId || e.PartnerId == userId);
    }

    public async Task<List<SavingsGoalResponse>> GetSavingsAsync(string userId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return [];

        var goals = await db.SavingsGoals
            .Include(g => g.Contributions)
                .ThenInclude(c => c.Contributor)
            .Where(g => g.EventId == ev.Id)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();

        return goals.Select(g => new SavingsGoalResponse(
            Id: g.Id,
            EventId: g.EventId,
            Title: g.Title,
            TargetAmount: g.TargetAmount,
            CurrentAmount: g.CurrentAmount,
            CreatedAt: g.CreatedAt,
            UpdatedAt: g.UpdatedAt,
            Contributions: g.Contributions
                .OrderByDescending(c => c.ContributedAt)
                .Select(c => new SavingsContributionResponse(
                    Id: c.Id,
                    GoalId: c.GoalId,
                    ContributorId: c.ContributorId,
                    ContributorName: c.Contributor.Name,
                    ContributorRole: c.Contributor.Role,
                    Amount: c.Amount,
                    ContributedAt: c.ContributedAt
                ))
                .ToList()
        )).ToList();
    }

    public async Task<SavingsGoalResponse?> CreateGoalAsync(string userId, CreateSavingsGoalRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var goal = new SavingsGoal
        {
            EventId = ev.Id,
            Title = req.Title,
            TargetAmount = req.TargetAmount,
            CurrentAmount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.SavingsGoals.Add(goal);
        await db.SaveChangesAsync();

        return new SavingsGoalResponse(
            Id: goal.Id,
            EventId: goal.EventId,
            Title: goal.Title,
            TargetAmount: goal.TargetAmount,
            CurrentAmount: goal.CurrentAmount,
            CreatedAt: goal.CreatedAt,
            UpdatedAt: goal.UpdatedAt,
            Contributions: []
        );
    }

    public async Task<SavingsGoalResponse?> UpdateGoalAsync(string userId, string goalId, UpdateSavingsGoalRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var goal = await db.SavingsGoals
            .Include(g => g.Contributions)
                .ThenInclude(c => c.Contributor)
            .FirstOrDefaultAsync(g => g.Id == goalId && g.EventId == ev.Id);

        if (goal is null) return null;

        goal.Title = req.Title;
        goal.TargetAmount = req.TargetAmount;
        goal.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new SavingsGoalResponse(
            Id: goal.Id,
            EventId: goal.EventId,
            Title: goal.Title,
            TargetAmount: goal.TargetAmount,
            CurrentAmount: goal.CurrentAmount,
            CreatedAt: goal.CreatedAt,
            UpdatedAt: goal.UpdatedAt,
            Contributions: goal.Contributions
                .OrderByDescending(c => c.ContributedAt)
                .Select(c => new SavingsContributionResponse(
                    Id: c.Id,
                    GoalId: c.GoalId,
                    ContributorId: c.ContributorId,
                    ContributorName: c.Contributor.Name,
                    ContributorRole: c.Contributor.Role,
                    Amount: c.Amount,
                    ContributedAt: c.ContributedAt
                ))
                .ToList()
        );
    }

    public async Task<bool> DeleteGoalAsync(string userId, string goalId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var goal = await db.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.EventId == ev.Id);

        if (goal is null) return false;

        db.SavingsGoals.Remove(goal);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<SavingsContributionResponse?> CreateContributionAsync(string userId, string goalId, CreateContributionRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var goal = await db.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.EventId == ev.Id);

        if (goal is null) return null;

        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;

        var contribution = new SavingsContribution
        {
            GoalId = goalId,
            ContributorId = userId,
            Amount = req.Amount,
            ContributedAt = DateTime.UtcNow
        };

        db.SavingsContributions.Add(contribution);
        await db.SaveChangesAsync();

        // Recalculate CurrentAmount
        goal.CurrentAmount = await db.SavingsContributions
            .Where(c => c.GoalId == goal.Id)
            .SumAsync(c => c.Amount);
        goal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new SavingsContributionResponse(
            Id: contribution.Id,
            GoalId: contribution.GoalId,
            ContributorId: contribution.ContributorId,
            ContributorName: user.Name,
            ContributorRole: user.Role,
            Amount: contribution.Amount,
            ContributedAt: contribution.ContributedAt
        );
    }

    public async Task<bool> DeleteContributionAsync(string userId, string contributionId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var contribution = await db.SavingsContributions
            .Include(c => c.SavingsGoal)
            .FirstOrDefaultAsync(c => c.Id == contributionId);

        if (contribution is null) return false;
        if (contribution.SavingsGoal.EventId != ev.Id) return false;

        var goalId = contribution.GoalId;

        db.SavingsContributions.Remove(contribution);
        await db.SaveChangesAsync();

        // Recalculate CurrentAmount
        var goal = await db.SavingsGoals.FindAsync(goalId);
        if (goal is not null)
        {
            goal.CurrentAmount = await db.SavingsContributions
                .Where(c => c.GoalId == goal.Id)
                .SumAsync(c => c.Amount);
            goal.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return true;
    }
}
