using JobFinder.Shared.DTOs.Jobs;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetJobFeed;

public record GetJobFeedQuery(
    string? UserId,     
    string? Search,
    string? Category,
    string? JobType,
    string? EmploymentType,
    string? Location,
    int PageSize = 20
) : IRequest<GetJobFeedResult>;

public record GetJobFeedResult(
    List<JobFeedItemDto> Items,
    int TotalCount);