using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;

namespace BusTrackingSystem
{
    public enum TrafficCondition
    {
        Light,
        Moderate,
        Heavy
    }

    public abstract class User
    {
        public abstract void ReportEmergency();

        public void DisplayEmergencyContacts()
        {
            Helper.ClearScreen("--- Emergency Contacts ---");
            Console.WriteLine("         Police: 911");
            Console.WriteLine("      Ambulance: 112");
            Console.WriteLine("Fire Department: 911");
            Console.WriteLine("--------------------------");
            Helper.Pause();
        }
    }


    public class Bus
    {
        public string BusNumber { get; private set; }
        public string Route { get; private set; }
        public string DriverName { get; set; }
        public string DriverPhoneNumber { get; set; }
        public string AttendantName { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public List<string> TravelHistory { get; private set; }
        public TrafficCondition Traffic { get; set; }
        public bool IsReverse { get; set; }
        public bool IsOnRoute { get; set; }
        public int Capacity { get; private set; }
        public int CurrentPassengers { get; set; }

        private string currentLocation;
        public string CurrentLocation
        {
            get => currentLocation;
            set
            {
                currentLocation = value;
                LastUpdatedTime = DateTime.Now;
            }
        }

        public Bus(string busNumber, string route, string driverName, string driverPhoneNumber, string attendantName, int capacity)
        {
            BusNumber = busNumber;
            Route = route;
            DriverName = driverName;
            DriverPhoneNumber = driverPhoneNumber;
            AttendantName = attendantName;
            Traffic = TrafficCondition.Light;
            CurrentLocation = "Bus Station";
            LastUpdatedTime = DateTime.Now;
            TravelHistory = new List<string>();
            IsReverse = false;
            Capacity = capacity;
            CurrentPassengers = 0;
        }

        public void BoardPassengers(int count)
        {
            if (CurrentPassengers + count > Capacity)
            {
                Console.WriteLine($"Cannot board {count} passengers. Only {Capacity - CurrentPassengers} seats available.");
            }
            else
            {
                CurrentPassengers += count;
                Console.WriteLine($"{count} passengers boarded. Current passengers: {CurrentPassengers}/{Capacity}");
            }
        }

        public void AlightPassengers(int count)
        {
            if (count > CurrentPassengers)
            {
                Console.WriteLine($"Cannot alight {count} passengers. Only {CurrentPassengers} passengers on board.");
            }
            else
            {
                CurrentPassengers -= count;
                Console.WriteLine($"{count} passengers alighted. Current passengers: {CurrentPassengers}/{Capacity}");
            }
        }
        public void UpdateRoute(string route)
        {
            Route = route;
        }

        public void SaveTravelHistory()
        {
           
            try
            {
                string filePath = $"Data/TravelHistory_{BusNumber}.txt";

                // Open the file in append mode
                using (StreamWriter writer = new StreamWriter(filePath,append: true))
                {
                    foreach (var entry in TravelHistory)

                    {
                        writer.WriteLine(entry);
                    }
                }

                Console.WriteLine("Travel history saved successfully.");
            }
            catch (Exception ex)
            {
                // Handle any errors that may occur
                Console.WriteLine($"Error saving travel history: {ex.Message}");
            }
        }


        public void LoadTravelHistory()
        {
            string path = $"Data/TravelHistory_{BusNumber}.txt";

            if (File.Exists(path))
            {
                Console.WriteLine($"Travel history for bus {BusNumber}:");
                try
                {
                    TravelHistory = new List<string>();

                    // Open the file using StreamReader and read each line
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            TravelHistory.Add(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that may occur
                    Console.WriteLine($"Error loading travel history: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Travel history file does not exist.");
            }
        }
        public void DeleteTravelHistory()
        {
            string filePath = $"Data/TravelHistory_{BusNumber}.txt";

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Travel history for bus {BusNumber} has been deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting travel history: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"No travel history found for bus {BusNumber}.");
            }

            Helper.Pause();
        }
        public static void LoadAllStates(List<Bus> buses)
        {
            foreach (var bus in buses)
            {
                // Create a temporary attendant to load the state for each bus
                var busAttendant = new BusAttendant(bus);
                busAttendant.LoadState();
            }
        }


    }

    public class Stop
    {
        public string Location { get; set; }
        public double Distance { get; set; }
        public double Fare { get; set; }

        public Stop(string location, double distance, double fare)
        {
            Location = location;
            Distance = distance;
            Fare = fare;
        }
    }

    public class Route
    {
        public string Name { get; set; }
        public List<Stop> Stops { get; set; }

        public Route(string name)
        {
            Name = name;
            Stops = new List<Stop>();
        }

        // Method to Add a New Stop
        public void AddStop(string location, double distance, double fare)
        {
            Stops.Add(new Stop(location, distance, fare));
            Console.WriteLine($"Stop '{location}' added successfully with a fare of P{fare:0.00}.");
        }

