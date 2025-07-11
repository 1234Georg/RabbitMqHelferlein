@echo off
echo RabbitMQ Event Consumer
echo ======================
echo.
echo Make sure RabbitMQ is running before starting the consumer.
echo.
echo Options:
echo   1. Start Consumer (with event storage)
echo   2. Publish Test Events
echo   3. Show Event History
echo   4. Exit
echo.
set /p choice="Enter your choice (1-4): "

if "%choice%"=="2" (
    echo.
    echo Publishing test events...
    dotnet run publish
    goto end
)

if "%choice%"=="3" (
    echo.
    echo Showing event history...
    dotnet run history
    goto end
)

if "%choice%"=="4" (
    echo.
    echo Goodbye!
    goto end
)

echo.
echo Starting consumer with event storage...
echo.
echo Interactive Commands:
echo   [S] - Show statistics
echo   [H] - Show event history
echo   [C] - Clear event history
echo   [ENTER] - Exit
echo.
dotnet run

:end
echo.
pause
