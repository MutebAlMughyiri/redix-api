namespace Redix.Api.Attachments;

/// <summary>Where the API keeps files. The folder is set with <c>REDIX_Attachments__Folder</c>; it is never in the repo, and the daily backup has to include it (NFR-10).</summary>
public class AttachmentOptions
{
    /// <summary>The folder files are kept under; by default one in the local application data of the user running the API.</summary>
    public string Folder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Redix", "Attachments");

    /// <summary>The full path of the file of <paramref name="attachment"/>.</summary>
    public string PathOf(Attachment attachment) => Path.Combine(Folder, attachment.StoragePath);
}