        // Method to Remove an Existing Stop
        public void RemoveStop(string location)
        {
            var stop = Stops.Find(s => s.Location.Equals(location, StringComparison.OrdinalIgnoreCase));
            if (stop != null)
            {
                Stops.Remove(stop);
                Console.WriteLine($"Stop '{location}' removed successfully.");
            }
            else
            {
                Console.WriteLine($"Stop '{location}' not found in this route.");
            }
        }

        // Method to Adjust Fares for All Stops
        public void AdjustFares(double newBaseFare, double additionalFarePerKm)
        {
            foreach (var stop in Stops)
            {
                double adjustedFare = (stop.Distance - 5) * additionalFarePerKm + newBaseFare;
                stop.Fare = adjustedFare > newBaseFare ? adjustedFare : newBaseFare;
            }
            Console.WriteLine("Fares updated successfully for all stops.");
        }

        
        public void DisplayRoute()
        {
            // Create a new table
            var table = new Table();

         
            table.AddColumn("Location");
            table.AddColumn("Distance (km)");
            table.AddColumn("Fare");

            // Set the table's formatting options
            table.Border = TableBorder.Rounded;
            table.Alignment = Justify.Left;
            table.Title = new TableTitle($"[green]Route: {Name}[/]");

            // Add rows for each stop
            foreach (var stop in Stops)
            {
                table.AddRow(
                    stop.Location,
                    $"{stop.Distance:0.0}",
                    $"P{stop.Fare:0.00}"
                );
            }

            // Render the table to the console
            AnsiConsole.Write(table);
        }

        public static List<Route> LoadRoutes()
        {
            List<Route> routes = new();
            string path = "Data/RoutesData.txt";
            // Check if the file exists before attempting to read it
            if (File.Exists(path))
            {
                using StreamReader reader = new(path);
                string line; // Variable to store each line read from the file
                Route currentRoute = null; 

                // Read the file line by line
                while ((line = reader.ReadLine()) != null)
                {
                    // If the line is "END", it marks the end of the current route
                    if (line == "END")
                    {
                        if (currentRoute != null) // Ensure a route exists
                            routes.Add(currentRoute); // Add route to the list
                        currentRoute = null; // Reset for the next route
                    }
                    else if (currentRoute == null)
                    {
                        currentRoute = new Route(line);
                    }
                    else
                    {
                        var data = line.Split(','); // Split the line by commas
                        currentRoute.Stops.Add(new Stop(
                            data[0],                // Location 
                            double.Parse(data[1]),  // Distance 
                            double.Parse(data[2])   // Fare 
                        ));
                    }
                }
            }
            return routes;
        }

    }



    class Program
    {
        static void Main()
        {
            List<Bus> buses = new();
            Owner owner = new(buses);
            Commuter commuter = new();
            List<Route> routes = Route.LoadRoutes();
            Bus.LoadAllStates(buses);

            // Menu options
            string[] MenuItems = { "Owner Login", "Bus Attendant", "Commuter", "Exit" };
            while (true)
            {
                int selectedIndex = Helper.NavigateMenu(MenuItems, "--- Main Menu ---");

                switch (selectedIndex)
                {
                    case 0: // Owner Login
                        Console.Clear();
                        Console.Write("Enter PIN: ");
                        string pin = Owner.ReadPassword();
                        if (owner.Login(pin))
                        {
                            OwnerMenu(owner, buses, routes,commuter);
                        }
                        else
                        {
                            Console.WriteLine("Invalid PIN.");
                            Helper.Pause();
                        }
                        break;
                    case 1: // Bus Attendant
                        Console.Clear();
                        Helper.DisplayBuses(buses);
                        Console.Write("Enter bus number: ");
                        string busNumber = Console.ReadLine();
                        var busAttendant = buses.Find(b => b.BusNumber == busNumber) != null
                            ? new BusAttendant(buses.Find(b => b.BusNumber == busNumber))
                            : null;
                        if (busAttendant != null)
                        {
                            BusAttendantMenu(busAttendant, routes);
                        }
                        else
                        {
                            Console.WriteLine("Bus not found.");
                            Helper.Pause();
                        }
                        break;
                    case 2: // Commuter
                        CommuterMenu(commuter, buses, routes);
                        break;
                    case 3: // Exit
                        return; // Exit the program
                    case -1:
                        Console.WriteLine("Exiting menu...");
                        return;


                }
            }
        }

