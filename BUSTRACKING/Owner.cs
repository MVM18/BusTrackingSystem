using BusTrackingSystem;

public class Owner : User
{
    private readonly List<Bus> buses;
    private const string OwnerPin = "1234";

    public Owner(List<Bus> buses)
    {
        this.buses = buses;
        LoadBuses();
    }
    public static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }

    public bool Login(string pin) => pin == OwnerPin;
    public static void SaveRoutes(List<Route> routes)
    {
        try
        {
            string filePath = "Data/RoutesData.txt";
            // Use StreamWriter to write routes to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var route in routes)
                {
                    // Write the route name
                    writer.WriteLine(route.Name);

                    // Write each stop's details (Location, Distance, Fare)
                    foreach (var stop in route.Stops)
                    {
                        writer.WriteLine($"{stop.Location},{stop.Distance},{stop.Fare}");
                    }

                    // Mark the end of the route
                    writer.WriteLine("END");
                }
            }

            Console.WriteLine("Routes saved successfully.");
        }
        catch (Exception ex)
        {
            // Handle any errors during the process
            Console.WriteLine($"Error saving routes: {ex.Message}");
        }
    }

    public void AddBus(List<Route> routes)
    {
        Console.WriteLine("Add a new bus:");

        // Get validated bus number
        string busNumber = Helper.GetNonEmptyInput("Enter bus number: ");

        // Check if the bus already exists
        if (buses.Exists(b => b.BusNumber.Equals(busNumber, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Bus with this number already exists.");
            Helper.Pause();
            return; // Exit early if the bus number is already in use
        }

        // Get other validated inputs
        string driverName = Helper.GetNonEmptyInput("Enter driver name: ");
        string driverPhoneNumber = Helper.GetNonEmptyInput("Enter driver phone number: ");
        string attendantName = Helper.GetNonEmptyInput("Enter attendant name: ");
        int capacity = Helper.GetIntInput("Enter bus capacity: ");
        string routeName = Helper.GetNonEmptyInput("Enter route name: ");
        
        // Check if the route already exists
        Route route = routes.Find(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
        if (route == null)
        {
            Console.WriteLine("Route does not exist. Please enter the details to create it.");
            route = new Route(routeName);
            Console.WriteLine("Add stops for the route (enter 'done' to finish):");

            while (true)
            {
                string location = Helper.GetNonEmptyInput("Enter stop location: ");
                if (location.Equals("done", StringComparison.OrdinalIgnoreCase))
                    break;

                double distance = Helper.GetDoubleInput("Enter distance (in km) from the starting point: ", allowNegative: false);

                // Calculate fare based on distance
                double fare;
                if (distance <= 5)
                {
                    fare = 12; // Flat minimum fare for up to 5 km
                }
                else
                {
                    fare = ((distance - 5) * 2.25) + 12; // Additional charge for distance beyond 5 km
                }

                // Add the stop with calculated fare
                route.Stops.Add(new Stop(location, distance, fare));
                Console.WriteLine($"Stop '{location}' added with a fare of ₱{fare:0.00}");
            }

            // Save the new route to the system
            routes.Add(route);
            SaveRoutes(routes);
        }

        // Add the bus to the system
        var newBus = new Bus(busNumber, route.Name, driverName, driverPhoneNumber, attendantName, capacity);
        buses.Add(newBus);
        Console.WriteLine("New bus added to the system.");
        SaveBuses();
        Helper.Pause();
    }


    public void RemoveBus(string busNumber)
    {
        var bus = buses.Find(b => b.BusNumber == busNumber);
        if (bus != null)
        {
            buses.Remove(bus);
            Console.WriteLine("Bus removed from the system.");
            SaveBuses();
        }
        else
        {
            Console.WriteLine("Bus not found.");
        }
        Helper.Pause();
    }

    public void UpdateBusInfo(string busNumber, string driverName, string driverPhoneNumber, string attendantName)
    {
        Bus bus = buses.Find(b => b.BusNumber.Equals(busNumber, StringComparison.OrdinalIgnoreCase));
        if (bus != null)
        {
            bus.DriverName = driverName;
            bus.DriverPhoneNumber = driverPhoneNumber;
            bus.AttendantName = attendantName;
            SaveBuses(); // Persist changes
        }
    }



    public void MonitorAllBuses(List<Bus>buses, Commuter commuter)
    {
        Console.WriteLine("Monitoring all buses:");
        commuter.TrackAllBuses(buses);
        Helper.Pause();
    }

    public void ViewTravelHistory(string busNumber)
    {
        var bus = buses.Find(b => b.BusNumber == busNumber);
        if (bus != null)
        {
            bus.LoadTravelHistory();
            foreach (string record in bus.TravelHistory)
                Console.WriteLine(record);
        }
        else
        {
            Console.WriteLine("Bus not found.");
        }
        Helper.Pause();
    }
    public void ManageRoutes(List<Route> routes)
    {
        string[] menuItems = {
            "Add Stop to a Route",
            "Remove Stop from a Route",
            "Adjust Fares for a Route",
            "View Routes",
            "Back to Owner Menu"
        };

        while (true)
        {
            int selectedIndex = Helper.NavigateMenu(menuItems, "--- Manage Routes ---");
            Console.Clear();
            switch (selectedIndex)
            {

                case 0: // Add Stop to a Route
                    Console.Write("Enter route name: ");
                    string routeName = Console.ReadLine();
                    Route route = routes.Find(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                    if (route != null)
                    {
                        Console.Write("Enter stop location: ");
                        string location = Console.ReadLine();
                        double distance = Helper.GetDoubleInput("Enter distance (in km) from starting point: ",allowNegative:false);
                        double fare = (distance - 5) * 2.25 + 12; // Default fare formula
                        route.AddStop(location, distance, fare > 12 ? fare : 12);
                    }
                    else
                    {
                        Console.WriteLine("Route not found.");
                    }
                    Helper.Pause();
                    continue;

                case 1:
                    Console.Write("Enter route name: ");
                    routeName = Console.ReadLine();
                    route = routes.Find(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                    if (route != null)
                    {
                        Console.Write("Enter stop location to remove: ");
                        string stopToRemove = Console.ReadLine();
                        route.RemoveStop(stopToRemove);
                    }
                    else
                    {
                        Console.WriteLine("Route not found.");
                    }
                    Helper.Pause();
                    continue;

                case 2: // Adjust Fares for a Route
                    Console.Write("Enter route name: ");
                    routeName = Console.ReadLine();
                    route = routes.Find(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
                    if (route != null)
                    {
                        double newBaseFare = Helper.GetDoubleInput("Enter new base fare: ", allowNegative: false);
                        double additionalFarePerKm = Helper.GetDoubleInput("Enter additional fare per km: ", allowNegative: false);
                        route.AdjustFares(newBaseFare, additionalFarePerKm);
                    }
                    else
                    {
                        Console.WriteLine("Route not found.");
                    }
                    Helper.Pause();
                    continue;

                case 3:
                    foreach (var r in routes)
                    {
                        r.DisplayRoute();
                    }
                    Helper.Pause();
                    continue;

                case 4:
                    return;
                case -1: 
                    Console.WriteLine("Exiting menu...");
                    return;
            }

            break;

        }
    }

    public override void ReportEmergency()
    {
        Console.WriteLine("Emergency reported. Notifying authorities...");
        DisplayEmergencyContacts();
        Helper.Pause();
    }

    private void SaveBuses()
    {
        try
        {
         
            Directory.CreateDirectory("Data");

            // Use StreamWriter to write bus data to the file
            using (StreamWriter writer = new StreamWriter("Data/BusesData.txt"))
            {
                foreach (var bus in buses)
                {
                    writer.WriteLine($"{bus.BusNumber}," +
                                     $"{bus.Route}," +
                                     $"{bus.DriverName}," +
                                     $"{bus.DriverPhoneNumber}," +
                                     $"{bus.AttendantName}," +
                                     $"{bus.Capacity}," +
                                     $"{bus.Traffic}");
                }
            }

            Console.WriteLine("Buses saved successfully.");
        }
        catch (Exception ex)
        {
            // Handle any errors during the saving process
            Console.WriteLine($"Error saving buses: {ex.Message}");
        }
    }

    private void LoadBuses()
    {
        string filePath = "Data/BusesData.txt";

        // Return early if the file does not exist
        if (!File.Exists(filePath))
            return;

        try
        {
            // Clear the existing bus list
            buses.Clear();

            // Use StreamReader to read the file line by line
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(',');

                    // Ensure the data has the minimum required fields
                    if (data.Length >= 6)
                    {
                        var bus = new Bus(data[0], data[1], data[2], data[3], data[4], int.Parse(data[5]))
                        {
                            Traffic = data.Length > 6
                                ? Enum.TryParse(data[6], out TrafficCondition traffic) ? traffic : TrafficCondition.Light
                                : TrafficCondition.Light
                        };

                        // Load the travel history for the bus
                        bus.LoadTravelHistory();

                        // Add the bus to the list
                        buses.Add(bus);
                    }
                    else
                    {
                        Console.WriteLine("Invalid bus data encountered. Skipping...");
                    }
                }
            }

            Console.WriteLine("Buses loaded successfully.");
        }
        catch (Exception ex)
        {
            // Handle any errors during the file reading process
            Console.WriteLine($"Error loading buses: {ex.Message}");
        }
    }
    public void DeleteTravelHistory(string busNumber)
    {
        var bus = buses.Find(b => b.BusNumber == busNumber);
        if (bus != null)
        {
            bus.DeleteTravelHistory();
        }
        else
        {
            Console.WriteLine($"Bus with number {busNumber} not found.");
        }
        Helper.Pause();
    }
}
