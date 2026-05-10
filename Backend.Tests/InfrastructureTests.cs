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
        Assert.That(s1, Is.SameAs(s2));
    }

    [Test]
    public void Proxy_CachesResult()
    {
        var mockStrategy = new Mock<ICompatibilityStrategy>();
        
        var dummyResult = new CompatibilityResult { IsSafe = true };
        mockStrategy.Setup(s => s.Check(It.IsAny<Product>(), It.IsAny<Product>())).Returns(dummyResult);
    
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
        var mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));

        var realStrategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance, mockLocalizer.Object);
        var decorator = new DiagnosticDecorator(realStrategy, NullLogger<DiagnosticDecorator>.Instance);
    
        var p1 = new Product { Name = "P1", Ingredients = new List<Ingredient>() };
        var p2 = new Product { Name = "P2", Ingredients = new List<Ingredient>() };
    
        var expected = realStrategy.Check(p1, p2);
        var actual = decorator.Check(p1, p2);
        
        Assert.That(actual.IsSafe, Is.EqualTo(expected.IsSafe));
    }
    
    [Test]
    public void Facade_AnalyzeRoutine_ReturnsReport()
    {
        var mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        
        mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("key", "value text"));
        mockLocalizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, $"mock {key}"));
        
        var strategy = new SimpleCompatibilityStrategy(
            NullLogger<SimpleCompatibilityStrategy>.Instance, 
            mockLocalizer.Object
        );

        var facade = new SkincareFacade(
            strategy, 
            NullLogger<SkincareFacade>.Instance, 
            mockLocalizer.Object, 
            NullLogger<DiagnosticDecorator>.Instance
        );

        var routine = new Routine("Test Routine");
        routine.Add(new Product { Name = "P1", Ingredients = new List<Ingredient>() });
        routine.Add(new Product { Name = "P2", Ingredients = new List<Ingredient>() });
     
        var result = facade.SimpleCheck(routine);
     
        Assert.That(result, Is.Not.Null.Or.Empty);
    }
    
    [Test]
    public void Observer_ReceivesNotification()
    {
        var notifier = new ConflictNotifier();
        var mockObserver = new Mock<IObserver>();
        notifier.Attach(mockObserver.Object);
        
        notifier.Notify("Conflict!");
        mockObserver.Verify(o => o.Update("Conflict!"), Times.Once);
    }
    
    [Test]
    public void ReportGenerator_CreatesFullReport_WithCorrectStructure()
    {
        var mockLocalizer = new Mock<IStringLocalizer>();
        
        mockLocalizer.Setup(l => l["RoutineHeader"]).Returns(new LocalizedString("RoutineHeader", "HEADER"));
        mockLocalizer.Setup(l => l["RoutineFooter"]).Returns(new LocalizedString("RoutineFooter", "FOOTER"));
        mockLocalizer.Setup(l => l["AllGood"]).Returns(new LocalizedString("AllGood", "BODY_OK"));

        var reportGen = new SimpleTextReport(mockLocalizer.Object);
        
        var dummyResult = new CompatibilityResult { IsSafe = true, Warnings = new List<string>() };
        var report = reportGen.CreateFullReport(dummyResult);
        
        Assert.Multiple(() =>
        {
            Assert.That(report, Contains.Substring("HEADER"));
            Assert.That(report, Contains.Substring("BODY_OK"));
            Assert.That(report, Contains.Substring("FOOTER"));
        });
    }
}