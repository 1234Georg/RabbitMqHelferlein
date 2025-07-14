@echo off
echo RabbitMQ Event Consumer Demo
echo ============================
echo.
echo This demo will:
echo 1. Start the publisher to send test events
echo 2. Show you how to start the consumer
echo 3. Demonstrate the event storage features
echo.
echo Press any key to start the demo...
pause > nul

echo.
echo Step 1: Publishing test events...
echo.
dotnet run publish

echo.
echo Step 2: The events have been published to RabbitMQ.
echo        Now start the consumer in a separate terminal with: dotnet run
echo        Or use the run.bat script.
echo.
echo Step 3: While the consumer is running, try these commands:
echo        [S] - View statistics about consumed events
echo        [H] - View history of the last 10 events
echo        [C] - Clear the event history from memory
echo.
echo The consumer stores all events in memory for analysis.
echo Events include timestamps, message content, headers, and processing status.
echo.
echo Press any key to exit...
pause > nul
