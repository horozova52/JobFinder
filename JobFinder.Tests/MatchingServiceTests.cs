using JobFinder.Core.Entities.Candidates;
using JobFinder.Core.Entities.Jobs;
using JobFinder.Shared.Enums;
using JobFinder.UseCases.Services;
using Xunit;

namespace JobFinder.Tests;

public class MatchingServiceTests
{
    private readonly MatchingService _matchingService;

    public MatchingServiceTests()
    {
        _matchingService = new MatchingService();
    }

    [Fact]
    public void CalculateMatchScore_AllSkillsMatch_Returns100()
    {
        var candidate = new CandidateProfile
        {
            Skills = new List<CandidateSkill>
            {
                new() { SkillId = 1, Level = SkillLevel.Advanced }
            }
        };

        var job = new JobPosting
        {
            Skills = new List<JobSkill>
            {
                new() { SkillId = 1, RequiredLevel = SkillLevel.Advanced }
            }
        };

        var score = _matchingService.CalculateMatchScore(candidate, job);

        Assert.Equal(100, score);
    }

    [Fact]
    public void CalculateMatchScore_NoSkillsMatch_Returns0()
    {
        var candidate = new CandidateProfile
        {
            Skills = new List<CandidateSkill>()
        };

        var job = new JobPosting
        {
            Skills = new List<JobSkill>
            {
                new() { SkillId = 1, RequiredLevel = SkillLevel.Advanced },
                new() { SkillId = 2, RequiredLevel = SkillLevel.Intermediate }
            }
        };

        var score = _matchingService.CalculateMatchScore(candidate, job);

        Assert.Equal(0, score);
    }

    [Fact]
    public void CalculateMatchScore_PartialLevelMatch_ReturnsProportionalScore()
    {
        var candidate = new CandidateProfile
        {
            Skills = new List<CandidateSkill>
            {
                new() { SkillId = 1, Level = SkillLevel.Beginner }
            }
        };

        var job = new JobPosting
        {
            Skills = new List<JobSkill>
            {
                new() { SkillId = 1, RequiredLevel = SkillLevel.Expert }
            }
        };

        var score = _matchingService.CalculateMatchScore(candidate, job);

        Assert.Equal(0, score);
    }

    [Fact]
    public void CalculateMatchScore_JobWithNoSkills_Returns0()
    {
        var candidate = new CandidateProfile
        {
            Skills = new List<CandidateSkill>
            {
                new() { SkillId = 1, Level = SkillLevel.Expert }
            }
        };

        var job = new JobPosting
        {
            Skills = new List<JobSkill>()
        };

        var score = _matchingService.CalculateMatchScore(candidate, job);

        Assert.Equal(0, score);
    }
}