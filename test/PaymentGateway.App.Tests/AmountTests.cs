using FluentAssertions;

using PaymentGateway.App.Models.ValueTypes;

namespace PaymentGateway.App.Tests;

public class AmountTests
{
    [Theory]
    [InlineData(100, Currency.GbpValue)]
    [InlineData(100, Currency.EurValue)]
    [InlineData(100, Currency.UsdValue)]
    public void New_Successful(int amountInMinor, string currency)
    {
        Amount
            .New(amountInMinor, currency)
            .IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(-100, Currency.GbpValue, "Invalid amount")]
    [InlineData(100, "", "Missing field: Currency")]
    [InlineData(100, null, "Missing field: Currency")]
    [InlineData(100, "KLM", "Invalid currency code")]
    public void Creation_Rules(int amountInMinor, string? currency, string error)
    {
        Amount
            .New(amountInMinor, currency!)
            .IsInvalid(out var message).Should().BeTrue();

        message.Should().Contain(error);
    }
}