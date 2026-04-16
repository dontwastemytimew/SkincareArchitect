using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Localization;
using NUnit.Framework;
using Moq;

namespace Backend.Tests;

[TestFixture]
public class InfrastructureTests
{
    [Test]
    public void Singleton_AlwaysReturnsSameInstance()
    {
        var s1 = SystemSettings.GetInstance(NullLogger<SystemSettings>.Instance);
        var s2 = SystemSettings.GetInstance(NullLogger<SystemSettings>.Instance);

        Assert.That(s1, Is.SameAs(s2), "Singleton має повертати один і той самий об'єкт");
    }

    [Test]
    public void Proxy_CachesResult_AndCallsRealStrategyOnlyOnce()
    {
        var mockStrategy = new Mock<ICompatibilityStrategy>();
        
        mockStrategy
            .Setup(s => s.Check(It.IsAny<Product>(), It.IsAny<Product>()))
            .Returns(true);
        
        var proxy = new CompatibilityProxy(mockStrategy.Object);
    
        var p1 = new Product { Name = "A" };
        var p2 = new Product { Name = "B" };
        
        proxy.Check(p1, p2);
        proxy.Check(p1, p2);
        
        mockStrategy.Verify(s => s.Check(It.IsAny<Product>(), It.IsAny<Product>()), Times.Once);
    }
    
    [Test]
    public void Decorator_ReturnsCorrectResultFromInnerStrategy()
    {
        var realStrategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance);
        var decorator = new DiagnosticDecorator(realStrategy, NullLogger<DiagnosticDecorator>.Instance);
    
        var p1 = new Product();
        var p2 = new Product();
    
        var expected = realStrategy.Check(p1, p2);
        var actual = decorator.Check(p1, p2);
    
        Assert.That(actual, Is.EqualTo(expected), "Декоратор не має змінювати логіку роботи стратегії");
    }
    
    [Test]
    public void Facade_SimpleCheck_ReturnsNonEmptyReport()
    {
        var strategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance);
        var mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        
        mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("key", "value"));

        var facade = new SkincareFacade(
            strategy, 
            NullLogger<SkincareFacade>.Instance, 
            mockLocalizer.Object, 
            NullLogger<DiagnosticDecorator>.Instance
        );

        var p1 = new Product { Name = "P1" };
        var p2 = new Product { Name = "P2" };
        
        var result = facade.SimpleCheck(p1, p2);
        
        Assert.That(result, Is.Not.Null.Or.Empty, "Фасад має повертати сформований звіт");
    }
    
    [Test]
    public void Observer_ReceivesNotification_OnConflict()
    {
        var notifier = new ConflictNotifier();
        var mockObserver = new Mock<IObserver>();
        notifier.Attach(mockObserver.Object);
        string testMessage = "Conflict detected!";
        
        notifier.Notify(testMessage);
        
        mockObserver.Verify(o => o.Update(testMessage), Times.Once, "Спостерігач мав отримати повідомлення один раз");
    }
    
    [Test]
    public void ReportGenerator_CreatesFullReport_WithCorrectStructure()
    {
        var mockLocalizer = new Mock<IStringLocalizer>();
        
        mockLocalizer.Setup(l => l["RoutineHeader"]).Returns(new LocalizedString("RoutineHeader", "HEADER"));
        mockLocalizer.Setup(l => l["RoutineFooter"]).Returns(new LocalizedString("RoutineFooter", "FOOTER"));
        mockLocalizer.Setup(l => l["Compatible"]).Returns(new LocalizedString("Compatible", "BODY_OK"));

        var reportGen = new SimpleTextReport(mockLocalizer.Object);
        
        var report = reportGen.CreateFullReport("Compatible");
        
        Assert.Multiple(() =>
        {
            Assert.That(report, Contains.Substring("HEADER"));
            Assert.That(report, Contains.Substring("BODY_OK"));
            Assert.That(report, Contains.Substring("FOOTER"));
        });
    }
}