namespace ScottNeidig.Web.Services;

/// <summary>
/// Read and delete only. The public contact form was removed, so nothing writes enquiries
/// any more; this exists to read and clear the ones already in the table.
/// </summary>
public interface IContactService
{
    /// <summary>Every enquiry, newest first, for the admin inbox.</summary>
    Task<List<ContactMessageSummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>False when the message no longer exists. For clearing spam that slips through.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
