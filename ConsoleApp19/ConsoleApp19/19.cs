using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Entry
{
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string XtraInfo { get; set; }

    public Entry(string name, string username, string password, string xtraInfo)
    {
        Name = name;
        Username = username;
        Password = password;
        XtraInfo = xtraInfo;
    }

    public override string ToString()
    {
        return $"Name: {Name}\nUsername: {Username}\nPassword: {Password}\nAdditional Info: {XtraInfo}";
    }
}

public abstract class Request
{
    public abstract void Execute(List<Entry> entries);
}

public class AddEntryRequest : Request
{
    public override void Execute(List<Entry> entries)
    {
        Console.Write("Enter Name: ");
        string name = Console.ReadLine();

        Console.Write("Enter Username: ");
        string username = Console.ReadLine();

        Console.Write("Enter Password: ");
        string password = Console.ReadLine();

        Console.Write("Enter Additional Info: ");
        string xtraInfo = Console.ReadLine();

        entries.Add(new Entry(name, username, password, xtraInfo));
        Console.WriteLine("Entry added successfully!");

        SaveToFile(entries);
    }

    private void SaveToFile(List<Entry> entries)
    {
        using (StreamWriter writer = new StreamWriter("passworddatabase.txt", false))
        {
            foreach (var entry in entries)
            {
                writer.WriteLine($"~Entry\nname={entry.Name}\nusername={entry.Username}\npassword={entry.Password}\nXinfo={entry.XtraInfo}\n");
            }
        }
    }
}

public class SearchEntryRequest : Request
{
    public void Search(List<Entry> entries, string query, bool searchName, bool searchUsername, bool searchPassword, bool searchXtraInfo)
    {
        var results = entries.Where(e =>
            (searchName && (e.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
            (searchUsername && (e.Username?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
            (searchPassword && (e.Password?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
            (searchXtraInfo && (e.XtraInfo?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)).ToList();
        DisplayResults(results);
    }

    private void DisplayResults(List<Entry> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("No entries found.");
        }
        else
        {
            foreach (var entry in results)
            {
                Console.WriteLine(entry);
            }
        }
    }

    public override void Execute(List<Entry> entries)
    {
        Console.Write("Enter search query: ");
        string query = Console.ReadLine();

        Console.WriteLine("Select parts to search (Y/N):");
        Console.Write("Search Name? ");
        bool searchName = Console.ReadLine().Trim().ToLower() == "y";
        Console.Write("Search Username? ");
        bool searchUsername = Console.ReadLine().Trim().ToLower() == "y";
        Console.Write("Search Password? ");
        bool searchPassword = Console.ReadLine().Trim().ToLower() == "y";
        Console.Write("Search Additional Info? ");
        bool searchXtraInfo = Console.ReadLine().Trim().ToLower() == "y";

        Search(entries, query, searchName, searchUsername, searchPassword, searchXtraInfo);
    }
}

public class EditEntryRequest : Request
{
    public override void Execute(List<Entry> entries)
    {
        Console.Write("Enter Entry Name to Edit: ");
        string name = Console.ReadLine();
        var entry = entries.FirstOrDefault(e => (e.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false));

        if (entry == null)
        {
            Console.WriteLine("Entry not found.");
            return;
        }

        Console.Write("Enter New Username (leave blank to keep current): ");
        string username = Console.ReadLine();
        if (!string.IsNullOrEmpty(username)) entry.Username = username;

        Console.Write("Enter New Password (leave blank to keep current): ");
        string password = Console.ReadLine();
        if (!string.IsNullOrEmpty(password)) entry.Password = password;

        Console.Write("Enter New Additional Info (leave blank to keep current): ");
        string xtraInfo = Console.ReadLine();
        if (!string.IsNullOrEmpty(xtraInfo)) entry.XtraInfo = xtraInfo;

        Console.WriteLine("Entry updated successfully!");

        SaveToFile(entries);
    }

    private void SaveToFile(List<Entry> entries)
    {
        using (StreamWriter writer = new StreamWriter("passworddatabase.txt", false))
        {
            foreach (var entry in entries)
            {
                writer.WriteLine($"~Entry\nname={entry.Name}\nusername={entry.Username}\npassword={entry.Password}\nXinfo={entry.XtraInfo}\n");
            }
        }
    }
}

public class DeleteEntryRequest : Request
{
    public override void Execute(List<Entry> entries)
    {
        Console.Write("Enter Entry Name to Delete: ");
        string name = Console.ReadLine();
        var entry = entries.FirstOrDefault(e => (e.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false));

        if (entry == null)
        {
            Console.WriteLine("Entry not found.");
        }
        else
        {
            entries.Remove(entry);
            Console.WriteLine("Entry deleted successfully!");

            SaveToFile(entries);
        }
    }

    private void SaveToFile(List<Entry> entries)
    {
        using (StreamWriter writer = new StreamWriter("passworddatabase.txt", false))
        {
            foreach (var entry in entries)
            {
                writer.WriteLine($"~Entry\nname={entry.Name}\nusername={entry.Username}\npassword={entry.Password}\nXinfo={entry.XtraInfo}\n");
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        Console.Write("Enter application password: ");
        string appPassword = "12345"; // Set your application password here
        string inputPassword = Console.ReadLine();

        if (inputPassword != appPassword)
        {
            Console.WriteLine("Incorrect password. Access denied.");
            return;
        }

        var entries = LoadFromFile();
        Console.WriteLine("Welcome to the Password Manager!");

        while (true)
        {
            Console.WriteLine("\nSelect an option:\n1. Add Entry\n2. Search Entry\n3. Edit Entry\n4. Delete Entry\n5. Exit");
            int choice = int.Parse(Console.ReadLine());

            Request request;
            switch (choice)
            {
                case 1:
                    request = new AddEntryRequest();
                    break;
                case 2:
                    request = new SearchEntryRequest();
                    break;
                case 3:
                    request = new EditEntryRequest();
                    break;
                case 4:
                    request = new DeleteEntryRequest();
                    break;
                case 5:
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    continue;
            }

            request.Execute(entries);
        }
    }

    private static List<Entry> LoadFromFile()
    {
        var entries = new List<Entry>();

        if (File.Exists("passworddatabase.txt"))
        {
            var content = File.ReadAllText("passworddatabase.txt");
            var sections = content.Split(new[] { "~Entry" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var section in sections)
            {
                var lines = section.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var entry = new Entry(
                    lines.FirstOrDefault(l => l.StartsWith("name="))?.Substring(5),
                    lines.FirstOrDefault(l => l.StartsWith("username="))?.Substring(9),
                    lines.FirstOrDefault(l => l.StartsWith("password="))?.Substring(9),
                    lines.FirstOrDefault(l => l.StartsWith("Xinfo="))?.Substring(6)
                );
                entries.Add(entry);
            }
        }

        return entries;
    }
}
