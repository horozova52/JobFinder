using MediatR;

namespace JobFinder.UseCases.Features.Jobs.Commands.Delete;

public record DeleteCommand(int Id) : IRequest<bool>;