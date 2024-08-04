namespace WebApi.Models;

public class GenerateRequestModel
{
    public byte Length { get; set; }
    
    public bool UseUppercase { get; set; }
    
    public bool UseNumber { get; set; }
    
    public bool UseSpecific { get; set; }
    
    // TODO: добавить особенные специальные символы
}