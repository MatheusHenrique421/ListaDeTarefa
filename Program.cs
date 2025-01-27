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

			//O código Map<HttpVerb> agora chama métodos em vez de lambdas
			RouteGroupBuilder listaDeTarefas = app.MapGroup("/listaDeTarefas");

			listaDeTarefas.MapGet("/", GetAllTarefas);
			listaDeTarefas.MapGet("/finalizada", GetTarefaFinalizadas);
			listaDeTarefas.MapGet("/{id}", GetTarefaId);
			listaDeTarefas.MapPost("/", CreateTarefa);
			listaDeTarefas.MapPut("/{id}", UpdateTarefa);
			listaDeTarefas.MapDelete("/{id}", DeleteTarefa);

			app.Run();
		}

		//Esses métodos retornam objetos que implementam IResult e são definidos por TypedResults e DTO
		static async Task<IResult> GetAllTarefas(TarefaDb db)
		{
			return TypedResults.Ok(await db.Tarefas.Select(t => new TarefaParaFazerDTO(t)).ToListAsync());

		}
		static async Task<IResult> GetTarefaFinalizadas(TarefaDb db)
		{
			return TypedResults.Ok(await db.Tarefas.Where(t => t.Finalizada).Select(t => new TarefaParaFazerDTO(t)).ToListAsync());
		}
		static async Task<IResult> GetTarefaId(int id, TarefaDb db)
		{
			return await db.Tarefas.FindAsync(id)
				is Tarefa tarefa
					? TypedResults.Ok(new TarefaParaFazerDTO(tarefa))
					: TypedResults.NotFound();
		}
		static async Task<IResult> CreateTarefa(TarefaParaFazerDTO tarefaParaFazerDTO, TarefaDb db)
		{
			var tarefaParaFazer = new Tarefa
			{
				Nome = tarefaParaFazerDTO.Nome,
				Finalizada = tarefaParaFazerDTO.Finalizada
			};

			db.Tarefas.Add(tarefaParaFazer);
			await db.SaveChangesAsync();

			tarefaParaFazerDTO = new TarefaParaFazerDTO(tarefaParaFazer);

			return TypedResults.Created($"/listaDeTarefas/{tarefaParaFazer.Id}", tarefaParaFazerDTO);
		}
		static async Task<IResult> UpdateTarefa(int id, TarefaParaFazerDTO tarefaParaFazerDTO, TarefaDb db)
		{
			var tarefa = await db.Tarefas.FindAsync(id);

			if (tarefa is null) return TypedResults.NotFound();

			tarefa.Nome = tarefaParaFazerDTO.Nome;
			tarefa.Finalizada = tarefaParaFazerDTO.Finalizada;

			await db.SaveChangesAsync();

			return TypedResults.NoContent();
		}
		static async Task<IResult> DeleteTarefa(int id, TarefaDb db)
		{
			if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
			{
				db.Tarefas.Remove(tarefa);
				await db.SaveChangesAsync();
				return TypedResults.NoContent();
			}
			return TypedResults.NotFound();
		}
	}
}