using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Services;

public class ContactService : IContactService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactService> _log;

    public ContactService(AppDbContext db, ILogger<ContactService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<int> CreateAsync(string name, string email, string message, CancellationToken ct = default)
    {
        var enquiry = new ContactMessage
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Message = message.Trim(),
            CreatedUtc = DateTime.UtcNow
        };

        _db.ContactMessages.Add(enquiry);
        await _db.SaveChangesAsync(ct);

        // A lead is money. Logged at Information so it's visible even if the admin inbox
        // isn't checked and email notification isn't wired up yet.
        _log.LogInformation("Contact enquiry {Id} from {Email}", enquiry.Id, enquiry.Email);

        return enquiry.Id;
    }

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
