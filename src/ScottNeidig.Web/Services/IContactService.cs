namespace ScottNeidig.Web.Services;

public interface IContactService
{
    /// <summary>
    /// Persists the enquiry and returns its id. Saving is the reliable path: a lead is stored
    /// before any email is attempted, so a mail outage never loses one. Email notification is
    /// layered on top of this, not in place of it.
    /// </summary>
    Task<int> CreateAsync(string name, string email, string message, CancellationToken ct = default);

    /// <summary>Every enquiry, newest first, for the admin inbox.</summary>
    Task<List<ContactMessageSummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>False when the message no longer exists. For clearing spam that slips through.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
