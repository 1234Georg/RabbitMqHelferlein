@echo off
echo Starting RabbitMQ Event Consumer...
echo.
echo Make sure RabbitMQ is running before starting the consumer.
echo.
echo Options:
echo   1. Start Consumer (default)
echo   2. Publish Test Events
echo.
set /p choice="Enter your choice (1 or 2): "

if "%choice%"=="2" (
    echo Publishing test events...
    dotnet run publish
) else (
    echo Starting consumer...
    dotnet run
)

pause
