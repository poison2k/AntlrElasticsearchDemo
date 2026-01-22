namespace SearchApi.Models;
public sealed class Document
{
    public String Id { get; init; } = Guid.NewGuid().ToString("N");
    public String Title { get; set; } = String.Empty; 
    public String Description { get; set; } = String.Empty;
    public String Text { get; set; } = String.Empty;
}