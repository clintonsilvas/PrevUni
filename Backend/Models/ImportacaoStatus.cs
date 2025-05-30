namespace Backend.Models
{
    public class ImportacaoStatus
    {
        public string Id { get; set; }
        public string Status { get; set; } // Ex: "Iniciada", "Em Andamento", "Concluída", "Erro"
        public string Mensagem { get; set; }
        public int ProgressoAtual { get; set; }
        public int TotalUsuarios { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Fim { get; set; }
    }
}
