using BusTrackingSystem;
using Spectre.Console;
public class BusAttendant : User
{
    private readonly Bus assignedBus;
    string StateFilePath = "Data/BusAttendantState.txt";

    public BusAttendant(Bus bus)
    {
        assignedBus = bus;
        LoadState();
    }

    public void StartRoute(List<Route> routes)
    {
        if (!assignedBus.IsOnRoute)
        {
            assignedBus.IsOnRoute = true;
            assignedBus.LastUpdatedTime = DateTime.Now;

            // Display route start message with markup
            AnsiConsole.MarkupLine($"[green]Bus {assignedBus.BusNumber} has started its route at {assignedBus.LastUpdatedTime:hh:mm tt}.[/]");

            // Ask for direction using a prompt
            var directionChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[blue]Route started. Please select travel direction:[/]")
                    .AddChoices("1. From City Bus Station", "2. To City Bus Station")
            );

            assignedBus.IsReverse = directionChoice == "2. To City Bus Station";

            // Display passenger boarding info
            AnsiConsole.MarkupLine("\n[green]--- Passenger Boarding ---[/]");
            AnsiConsole.MarkupLine($"Bus capacity: [yellow]{assignedBus.Capacity}[/]");
            AnsiConsole.MarkupLine($"Available seats: [yellow]{assignedBus.Capacity - assignedBus.CurrentPassengers}[/]");

            // Get the number of passengers boarding
            int passengers = AnsiConsole.Ask<int>("Enter the number of passengers boarding: ");
            if (passengers > (assignedBus.Capacity - assignedBus.CurrentPassengers))
            {
                AnsiConsole.MarkupLine($"[red]Cannot board {passengers} passengers. Only {assignedBus.Capacity - assignedBus.CurrentPassengers} seats available.[/]");
                passengers = 0; // No passengers board if the number exceeds capacity
            }
            else
            {
                assignedBus.CurrentPassengers += passengers;
                AnsiConsole.MarkupLine($"[green]{passengers} passengers boarded. Current passengers: [yellow]{assignedBus.CurrentPassengers}/{assignedBus.Capacity}[/].[/]");
            }


            // Find the route
            Route route = routes.Find(r => r.Name == assignedBus.Route);
            if (route != null)
            {
                // Handle stops based on direction
                List<Stop> stops = assignedBus.IsReverse ? new List<Stop>(route.Stops) : route.Stops;
                if (assignedBus.IsReverse) stops.Reverse();

                // Display route stops using a Spectre.Console table
                var table = new Table
                {
                    Title = new TableTitle("[green]Route Stops[/]"),
                    Border = TableBorder.Rounded
                };
                table.AddColumn("Stop No.");
                table.AddColumn("Location");

                for (int i = 0; i < stops.Count; i++)
                {
                    table.AddRow((i + 1).ToString(), stops[i].Location);
                }

                AnsiConsole.Write(table);

                // Select the current stop
                int choice = AnsiConsole.Ask<int>("Select stop point number: ");
                if (choice >= 1 && choice <= stops.Count)
                {
                    assignedBus.CurrentLocation = stops[choice - 1].Location;
                    AnsiConsole.MarkupLine($"[green]Current location set to: [yellow]{assignedBus.CurrentLocation}[/].[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid choice. Defaulting to the starting point.[/]");
                    assignedBus.CurrentLocation = "Starting Point";
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Route not found. Defaulting to 'Starting Point'.[/]");
                assignedBus.CurrentLocation = "Starting Point";
            }

            // Save the state
            SaveState();
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Route is already in progress.[/]");
        }
        Helper.Pause();
    }
    public void EndRoute()
    {
        if (assignedBus.IsOnRoute)
        {
            assignedBus.IsOnRoute = false;

            assignedBus.TravelHistory.Add($"Completed route {assignedBus.Route} at {DateTime.Now}");
            Console.WriteLine("Route ended.");
            assignedBus.SaveTravelHistory();
            assignedBus.CurrentLocation = "Bus Station";

            ClearState();
        }
        else
        {
            Console.WriteLine("No route in progress to end.");
        }

        Helper.Pause();
    }
    private void SaveState()
    {
        if (!File.Exists(StateFilePath))
        {
            Console.WriteLine("State file not found. Defaulting to initial state.");
            return;
        }
        try
        {
            
            // Read existing states from the file, if it exists
            var states = File.Exists(StateFilePath)
                ? File.ReadAllLines(StateFilePath)
                : Array.Empty<string>();

            // Use StreamWriter to write updated states to the file
            using (StreamWriter writer = new StreamWriter(StateFilePath))
            {
                bool stateUpdated = false;

                foreach (string state in states)
                {
                    // Check if the current state matches the bus number
                    if (state.StartsWith($"{assignedBus.BusNumber},"))
                    {
                        // Update the state for the assigned bus 
                        writer.WriteLine($"{assignedBus.BusNumber}," +
                                         $"{assignedBus.IsOnRoute}," +
                                         $"{assignedBus.CurrentLocation}," +
                                         $"{assignedBus.Route}," +
                                         $"{assignedBus.IsReverse}," +
                                         $"{assignedBus.CurrentPassengers}," +
                                         $"{assignedBus.Traffic}," +
                                         $"{DateTime.Now}");
                        stateUpdated = true;
                    }
                    else
                    {
                        // Retain existing states for other buses
                        writer.WriteLine(state);
                    }
                }

                // Add a new state if no matching state was found
                if (!stateUpdated)
                {
                    writer.WriteLine($"{assignedBus.BusNumber}," +
                                     $"{assignedBus.IsOnRoute}," +
                                     $"{assignedBus.CurrentLocation}," +
                                     $"{assignedBus.Route}," +
                                     $"{assignedBus.IsReverse}," +
                                     $"{assignedBus.CurrentPassengers}," +
                                     $"{assignedBus.Traffic}," +
                                     $"{DateTime.Now}");
                }
            }

            Console.WriteLine("State saved successfully.");
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the process
            Console.WriteLine($"Error saving state: {ex.Message}");
        }
    }

