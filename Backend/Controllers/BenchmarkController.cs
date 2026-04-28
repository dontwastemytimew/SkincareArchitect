using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;

namespace Backend.Controllers;

/// <summary>
/// API-контролер для проведення бенчмаркінгу (вимірювання продуктивності).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BenchmarkController : ControllerBase
{
    private readonly SephoraDataService _dataService;

    /// <summary>
    /// Ініціалізує контролер та підключає сервіс роботи з великими даними.
    /// </summary>
    public BenchmarkController()
    {
        _dataService = new SephoraDataService("Data/product_info.csv");
    }

    /// <summary>
    /// Запускає порівняльний аналіз послідовного та паралельного алгоритмів.
    /// Виконує вимогу порівняння часу виконання для різних потоків.
    /// </summary>
    /// <returns>JSON з метриками продуктивності (час виконання, прискорення, кількість ядер).</returns>
    [HttpGet("run")]
    public IActionResult RunBenchmark()
    {
        var products = _dataService.LoadProducts();
        int count = products.Count;
        
        long seqTime = _dataService.ParseSequential(new List<SephoraProduct>(products));
        
        long parTime = _dataService.ParseParallel(new List<SephoraProduct>(products));

        return Ok(new
        {
            TotalProducts = count,
            SequentialTimeMs = seqTime,
            ParallelTimeMs = parTime,
            SpeedUp = Math.Round((double)seqTime / parTime, 2) + "x",
            CoresUsed = Environment.ProcessorCount
        });
    }
}