void DisplayInfo(object obj)
{
    switch (obj)
    {
        case Task t:
            Console.WriteLine($"Task: {t.Title} (Completed: {t.IsComplete})");
            break;

        case Project p:
            Console.WriteLine($"Project: {p.Name} — {p.Tasks.Count} task(s)");
            break;

        default:
            Console.WriteLine("Unknown type");
            break;
    }
}

void AddTask(Project project)
{
    Console.Write("Enter task title: ");
    var title = Console.ReadLine();

    var newTask = new Task(title ?? "Untitled Task", false, DateTime.Now.AddDays(7));
    project.Tasks.Add(newTask);

    Console.WriteLine($"✅ Task '{newTask.Title}' added!");
}

void MarkTaskComplete(Project project)
{
    if (project.Tasks.Count == 0)
    {
        Console.WriteLine("No tasks to complete.");
        return;
    }

    DisplayTasks(project);
    Console.Write("Enter task number to mark complete: ");
    if (int.TryParse(Console.ReadLine(), out int index) &&
        index > 0 && index <= project.Tasks.Count)
    {
        var task = project.Tasks[index - 1];
        var updatedTask = task with { IsComplete = true };
        project.Tasks[index - 1] = updatedTask;

        Console.WriteLine($"✅ Task '{task.Title}' marked as completed!");
    }
    else
    {
        Console.WriteLine("Invalid input.");
    }
}

void DisplayTasks(Project project)
{
    Console.WriteLine($"\nTasks for project: {project.Name}");
    if (project.Tasks.Count == 0)
    {
        Console.WriteLine("No tasks yet.");
        return;
    }

    for (int i = 0; i < project.Tasks.Count; i++)
    {
        var task = project.Tasks[i];
        var status = task.IsComplete ? "[✓]" : "[ ]";
        Console.WriteLine($"{i + 1}. {status} {task.Title} (Due: {task.DueDate:d})");
    }
}

//Initialize a project with one task
var project = new Project("Test Project", new List<Task>
{
    new Task("Initial Task", false, DateTime.Now.AddDays(7))
});
//With "with"
var project2 = project with
{
    Tasks = new List<Task>(project.Tasks)
    {
        new Task("New Task", false, DateTime.Now.AddDays(14))
    }
};
var task = new Task("Prepare slides", false, DateTime.Now.AddDays(2));
DisplayInfo(project);
DisplayInfo(task);


Console.WriteLine("-----------------------");
var overdueTasks = project.Tasks
    .Where(t => !t.IsComplete && t.DueDate < DateTime.Now)
    .ToList();

Console.WriteLine($"Overdue tasks in project '{project.Name}':");

if (overdueTasks.Count == 0)
{ 
    Console.WriteLine("None");
}
else
{
    foreach (var t in overdueTasks)
    {
        Console.WriteLine($" - {t.Title} (Due: {t.DueDate:d})");
    }
}

var manager = new Manager
{
    Name = "Alice Johnson",
    Team = "Development",
    Email = "mail@mail.ro"
};

while (true)
{
    Console.WriteLine("\n--- Task Manager ---");
    Console.WriteLine("1. Add new task");
    Console.WriteLine("2. Mark task as complete");
    Console.WriteLine("3. View all tasks");
    Console.WriteLine("4. Exit");
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            AddTask(project);
            break;
        case "2":
            MarkTaskComplete(project);
            break;
        case "3":
            DisplayTasks(project);
            break;
        case "4":
            return;
        default:
            Console.WriteLine("Invalid choice. Try again.");
            break;
    }
}



public record Task(string Title, bool IsComplete, DateTime DueDate);
public record Project(string Name, List<Task> Tasks);


public class Manager
{
    public string Name { get; init; }
    public string Team { get; init; }
    public string Email { get; init; }
}