    public void LoadState()
    {
        // Return early if the state file does not exist
        if (!File.Exists(StateFilePath))
        {
            Console.WriteLine("State file not found. Defaulting to initial state.");
            return;
        }
        try
        {
            // Read and process each line from the state file
            using (StreamReader reader = new StreamReader(StateFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split the line into data fields
                    string[] data = line.Split(',');

                    // Ensure the data has at least 7 fields 
                    if (data.Length >= 7 && data[0] == assignedBus.BusNumber)
                    {
                        assignedBus.IsOnRoute = bool.TryParse(data[1], out bool isOnRoute) && isOnRoute;
                        assignedBus.CurrentLocation = data[2];
                        assignedBus.UpdateRoute(data[3]);
                        assignedBus.IsReverse = bool.TryParse(data[4], out bool isReverse) && isReverse;
                        assignedBus.CurrentPassengers = int.TryParse(data[5], out int passengers) ? passengers : 0;
                        assignedBus.Traffic = Enum.Parse<TrafficCondition>(data[6]);
                        assignedBus.LastUpdatedTime = DateTime.TryParse(data.Length > 7 ? data[7] : null, out DateTime lastUpdated)
                            ? lastUpdated
                            : DateTime.Now;
                        // Provide a summary of the loaded state to the user
                        Console.WriteLine($"Loaded state for bus {assignedBus.BusNumber}: " +
                                          $"IsOnRoute={assignedBus.IsOnRoute}, " +
                                          $"Location={assignedBus.CurrentLocation}, " +
                                          $"Route={assignedBus.Route}, " +
                                          $"Direction={(assignedBus.IsReverse ? "Reverse" : "Normal")}, " +
                                          $"Traffic={assignedBus.Traffic},"+
                                          $"Passengers={assignedBus.CurrentPassengers}," +
                                          $" LastUpdated={assignedBus.LastUpdatedTime:hh:mm tt}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions during the loading process
            Console.WriteLine($"Error loading state: {ex.Message}");
        }
    }

    private void ClearState()
    {
        
        try
        {
            // Check if the state file exists
            if (!File.Exists(StateFilePath)) return;

            // Read all existing states
            var states = File.ReadAllLines(StateFilePath);

            // Use StreamWriter to rewrite the file without the target bus state
            using (StreamWriter writer = new StreamWriter(StateFilePath))
            {
                foreach (string state in states)
                {
                    // Write all states except the one corresponding to the assigned bus
                    if (!state.StartsWith($"{assignedBus.BusNumber},"))
                    {
                        writer.WriteLine(state);
                    }
                }
            }

            Console.WriteLine($"State cleared for bus {assignedBus.BusNumber}.");
        }
        catch (Exception ex)
        {
            // Handle any errors during the process
            Console.WriteLine($"Error clearing bus attendant state: {ex.Message}");
        }
    }


    public void ManagePassengers()
    {
        if (assignedBus.IsOnRoute)
        {
            Console.WriteLine("1. Board Passengers");
            Console.WriteLine("2. Alight Passengers");
            int choice = Helper.GetIntInput("Enter choice: ");

            switch (choice)
            {
                case 1:
                    int boardCount = Helper.GetIntInput("Enter number of passengers boarding: ");
                    assignedBus.BoardPassengers(boardCount);
                    break;
                case 2:
                    int alightCount = Helper.GetIntInput("Enter number of passengers alighting: ");
                    assignedBus.AlightPassengers(alightCount);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Route is not in progress. Start the route first.");
        }

        Helper.Pause();
    }
    public void UpdateBusStatus(List<Route> routes)
    {
        if (assignedBus.IsOnRoute)
        {
            // Find the route for the assigned bus
            Route route = routes.Find(r => r.Name == assignedBus.Route);
            if (route != null)
            {
                Console.WriteLine("Select the new stop point from the route:");

                // Display stops
                List<Stop> stops = assignedBus.IsReverse
                    ? new List<Stop>(route.Stops)
                    : route.Stops;
                if (assignedBus.IsReverse)
                    stops.Reverse();

                DisplayRouteStops(stops);

                int choice = Helper.GetIntInput("Select stop point number: ");
                if (choice >= 1 && choice <= stops.Count)
                {
                    assignedBus.CurrentLocation = stops[choice - 1].Location;
                    assignedBus.LastUpdatedTime = DateTime.Now;
                    Console.WriteLine($"Current location updated to: {assignedBus.CurrentLocation} at {assignedBus.LastUpdatedTime:hh:mm tt}");

                    // Ask for the updated traffic condition
                    Console.WriteLine("\nSelect the current traffic condition:");
                    foreach (var condition in Enum.GetValues(typeof(TrafficCondition)))
                    {
                        Console.WriteLine($"{(int)condition} - {condition}");
                    }

                    // Get the updated traffic condition
                    int trafficChoice = Helper.GetIntInput("Enter traffic condition number: ");
                    if (Enum.IsDefined(typeof(TrafficCondition), trafficChoice))
                    {
                        assignedBus.Traffic = (TrafficCondition)trafficChoice;
                        Console.WriteLine($"Traffic condition updated to: {assignedBus.Traffic}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Traffic condition not updated.");
                    }
                    // Save the state with the updated location and time
                    SaveState();
                }
                else
                {
                    Console.WriteLine("Invalid choice. Location not updated.");
                }
            }
            else
            {
                Console.WriteLine("Route not found. Unable to update location.");
            }
        }
        else
        {
            Console.WriteLine("Cannot update status. Start the route first.");
        }

        Helper.Pause();
    }


    private void DisplayRouteStops(List<Stop> stops)
    {
        // Check if the list is empty
        if (stops == null || stops.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No stops available on this route.[/]");
            return;
        }

        // Create a table for displaying stops
        var table = new Table();
        table.Border(TableBorder.Rounded); // Apply a rounded border
        table.AddColumn("[bold yellow]No.[/]");
        table.AddColumn("[bold yellow]Location[/]");

        // Add rows for each stop
        for (int i = 0; i < stops.Count; i++)
        {
            table.AddRow((i + 1).ToString(), stops[i].Location);
        }

        // Render the table
        AnsiConsole.Write(table);
    }


    public override void ReportEmergency()
    {
        Console.WriteLine("Breakdown reported. Assistance will be dispatched.");
        DisplayEmergencyContacts();
    }

}

