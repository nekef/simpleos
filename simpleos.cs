using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleOS
{
    class Program
    {
        static readonly Dictionary<string, string> Users = new Dictionary<string, string>
        {
            {"admin", "admin123"},
            {"user", "user123"}
        };

        static readonly Dictionary<string, List<string>> AdminPermissions = new Dictionary<string, List<string>>
        {
            {"admin", new List<string>{"help", "sayhello", "exit", "listusers", "adduser", "removeuser", "changepassword", "storedatalist", "editdata", "changeuser", "changeusertype", "ls", "cd", "mkdir", "rm", "store", "retrieve", "env", "setenv", "getenvi", "notepad", "save", "read", "delete"}},
            {"user", new List<string>{"help", "sayhello", "exit", "changepassword", "store", "retrieve", "storedatalist", "changeuser", "ls", "cd", "mkdir", "rm", "env", "setenv", "getenvi", "notepad", "save", "read", "delete"}}
        };

        static Dictionary<string, List<string>> UserPermissions = new Dictionary<string, List<string>>(AdminPermissions);
        static string CurrentUser;
        static readonly List<string> CommandHistory = new List<string>();
        static readonly Dictionary<string, string> StoredData = new Dictionary<string, string>();
        static readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();

        static readonly Dictionary<string, Dictionary<string, object>> FileSystem = new Dictionary<string, Dictionary<string, object>>();
        static string CurrentDirectory = "/";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to SimpleOS!");
            ShowUsernames();
            InitializeFileSystem();
            Login();
        }

        static void InitializeFileSystem()
        {
            FileSystem["/"] = new Dictionary<string, object>();
        }

        static void ShowUsernames()
        {
            Console.WriteLine("Available users:");
            foreach (var user in Users.Keys)
            {
                Console.WriteLine($"  {user}");
            }
        }

        static void Login()
        {
            while (true)
            {
                Console.Write("Username: ");
                string username = Console.ReadLine();

                Console.Write("Password: ");
                string password = Console.ReadLine();

                if (Users.TryGetValue(username, out var storedPassword) && storedPassword == password)
                {
                    Console.WriteLine($"Login successful. Welcome, {username}!");
                    CurrentUser = username;
                    RunShell();
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid username or password. Please try again.");
                }
            }
        }

        static void RunShell()
        {
            Console.WriteLine($"Available commands for {CurrentUser}:");
            foreach (var command in UserPermissions[CurrentUser])
            {
                Console.WriteLine($"  {command}");
            }

            while (true)
            {
                Console.Write($"{CurrentDirectory}> ");
                string input = Console.ReadLine();
                CommandHistory.Add(input);

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0];
                List<string> arguments = parts.Skip(1).ToList();

                ExecuteCommand(command, arguments);
            }
        }

        static void ExecuteCommand(string command, List<string> arguments)
        {
            if (!UserPermissions[CurrentUser].Contains(command))
            {
                Console.WriteLine($"Access denied: {CurrentUser} cannot execute command '{command}'.");
                return;
            }

            switch (command.ToLower())
            {
                case "help":
                    ShowHelp();
                    break;
                case "sayhello":
                    Console.WriteLine("Hello!");
                    break;
                case "exit":
                    Environment.Exit(0);
                    break;
                case "listusers":
                    ListUsers();
                    break;
                case "adduser":
                    AddUser(arguments);
                    break;
                case "removeuser":
                    RemoveUser(arguments);
                    break;
                case "store":
                    StoreData(arguments);
                    break;
                case "retrieve":
                    RetrieveData(arguments);
                    break;
                case "changepassword":
                    ChangePassword(arguments);
                    break;
                case "storedatalist":
                    ListStoredData();
                    break;
                case "editdata":
                    EditData(arguments);
                    break;
                case "changeuser":
                    ChangeUser();
                    break;
                case "changeusertype":
                    ChangeUserType(arguments);
                    break;
                case "ls":
                    ListFiles(arguments);
                    break;
                case "cd":
                    ChangeDirectory(arguments);
                    break;
                case "mkdir":
                    CreateDirectory(arguments);
                    break;
                case "rm":
                    RemoveFileOrDirectory(arguments);
                    break;
                case "env":
                    ListEnvironmentVariables();
                    break;
                case "setenv":
                    SetEnvironmentVariable(arguments);
                    break;
                case "getenvi":
                    GetEnvironmentVariable(arguments);
                    break;
                case "notepad":
                    OpenNotepad(arguments);
                    break;
                case "save":
                    SaveFile(arguments);
                    break;
                case "read":
                    ReadFile(arguments);
                    break;
                case "delete":
                    DeleteFile(arguments);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                    break;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine($"Available commands for {CurrentUser}:");
            foreach (var command in UserPermissions[CurrentUser])
            {
                Console.WriteLine($"  {command}");
            }
        }

        static void ListUsers()
        {
            Console.WriteLine("Users:");
            foreach (var user in Users.Keys)
            {
                Console.WriteLine($"  {user}");
            }
        }

        static void AddUser(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: adduser <username> <password>");
                return;
            }

            string username = arguments[0];
            string password = arguments[1];

            if (Users.ContainsKey(username))
            {
                Console.WriteLine($"User '{username}' already exists.");
            }
            else
            {
                Users.Add(username, password);
                UserPermissions.Add(username, new List<string>(UserPermissions["user"]));
                Console.WriteLine($"User '{username}' added successfully.");
            }
        }

        static void RemoveUser(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: removeuser <username>");
                return;
            }

            string username = arguments[0];

            if (Users.Remove(username))
            {
                UserPermissions.Remove(username);
                Console.WriteLine($"User '{username}' removed successfully.");
            }
            else
            {
                Console.WriteLine($"User '{username}' does not exist.");
            }
        }

        static void StoreData(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: store <key> <value>");
                return;
            }

            string key = arguments[0];
            string value = arguments[1];

            StoredData[key] = value;
            Console.WriteLine($"Data stored with key '{key}'.");
        }

        static void RetrieveData(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: retrieve <key>");
                return;
            }

            string key = arguments[0];

            if (StoredData.TryGetValue(key, out var value))
            {
                Console.WriteLine($"Data for key '{key}': {value}");
            }
            else
            {
                Console.WriteLine($"No data found for key '{key}'.");
            }
        }

        static void ChangePassword(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: changepassword <oldpassword> <newpassword>");
                return;
            }

            string oldPassword = arguments[0];
            string newPassword = arguments[1];

            if (Users[CurrentUser] == oldPassword)
            {
                Users[CurrentUser] = newPassword;
                Console.WriteLine("Password changed successfully.");
            }
            else
            {
                Console.WriteLine("Old password is incorrect.");
            }
        }

        static void ListStoredData()
        {
            Console.WriteLine("Stored Data:");
            foreach (var data in StoredData)
            {
                Console.WriteLine($"  Key: {data.Key}, Value: {data.Value}");
            }
        }

        static void EditData(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: editdata <key> <newvalue>");
                return;
            }

            string key = arguments[0];
            string newValue = arguments[1];

            if (StoredData.ContainsKey(key))
            {
                StoredData[key] = newValue;
                Console.WriteLine($"Data for key '{key}' updated successfully.");
            }
            else
            {
                Console.WriteLine($"No data found for key '{key}'. Use 'store <key> <value>' to add new data.");
            }
        }

        static void ChangeUser()
        {
            Login();
        }

        static void ChangeUserType(List<string> arguments)
        {
            if (CurrentUser != "admin")
            {
                Console.WriteLine("Access denied: Only admin can change user types.");
                return;
            }

            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: changeusertype <username> <usertype>");
                return;
            }

            string username = arguments[0];
            string usertype = arguments[1];

            if (Users.ContainsKey(username))
            {
                switch (usertype)
                {
                    case "admin":
                        UserPermissions[username] = new List<string>(AdminPermissions["admin"]);
                        Console.WriteLine($"User '{username}' is now an admin.");
                        break;
                    case "user":
                        UserPermissions[username] = new List<string>(AdminPermissions["user"]);
                        Console.WriteLine($"User '{username}' is now a standard user.");
                        break;
                    default:
                        Console.WriteLine("Invalid user type. Use 'admin' or 'user'.");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"User '{username}' does not exist.");
            }
        }

        static void ListFiles(List<string> arguments)
        {
            var dir = arguments.Count > 0 ? arguments[0] : CurrentDirectory;
            if (!FileSystem.ContainsKey(dir))
            {
                Console.WriteLine($"Directory '{dir}' does not exist.");
                return;
            }

            Console.WriteLine($"Contents of {dir}:");
            foreach (var item in FileSystem[dir])
            {
                if (item.Value is Dictionary<string, object>)
                {
                    Console.WriteLine($"<DIR> {item.Key}");
                }
                else
                {
                    Console.WriteLine($"      {item.Key}");
                }
            }
        }

        static void ChangeDirectory(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: cd <directory>");
                return;
            }

            string path = arguments[0];
            if (path == "..")
            {
                if (CurrentDirectory != "/")
                {
                    int lastIndex = CurrentDirectory.LastIndexOf('/');
                    CurrentDirectory = CurrentDirectory.Substring(0, lastIndex);
                    if (string.IsNullOrEmpty(CurrentDirectory))
                    {
                        CurrentDirectory = "/";
                    }
                }
                Console.WriteLine($"Current directory: {CurrentDirectory}");
                return;
            }

            string newPath = CurrentDirectory == "/" ? $"/{path}" : $"{CurrentDirectory}/{path}";

            if (FileSystem.ContainsKey(newPath))
            {
                CurrentDirectory = newPath;
                Console.WriteLine($"Current directory: {CurrentDirectory}");
            }
            else
            {
                Console.WriteLine($"Directory '{path}' does not exist.");
            }
        }

        static void CreateDirectory(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: mkdir <directory>");
                return;
            }

            string path = arguments[0];
            string newDir = CurrentDirectory == "/" ? $"/{path}" : $"{CurrentDirectory}/{path}";

            if (!FileSystem.ContainsKey(newDir))
            {
                FileSystem[newDir] = new Dictionary<string, object>();
                Console.WriteLine($"Directory '{newDir}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Directory '{newDir}' already exists.");
            }
        }

        static void RemoveFileOrDirectory(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: rm <file/directory>");
                return;
            }

            string path = arguments[0];
            string target = CurrentDirectory == "/" ? $"/{path}" : $"{CurrentDirectory}/{path}";

            if (FileSystem.Remove(target))
            {
                Console.WriteLine($"Directory '{target}' removed successfully.");
            }
            else
            {
                if (FileSystem[CurrentDirectory].Remove(path))
                {
                    Console.WriteLine($"File '{path}' removed successfully.");
                }
                else
                {
                    Console.WriteLine($"'{path}' does not exist.");
                }
            }
        }

        static void ListEnvironmentVariables()
        {
            Console.WriteLine("Environment Variables:");
            foreach (var env in EnvironmentVariables)
            {
                Console.WriteLine($"  {env.Key}: {env.Value}");
            }
        }

        static void SetEnvironmentVariable(List<string> arguments)
        {
            if (arguments.Count < 2)
            {
                Console.WriteLine("Usage: setenv <key> <value>");
                return;
            }

            string key = arguments[0];
            string value = arguments[1];

            EnvironmentVariables[key] = value;
            Console.WriteLine($"Environment variable '{key}' set to '{value}'.");
        }

        static void GetEnvironmentVariable(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: getenv <key>");
                return;
            }

            string key = arguments[0];

            if (EnvironmentVariables.TryGetValue(key, out var value))
            {
                Console.WriteLine($"Environment variable '{key}': {value}");
            }
            else
            {
                Console.WriteLine($"No environment variable found with key '{key}'.");
            }
        }

        // New Commands for Notepad functionality
        static void OpenNotepad(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: notepad <filename>");
                return;
            }

            string filename = arguments[0];
            if (!filename.EndsWith(".txt"))
            {
                Console.WriteLine("Error: Filename must have a .txt extension.");
                return;
            }

            Console.WriteLine($"Editing {filename}. Type your text below (end with an empty line):");
            List<string> lines = new List<string>();
            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                lines.Add(line);
            }

            string fileContent = string.Join(Environment.NewLine, lines);
            FileSystem[CurrentDirectory][filename] = fileContent;
            Console.WriteLine($"{filename} edited successfully. Use 'save {filename}' to save.");
        }

        static void SaveFile(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: save <filename>");
                return;
            }

            string filename = arguments[0];
            if (FileSystem[CurrentDirectory].TryGetValue(filename, out var content))
            {
                if (content is string fileContent)
                {
                    System.IO.File.WriteAllText(filename, fileContent);
                    Console.WriteLine($"{filename} saved successfully.");
                }
                else
                {
                    Console.WriteLine($"Error: {filename} is not a valid file.");
                }
            }
            else
            {
                Console.WriteLine($"Error: {filename} does not exist in the current directory.");
            }
        }

        static void ReadFile(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: read <filename>");
                return;
            }

            string filename = arguments[0];
            if (FileSystem[CurrentDirectory].TryGetValue(filename, out var content))
            {
                if (content is string fileContent)
                {
                    Console.WriteLine($"Contents of {filename}:");
                    Console.WriteLine(fileContent);
                }
                else
                {
                    Console.WriteLine($"Error: {filename} is not a valid file.");
                }
            }
            else
            {
                Console.WriteLine($"Error: {filename} does not exist in the current directory.");
            }
        }

        static void DeleteFile(List<string> arguments)
        {
            if (arguments.Count < 1)
            {
                Console.WriteLine("Usage: delete <filename>");
                return;
            }

            string filename = arguments[0];
            if (FileSystem[CurrentDirectory].Remove(filename))
            {
                Console.WriteLine($"{filename} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Error: {filename} does not exist in the current directory.");
            }
        }
    }
}
