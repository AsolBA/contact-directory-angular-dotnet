using System.Text.Json.Serialization;
namespace KisiRehberiApi;

public class Occupation
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    [JsonIgnore]
    public List<Contact> Contacts { get; set; } = new();
}