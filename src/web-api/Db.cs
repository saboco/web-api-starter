namespace web_api;

using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

public class Db
{
    private readonly IDbConnection _connection;

    public Db(string connectionString = "Data Source=:memory:")
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        SqlMapper.AddTypeHandler(new DateOnlyHandler());
        SqlMapper.AddTypeHandler(new GuidHandler());
        SqlMapper.AddTypeHandler(new TimeSpanHandler());
        Initialize();
    }

    public void InsertWeatherForecast(WeatherForecast weatherforecast)
    {

        try
        {
            _connection.Execute(
                @"INSERT INTO weather_forecast (Date, TemperatureC, TemperatureF, Country, Summary)
              VALUES (@Date, @TemperatureC, @TemperatureF, @Country,@Summary)
             ",
                new
                {
                    Date = weatherforecast.Date,
                    TemperatureC = weatherforecast.TemperatureC,
                    TemperatureF = weatherforecast.TemperatureF,
                    Country = weatherforecast.Country,
                    Summary = weatherforecast.Summary
                }
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void Initialize()
    {
        var command = _connection.Execute(
            @" 
                CREATE TABLE weather_forecast (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    TemperatureC INTEGER NOT NULL,
                    temperatureF INTEGER NOT NULL,
                    Country TEXT NOT NULL,
                    Summary TEXT NULL
                    )
            ");
    }

    public WeatherForecast[] GetWeatherForecast()
    {
        var sql = "SELECT Date, TemperatureC, Country, Summary FROM weather_forecast";
        var result = _connection.Query<WeatherForecast>(sql);
        return result.ToArray();
    }
}

abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
{
    // Parameters are converted by Microsoft.Data.Sqlite
    public override void SetValue(IDbDataParameter parameter, T? value)
        => parameter.Value = value;
}

class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
        => DateTimeOffset.Parse((string)value);
}
class DateOnlyHandler : SqliteTypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
        => DateOnly.Parse((string)value);
}

class GuidHandler : SqliteTypeHandler<Guid>
{
    public override Guid Parse(object value)
        => Guid.Parse((string)value);
}

class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
{
    public override TimeSpan Parse(object value)
        => TimeSpan.Parse((string)value);
}