        static void OwnerMenu(Owner owner, List<Bus> buses, List<Route> routes, Commuter commuter)
        {
            string[] MenuItems = {
        "Add Bus",
        "Remove Bus",
        "Update Bus Info",
        "Monitor All Buses",
        "View Travel History",
        "Manage Routes",
        "Delete Travel History",
        "Report Emergency",
        "Logout"
    };

            while (true)
            {
                int selectedIndex = Helper.NavigateMenu(MenuItems, "--- Owner Menu ---");
                Console.Clear();
                switch (selectedIndex)
                {
                    case 0: // Add Bus
                        Helper.DisplayBuses(buses);
                        owner.AddBus(routes);
                        continue;

                    case 1: // Remove Bus
                        Helper.DisplayBuses(buses);
                        Console.Write("Enter bus number to remove: ");
                        string removeBusNumber = Console.ReadLine();
                        owner.RemoveBus(removeBusNumber);
                        continue;

                    case 2: // Update Bus Info
                        Helper.DisplayBuses(buses);

                        Console.Write("Enter bus number to update: ");
                        string updateBusNumber = Console.ReadLine()?.Trim();

                        // Validate the entered bus number
                        Bus busToUpdate = buses.Find(b => b.BusNumber.Equals(updateBusNumber, StringComparison.OrdinalIgnoreCase));
                        if (busToUpdate == null)
                        {
                            Console.WriteLine($"No bus found with the number '{updateBusNumber}'.");
                            Helper.Pause();
                            continue; // Return to menu
                        }

                        // Prompt for new driver information
                        Console.Write("Enter new driver name: ");
                        string driverName = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(driverName))
                        {
                            Console.WriteLine("Driver name cannot be empty.");
                            Helper.Pause();
                            continue;
                        }

                        Console.Write("Enter new driver phone number: ");
                        string driverPhoneNumber = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(driverPhoneNumber))
                        {
                            Console.WriteLine("Driver phone number cannot be empty.");
                            Helper.Pause();
                            continue;
                        }

                        // Prompt for new attendant name
                        Console.Write("Enter new attendant name: ");
                        string attendantName = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(attendantName))
                        {
                            Console.WriteLine("Attendant name cannot be empty.");
                            Helper.Pause();
                            continue;
                        }

                        // Update the bus information
                        owner.UpdateBusInfo(updateBusNumber, driverName, driverPhoneNumber, attendantName);
                        Console.WriteLine("Bus information updated successfully.");
                        Helper.Pause();
                        break;


                    case 3: // Monitor All Buses
                        owner.MonitorAllBuses(buses,commuter);
                        continue;

                    case 4: // View Travel History
                        Helper.DisplayBuses(buses);
                        Console.Write("Enter bus number: ");
                        string historyBusNumber = Console.ReadLine();
                        owner.ViewTravelHistory(historyBusNumber);
                        continue;

                    case 5: // Manage Routes
                        Helper.DisplayRoutes(routes);
                        Helper.Pause();
                        owner.ManageRoutes(routes);
                        continue;
                    case 6:
                        Helper.DisplayBuses(buses);
                        Console.Write("Enter bus number to delete travel history: ");
                        string busNumber = Console.ReadLine();
                        owner.DeleteTravelHistory(busNumber);
                        continue;

                    case 7: // Report Emergency
                        owner.ReportEmergency();
                        continue;

                    case 8: // Logout
                        break; // Exit the Owner Menu
                    case -1:
                        Console.WriteLine("Exiting menu...");
                        return;
                }
                break;
            }
        }


        static void BusAttendantMenu(BusAttendant busAttendant, List<Route> routes)
        {
            string[] menuItems = {
        "Start Route",
        "Update Bus Status",
        "Manage Passengers",
        "End Route",
        "Report Emergency",
        "Logout"
    };

            while (true)
            {

                int selectedIndex = Helper.NavigateMenu(menuItems, "--- Bus Attendant Menu ---");

                Console.Clear();
                switch (selectedIndex)
                {
                    case 0:
                        busAttendant.StartRoute(routes);
                        continue;

                    case 1:
                        busAttendant.UpdateBusStatus(routes);
                        continue;

                    case 2:
                        busAttendant.ManagePassengers();
                        continue;

                    case 3:
                        busAttendant.EndRoute();
                        return;

                    case 4:
                        busAttendant.ReportEmergency();
                        continue;

                    case 5:
                        return;

                    case -1:
                        Console.WriteLine("Exiting menu...");
                        return;
                }

                break;
            }

        }

        static void CommuterMenu(Commuter commuter, List<Bus> buses, List<Route> routes)
        {

            string[] menuItems =
                {
        "Track Specific Bus",
        "Track All Buses",
        "Display All Routes",
        "Calculate Fare",
        "Report Emergency",
        "Logout"
                };
            while (true)
            {


                int selectedIndex = Helper.NavigateMenu(menuItems, "--- Commuter Menu ---");

                Console.Clear();
                switch (selectedIndex)
                {
                    case 0:
                        Helper.DisplayBuses(buses);
                        Console.Write("Enter bus number: ");
                        string busNumber = Console.ReadLine()?.Trim();

                        var foundBus = buses.Find(b => b.BusNumber.Equals(busNumber, StringComparison.OrdinalIgnoreCase));
                        if (foundBus != null)

                            commuter.TrackBus(foundBus);
                        else
                            Console.WriteLine("Bus not found.");

                        Helper.Pause();
                        continue;
                    case 1:
                        Console.WriteLine("Tracking all buses:");
                        commuter.TrackAllBuses(buses);
                        Helper.Pause();
                        continue;
                    case 2:
                        Helper.DisplayRoutes(routes);
                        Helper.Pause();
                        continue;
                    case 3:
                        commuter.CalculateFare(buses, routes);
                        continue;
                    case 4:
                        commuter.ReportEmergency();
                        continue;
                    case 5:
                        return;
                    case -1:
                        Console.WriteLine("Exiting menu...");
                        return;
                }
                break;

            }

        }

    }
}