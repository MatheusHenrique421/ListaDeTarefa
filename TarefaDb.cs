using Microsoft.EntityFrameworkCore;

namespace ListaDeTarefa
{
  class TarefaDb : DbContext
  {
	public TarefaDb(DbContextOptions<TarefaDb> options) : base(options) { }

	public DbSet<Tarefa> Tarefas => Set<Tarefa>();
  }
}