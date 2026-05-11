using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Jobs;

namespace JobFinder.UseCases.Services;

public class MatchingService
{ 
    public double CalculateMatchScore(CandidateProfile candidate, JobPosting job)
    {
        if (job.Skills == null || !job.Skills.Any())
            return 0;

        double totalScore = 0;

        foreach (var jobSkill in job.Skills)
        {
            var candidateSkill = candidate.Skills?
                .FirstOrDefault(s => s.SkillId == jobSkill.SkillId);
            if (candidateSkill == null)
                continue;
            double levelScore = candidateSkill.Level >= jobSkill.RequiredLevel
                ? 1.0
                : (double)candidateSkill.Level / (double)jobSkill.RequiredLevel;

            totalScore += levelScore;
        }

        return (totalScore / job.Skills.Count) * 100;
    }
}