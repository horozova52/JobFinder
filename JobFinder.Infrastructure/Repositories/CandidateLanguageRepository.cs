using JobFinder.Core.Entities.Candidates;
using JobFinder.Infrastructure.Data;
using JobFinder.UseCases.Contracts;
using Microsoft.EntityFrameworkCore;

namespace JobFinder.Infrastructure.Repositories;

public class CandidateLanguageRepository : ICandidateLanguageRepository
{
    private readonly ApplicationDbContext _context;

    public CandidateLanguageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateLanguage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateLanguages
            .Include(l => l.Language)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<CandidateLanguage> CreateAsync(CandidateLanguage language, CancellationToken cancellationToken = default)
    {
        _context.CandidateLanguages.Add(language);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.CandidateLanguages
            .Include(l => l.Language)
            .FirstAsync(l => l.Id == language.Id, cancellationToken);
    }

    public async Task UpdateAsync(CandidateLanguage language, CancellationToken cancellationToken = default)
    {
        _context.CandidateLanguages.Update(language);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lang = await _context.CandidateLanguages.FindAsync([id], cancellationToken);
        if (lang != null)
        {
            _context.CandidateLanguages.Remove(lang);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, int candidateProfileId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateLanguages
            .AnyAsync(l => l.Id == id && l.CandidateProfileId == candidateProfileId, cancellationToken);
    }

    public async Task<bool> AlreadyHasLanguageAsync(int candidateProfileId, int languageId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateLanguages
            .AnyAsync(l => l.CandidateProfileId == candidateProfileId && l.LanguageId == languageId, cancellationToken);
    }
}