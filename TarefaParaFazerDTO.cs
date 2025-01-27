namespace ListaDeTarefa
{
	public class TarefaParaFazerDTO
	{
		public int Id { get; set; }
		public string? Nome { get; set; }
		public bool Finalizada { get; set; }
		public TarefaParaFazerDTO() { }
		public TarefaParaFazerDTO(Tarefa tarefaParaFazer) =>
			(Id, Nome, Finalizada) =
			(tarefaParaFazer.Id, tarefaParaFazer.Nome, tarefaParaFazer.Finalizada);

	}
}
