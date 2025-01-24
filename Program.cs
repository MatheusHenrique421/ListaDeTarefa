using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
namespace ListaDeTarefa
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddDbContext<TarefaDb>(options => options.UseInMemoryDatabase("ListaDeTarefa"));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// Configuração do middleware do Swagger
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddOpenApiDocument(config =>
			{
				config.DocumentName = "ListaDeTarefaAPI";
				config.Title = "ListaDeTarefaAPI v1";
				config.Version = "v1";
			});

			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.UseOpenApi();
				app.UseSwaggerUi(config =>
				{
					config.DocumentTitle = "ListaDeTarefaAPI";
					config.Path = "/swagger";
					config.DocumentPath = "/swagger/{documentName}/swagger.json";
					config.DocExpansion = "list";
				});
			}

			//Usando a API do MapGroup
			var listaDeTarefas = app.MapGroup("/listaDeTarefas");

			listaDeTarefas.MapGet("/", async (TarefaDb db) =>
			  await db.Tarefas.ToListAsync());

			listaDeTarefas.MapGet("/finalizada", async (TarefaDb db) => await db.Tarefas.Where(t => t.Finalizada).ToListAsync());

			listaDeTarefas.MapGet("/{id}", async (int id, TarefaDb db) => await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

			listaDeTarefas.MapPost("/", async (Tarefa tarefa, TarefaDb db) =>
			{
				db.Tarefas.Add(tarefa);
				await db.SaveChangesAsync();

				return Results.Created($"/listaDeTarefas/{tarefa.Id}", tarefa);
			});

			listaDeTarefas.MapPut("/{id}", async (int id, Tarefa inputTarefa, TarefaDb db) =>
			{
				var tarefa = await db.Tarefas.FindAsync(id);

				if (tarefa is null) return Results.NotFound();

				tarefa.Nome = inputTarefa.Nome;
				tarefa.Finalizada = inputTarefa.Finalizada;

				await db.SaveChangesAsync();

				return Results.NoContent();
			});

			listaDeTarefas.MapDelete("/{id}", async (int id, TarefaDb db) =>
			{
				if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
				{
					db.Tarefas.Remove(tarefa);
					await db.SaveChangesAsync();
					return Results.NoContent();
				}

				return Results.NotFound();
			});



			app.MapGet("/", () => "Hello World!");

			app.Run();
		}
	}
}