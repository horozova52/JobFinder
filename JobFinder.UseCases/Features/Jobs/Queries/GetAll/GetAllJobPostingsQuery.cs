using JobFinder.Shared.DTOs.Jobs;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetAll;

public record GetAllJobPostingsQuery() : IRequest<IEnumerable<JobPostingDto>>;