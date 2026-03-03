using JobFinder.Shared.DTOs.Jobs;
using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Queries.GetById;

public record GetJobPostingByIdQuery(int Id) : IRequest<JobPostingDto?>;