using JobFinder.Core.Entities.Applications;

namespace JobFinder.UseCases.Contracts;

public interface IEmploymentConfirmationRepository
{
    Task<EmploymentConfirmation?> GetByApplicationIdAsync(int applicationId, CancellationToken ct = default);
    Task<List<EmploymentConfirmation>> GetPendingByEmployerAsync(int employerProfileId, CancellationToken ct = default);
    Task<EmploymentConfirmation> CreateAsync(EmploymentConfirmation confirmation, CancellationToken ct = default);
    Task UpdateAsync(EmploymentConfirmation confirmation, CancellationToken ct = default);
    Task<bool> ExistsForApplicationAsync(int applicationId, CancellationToken ct = default);
}