namespace Backend.Models
{
    public class AlunoResumo
    {
        public string user_id { get; set; }
        public string nome { get; set; }
        public string ultimo_acesso { get; set; }
        public int total_acessos { get; set; }
        public int dias_ativos { get; set; }
        public Dictionary<string, int> interacoes_por_componente { get; set; }
        public List<string> cursos_acessados { get; set; }
    }
}
