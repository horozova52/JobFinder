using JobFinder.Shared.DTOs.Candidates;
using MediatR;

namespace JobFinder.UseCases.Features.Candidates.Commands.UpdateProfile;

public record UpdateCandidateProfileCommand(
    string UserId,
    UpdateCandidateProfileDto Dto) : IRequest<UpdateProfileResult>;

public record UpdateProfileResult(
    bool Success,
    string? Message,
    CandidateProfileDto? Data);