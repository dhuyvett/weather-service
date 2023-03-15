using Microsoft.AspNetCore.Mvc;

namespace WeatherService.Controllers;

public class WeatherForecastControllerImpl : WeatherForecastControllerBase
{
    public override async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecast(string city, Units? units, CancellationToken cancellationToken)
    {
        // TODO: actually get the forecast
        await Task.Yield();

        // For now, just return dummy data
        var forecast = new WeatherForecast()
        {
            Date = DateTime.Now,
            LowTemperature = 0,
            HighTemperature = 100,
            Summary = "placeholder"
        };
        var result = new List<WeatherForecast> { forecast };
        return Ok(result);
    }
}
