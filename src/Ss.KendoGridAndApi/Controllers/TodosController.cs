using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;

namespace Ss.GridAndApi.Controllers;

public record Todo(long Id, string Name, bool Done);

[ApiController]
public class TodosController : ControllerBase
{
    private static readonly Dictionary<long, List<Todo>> Db = new()
    {
        {
            1L, new List<Todo>
            {
                new(1, "Play Football", false),
                new(2, "Play Cricket", true),
                new(3, "Play Badminton", false),
            }
        },

        {
            2L, new List<Todo>
            {
                new(1, "Study", false),
                new(2, "Debate", true),
                new(3, "Exercise", false),
            }
        },
    };

    private readonly ILogger<TodosController> _logger;

    public TodosController(ILogger<TodosController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("/api/users/{userId:long}/todos/{todoId:long}")]
    public IActionResult Update(long userId, long todoId,
        [FromBody] Todo vm,
        [DataSourceRequest] DataSourceRequest request)
    {
        _logger.LogInformation("{UserId} updating todo {TodoId}", userId, todoId);

        if (!Db.TryGetValue(userId, out var results))
        {
            return NotFound();
        }

        var todo = results.FirstOrDefault(todo => todo.Id == todoId);
        if (todo == null)
        {
            return NotFound();
        }

        var updatedTodo = todo with
        {
            Name = vm.Name,
            Done = vm.Done,
        };
        Db[userId].Remove(todo);
        Db[userId].Add(updatedTodo);

        return Ok(new[] { updatedTodo }.ToDataSourceResult(request));
    }

    [HttpGet]
    [Route("/api/users/{userId:long}/todos")]
    public IActionResult Get(long userId, [DataSourceRequest] DataSourceRequest request)
    {
        _logger.LogInformation("Get todos for {UserId}", userId);

        if (!Db.TryGetValue(userId, out var results))
        {
            return NotFound();
        }

        return Ok(results.ToDataSourceResult(request));
    }
}