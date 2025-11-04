using Application.Models.Entities;

namespace Application.Models.ViewModels;

public class PersonModel
{
    public string Email { get; set; }
    public int Age { get; set; }
    public string Name { get; set; }
    
    public CertificateModel Certificate { get; set; }
    
    public List<AddressModel> Addresses { get; set; } = new();
    
    public static implicit operator PersonEntity(PersonModel person)
    {
        return new PersonEntity
        {
            Email = person.Email,
            Age = person.Age,
            Name = person.Name,
            Certificate = person.Certificate,
            Addresses = person.Addresses.Select(a=> (AddressEntity) a).ToList()
            
        };
    }
}

public class CertificateModel
{
    public string CertificateId { get; set; }
    public string CertificateName { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public static implicit operator CertificateEntity(CertificateModel model)
    {
        return new CertificateEntity
        {
            CertificateId = model.CertificateId,
            CertificateName = model.CertificateName,
            ExpiryDate = model.ExpiryDate
        };
    }
}

public class AddressModel
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    
    public static implicit operator AddressEntity(AddressModel model)
    {
        return new AddressEntity
        {
            Street = model.Street,
            City = model.City,
            ZipCode = model.ZipCode
        };
    }
}
