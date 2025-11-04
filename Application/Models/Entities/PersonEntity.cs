using Application.Models.ViewModels;

namespace Application.Models.Entities;

public partial class PersonEntity
{
    public string Email { get; set; }
    public int Age { get; set; }
    public string Name { get; set; }
    
    public CertificateEntity Certificate { get; set; }
    
    public List<AddressEntity> Addresses { get; set; } = new();
        
    public static implicit operator PersonModel(PersonEntity person)
    {
        return new PersonModel
        {
            Email = person.Email,
            Age = person.Age,
            Name = person.Name,
            Certificate = person.Certificate,
            Addresses = person.Addresses.Select(a=> (AddressModel)a).ToList()
            
        };
    }
}

public class CertificateEntity
{
    public string CertificateId { get; set; }
    public string CertificateName { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public static implicit operator CertificateModel(CertificateEntity model)
    {
        return new CertificateModel
        {
            CertificateId = model.CertificateId,
            CertificateName = model.CertificateName,
            ExpiryDate = model.ExpiryDate
        };
    }
}

public class AddressEntity
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    
    public static implicit operator AddressModel(AddressEntity model)
    {
        return new AddressModel
        {
            Street = model.Street,
            City = model.City,
            ZipCode = model.ZipCode
        };
    }
}