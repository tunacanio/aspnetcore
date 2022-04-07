// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.AspNetCore.Mvc;

var app = WebApplication.Create(args);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

string Plaintext() => "Hello, World!";
app.MapGet("/plaintext", Plaintext);

object Json() => new { message = "Hello, World!" };
app.MapGet("/json", Json);

string SayHello(string name) => $"Hello, {name}!";
app.MapGet("/hello/{name}", SayHello);

app.MapGet("/null-result", IResult () => null);

app.MapGet("/todo/{id}", Results<OkObjectHttpResult, NotFoundObjectHttpResult> (int id) =>
{
    return id switch
    {
        >= 1 and <= 10 => (OkObjectHttpResult)Results.Ok(new { Id = id, Title = "Walk the dog" }),
        _ => (NotFoundObjectHttpResult)Results.NotFound()
    };
});

var extensions = new Dictionary<string, object>() { { "traceId", "traceId123" } };

app.MapGet("/problem", () =>
    Results.Problem(statusCode: 500, extensions: extensions));

app.MapGet("/problem-object", () =>
    Results.Problem(new ProblemDetails() { Status = 500, Extensions = { { "traceId", "traceId123" } } }));

var errors = new Dictionary<string, string[]>();

app.MapGet("/validation-problem", () =>
    Results.ValidationProblem(errors, statusCode: 400, extensions: extensions));

app.MapGet("/validation-problem-object", () =>
    Results.Problem(new HttpValidationProblemDetails(errors) { Status = 400, Extensions = { { "traceId", "traceId123" } } }));

app.MapPost("/todos", (TodoBindable todo) => todo);

app.Run();

public class TodoBindable : IBindableFromHttpContext<TodoBindable>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }

    public static ValueTask<TodoBindable> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        return ValueTask.FromResult(new TodoBindable { Id = 1, Title = "I was bound from IBindableFromHttpContext<TodoBindable>.BindAsync!" });
    }
}
