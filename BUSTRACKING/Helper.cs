using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTrackingSystem
{
    public static class Helper
    {
        public static void Pause(string message = "Press Enter to continue...")
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }

        public static void ClearScreen(string header = null)
        {
            Console.Clear();
            if (!string.IsNullOrWhiteSpace(header))
            {
                Console.WriteLine(header);
                Console.WriteLine(new string('-', header.Length));
            }
        }

        public static int GetIntInput(string prompt, string errorMessage = "Invalid input. Please enter a valid number.")
        {
            while (true)
            {
                try
                {
                    Console.Write(prompt);
                    if (int.TryParse(Console.ReadLine(), out int result))
                        return result;

                    Console.WriteLine(errorMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }


        public static double GetDoubleInput(string prompt, bool allowNegative = true)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();

                if (double.TryParse(input, out double result))
                {
                    // Check for negative input if not allowed
                    if (!allowNegative && result < 0)
                    {
                        Console.WriteLine("Value cannot be negative. Please enter a positive number.");
                        continue;
                    }

                    return result; // Valid input
                }

                Console.WriteLine("Invalid input. Please enter a valid numeric value.");
            }
        }

        public static string GetNonEmptyInput(string prompt)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again.");
                }
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }

        public static void DisplayBuses(List<Bus> buses)
        {
            if (buses == null || buses.Count == 0)
            {
                Console.WriteLine("No buses are currently available.");
                return;
            }

            Console.WriteLine("\n--- Available Buses ---");
            foreach (var bus in buses)
            {
                Console.WriteLine($"- {bus.BusNumber}");
            }
            Console.WriteLine("------------------------");
        }
        public static void DisplayRoutes(List<Route> routes)
        {
            if (routes == null || routes.Count == 0)
            {
                Console.WriteLine("No routes are currently available.");
                return;
            }

            Console.WriteLine("\n--- Available Routes ---");
            foreach (var route in routes)
            {
                route.DisplayRoute();
            }
            Console.WriteLine("------------------------");
        }

        // Method to display a menu and return the selected index
        public static int NavigateMenu(string[] menuItems, string header = null)
        {
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();

                // Display the header if provided
                if (!string.IsNullOrWhiteSpace(header))
                {
                    Console.WriteLine(header);
                    Console.WriteLine(new string('-', header.Length));
                }

                // Display menu items with highlighting
                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan; // Highlight selected item
                        Console.WriteLine($"> {menuItems[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {menuItems[i]}");
                    }
                }

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                        break;

                    case ConsoleKey.Enter:
                        return selectedIndex;

                    case ConsoleKey.Escape:
                        return -1;
                }
            }
        }
    }
}
