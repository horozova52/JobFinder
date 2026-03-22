using JobFinder.Shared.DTOs.Candidates;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Queries.GetProfile;

public record GetCandidateProfileQuery(string UserId) : IRequest<GetProfileResult>;

public record GetProfileResult(
    bool Success,
    string? Message,
    CandidateProfileDto? Data);