namespace KisiRehberiApi;


public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? City {get; set;} 
    public int? OccupationId{get; set;}//null olabilir(meslek seçmeden de kayıt yapılması için)
    public Occupation? Occupation {get;set;}//OccupationId üzerinden meslek nesnesine geçiş için 
}