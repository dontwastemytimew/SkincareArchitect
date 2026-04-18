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
            mockStrategy.Setup(s => s.Check(It.IsAny<Product>(), It.IsAny<Product>())).Returns(true);
        
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
    public void Facade_AnalyzeRoutine_ReturnsReport()
    {
        var strategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance);
        var mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("key", "value text"));

        var facade = new SkincareFacade(
            strategy, 
            NullLogger<SkincareFacade>.Instance, 
            mockLocalizer.Object, 
            NullLogger<DiagnosticDecorator>.Instance
        );

        var routine = new Routine("Test Routine");
        routine.Add(new Product { Name = "P1" });
        routine.Add(new Product { Name = "P2" });
        
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