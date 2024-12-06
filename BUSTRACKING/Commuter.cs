using BusTrackingSystem;
using Spectre.Console;

public class Commuter : User
{
    public void TrackBus(Bus bus)
    {
        // Determine direction
        string direction;
        if (bus.IsOnRoute)
        {
            if (bus.IsReverse)
                direction = "To City Bus Terminal";
            else
                direction = "From City Bus Terminal";
        }
        else
        {
            direction = "Bus Station";
        }

        // Determine current location
        string currentLocation = bus.IsOnRoute
            ? $"{bus.CurrentLocation} at ({bus.LastUpdatedTime:hh:mm tt})"
            : "Not on trip";

        // Create a new table
        var table = new Table();

        // Add columns
        table.AddColumn(new TableColumn("Property").Centered());
        table.AddColumn(new TableColumn("Details").Centered());

        // Set table properties
        table.Border = TableBorder.Rounded;
        table.Title = new TableTitle($"[yellow]Bus Information for {bus.BusNumber}[/]");
        table.Alignment = Justify.Left;

        // Add rows for bus information
        table.AddRow("Route", bus.Route);
        table.AddRow("Direction", direction);
        table.AddRow("Total Capacity", bus.Capacity.ToString());
        table.AddRow("Available Seats", (bus.Capacity - bus.CurrentPassengers).ToString());
        table.AddRow("Current Location", currentLocation);
        table.AddRow("Traffic Condition", bus.Traffic.ToString());
        table.AddRow("Driver Name", bus.DriverName);
        table.AddRow("Driver Phone Number", bus.DriverPhoneNumber);
        table.AddRow("Attendant Name", bus.AttendantName);

        // Render the table to the console
        AnsiConsole.Write(table);
      
    }

    public void TrackAllBuses(List<Bus> buses)
    {
        foreach (var bus in buses)
            TrackBus(bus);
    }

    public void CalculateFare(List<Bus> buses, List<Route> routes)
    {
        // Ask for the bus number
        string busNumber = AnsiConsole.Ask<string>("Enter [blue]bus number[/] you are riding: ");
        Bus selectedBus = buses.Find(b => b.BusNumber == busNumber);

        if (selectedBus == null)
        {
            AnsiConsole.MarkupLine("[red]Bus not found.[/]");
            Helper.Pause();
            return;
        }

        // Find the route of the selected bus
        Route route = routes.Find(r => r.Name == selectedBus.Route);

        if (route == null)
        {
            AnsiConsole.MarkupLine("[red]Route not found for the selected bus.[/]");
            Helper.Pause();
            return;
        }

        // Adjust the stop list based on the bus direction
        List<Stop> stops = selectedBus.IsReverse ? new List<Stop>(route.Stops.AsEnumerable().Reverse()) : route.Stops;

        // Display the route stops using a table
        var table = new Table
        {
            Title = new TableTitle("[green]Route Stops[/]"),
            Border = TableBorder.Rounded
        };
        table.AddColumn("Stop No.");
        table.AddColumn("Location");

        for (int i = 0; i < stops.Count; i++)
        {
            table.AddRow(
                (i + 1).ToString(),
                stops[i].Location
                
            );
        }

        AnsiConsole.Write(table);

        // Get the starting point and destination from the user
        int startStop = AnsiConsole.Ask<int>("Where did you start riding the bus ([green]Stop No.[/]): ");
        int endStop = AnsiConsole.Ask<int>("Where will you get down ([green]Stop No.[/]): ");

        // Validate stop inputs
        if (startStop < 1 || startStop > stops.Count || endStop < 1 || endStop > stops.Count)
        {
            AnsiConsole.MarkupLine("[red]Invalid stop selection. Please try again.[/]");
            Helper.Pause();
            return;
        }

        if (endStop <= startStop)
        {
            AnsiConsole.MarkupLine("[red]Destination must be after the starting point. Please try again.[/]");
            Helper.Pause();
            return;
        }

        // Calculate the fare
        double startDistance = stops[startStop - 1].Distance;
        double endDistance = stops[endStop - 1].Distance;
        double distanceTravelled = Math.Abs(endDistance - startDistance);

        double fare = distanceTravelled <= 5
            ? 12
            : 12 + ((distanceTravelled - 5) * 2.25);

        AnsiConsole.MarkupLine($"\nThe total fare from [yellow]{stops[startStop - 1].Location}[/] to [yellow]{stops[endStop - 1].Location}[/] is [green]P{fare:0.00}[/].");
        Helper.Pause();
    }

    public override void ReportEmergency()
    {
        Console.WriteLine("Commuter has reported an emergency.");
        DisplayEmergencyContacts();
        Helper.Pause();
    }
}