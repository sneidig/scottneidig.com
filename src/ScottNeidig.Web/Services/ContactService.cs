using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ContactService : IContactService
{
    private readonly AppDbContext _db;

    public ContactService(AppDbContext db) => _db = db;

    public Task<List<ContactMessageSummary>> GetAllAsync(CancellationToken ct = default) =>
        _db.ContactMessages
            .OrderByDescending(m => m.CreatedUtc)
            .ThenByDescending(m => m.Id)
            .Select(m => new ContactMessageSummary(m.Id, m.Name, m.Email, m.Message, m.CreatedUtc))
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var message = await _db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (message is null)
        {
            return false;
        }

        _db.ContactMessages.Remove(message);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
