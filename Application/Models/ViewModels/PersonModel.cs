using Application.Models.Entities;

namespace Application.Models.ViewModels;

public class PersonModel
{
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public CertificateModel Certificate { get; set; } = new();
    
    public List<AddressModel> Addresses { get; set; } = new();
    
    public static implicit operator Application.Models.Entities.PersonEntity(PersonModel person)
    {
        return new Application.Models.Entities.PersonEntity
        {
            Email = person.Email,
            Age = person.Age,
            Name = person.Name,
            Certificate = person.Certificate,
            Addresses = person.Addresses.Select(a=> (Application.Models.Entities.AddressEntity) a).ToList()
            
        };
    }
}

public class CertificateModel
{
    public string CertificateId { get; set; } = string.Empty;
    public string CertificateName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    
    public static implicit operator Application.Models.Entities.CertificateEntity(CertificateModel model)
    {
        return new Application.Models.Entities.CertificateEntity
        {
            CertificateId = model.CertificateId,
            CertificateName = model.CertificateName,
            ExpiryDate = model.ExpiryDate
        };
    }
}

public class AddressModel
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    
    public static implicit operator Application.Models.Entities.AddressEntity(AddressModel model)
    {
        return new Application.Models.Entities.AddressEntity
        {
            Street = model.Street,
            City = model.City,
            ZipCode = model.ZipCode
        };
    }
}
