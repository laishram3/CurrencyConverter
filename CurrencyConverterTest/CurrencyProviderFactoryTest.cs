public class CurrencyProviderFactoryTests
{
    [Fact]
    public void GetProvider_ReturnsFrankfurterProvider_WhenProviderNameIsFrankfurter()
    {
        // Arrange        
        var mockProvider = new Mock<FrankfurterCurrencyProvider>(
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
            Microsoft.Extensions.Options.Options.Create(new FrankfurterSettings())
        );

        var serviceProviderMock = new Mock<IServiceProvider>();
       
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(FrankfurterCurrencyProvider)))
            .Returns(mockProvider.Object);

        var factory = new CurrencyProviderFactory(serviceProviderMock.Object);

        // Act
        var provider = factory.GetProvider("frankfurter");

        // Assert
        Assert.NotNull(provider);
        Assert.IsAssignableFrom<FrankfurterCurrencyProvider>(provider); // works with Moq proxy
    }



    [Fact]
    public void GetProvider_ThrowsNotSupportedException_WhenProviderNameIsUnknown()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var factory = new CurrencyProviderFactory(serviceProviderMock.Object);

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => factory.GetProvider("unknown"));
        Assert.Equal("Provider unknown is not supported.", ex.Message);
    }    
}
