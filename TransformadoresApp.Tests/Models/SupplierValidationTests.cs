using System.ComponentModel.DataAnnotations;
using TransformadoresApp.Models.Purchasing;

namespace TransformadoresApp.Tests.Models;

public class SupplierValidationTests
{
    [Theory]
    [InlineData("12345678", true)]
    [InlineData("+54 351 123-4567", true)]
    [InlineData("1234567", false)]
    [InlineData("teléfono12345678", false)]
    public void Phone_ValidatesAllowedCharactersAndDigitCount(
        string phone,
        bool expectedValid)
    {
        var supplier = new Supplier { Phone = phone };
        var validationContext = new ValidationContext(supplier)
        {
            MemberName = nameof(Supplier.Phone)
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(
            supplier.Phone,
            validationContext,
            validationResults);

        Assert.Equal(expectedValid, isValid);
    }
}
