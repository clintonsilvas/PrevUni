using System.Collections.Generic;

namespace Front.Models
{
    //public class AlunoResumo
    //{
    //    public string user_id { get; set; }
    //    public string name { get; set; }
    //    public string ultimo_acesso { get; set; }
    //    public int total_acessos { get; set; }
    //    public int dias_ativos { get; set; }
    //    public Dictionary<string, int> interacoes_por_componente { get; set; }
    //    public List<string> cursos_acessados { get; set; }
    //}

    public class AcaoQtd
    {
        public string Acao { get; set; }
        public int Quantidade { get; set; }
    }

    public class UsuarioComAcoes
    {
        public string UserId { get; set; }
        public string Nome { get; set; }
        public List<AcaoQtd> Acoes { get; set; }
    }
}