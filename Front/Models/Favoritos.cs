using System.Linq;
using System.Text.Json;

namespace Front.Models
{
    public class AppFavoritos
    {
        public Favoritos Favoritos { get; set; } = new();
    }

    public class Favoritos
    {
        public List<string> Cursos { get; set; } = new();
        public List<string> Alunos { get; set; } = new();
    }

    public  class FavoritoService
    {
        private static readonly string _path = "appfavoritos.json";
        private static AppFavoritos _dados;

        static FavoritoService()
        {
            if (!File.Exists(_path))
            {
                _dados = new AppFavoritos();
                Salvar();
            }
            else
            {
                var json = File.ReadAllText(_path);
                _dados = JsonSerializer.Deserialize<AppFavoritos>(json) ?? new AppFavoritos();
            }

            _dados.Favoritos ??= new Favoritos();
            _dados.Favoritos.Cursos ??= new List<string>();  // mudou para string
            _dados.Favoritos.Alunos ??= new List<string>();
        }

        public static void AdicionarCurso(string curso)
        {
            try
            {
                if (!_dados.Favoritos.Cursos.Contains(curso))
                {
                    _dados.Favoritos.Cursos.Add(curso);
                    Salvar();
                }
            }
            catch (Exception ex)
            {
                // log do erro para ver o que está causando
                Console.WriteLine($"Erro ao adicionar curso: {ex.Message}");
                throw;  // relança pra que o erro 500 apareça
            }
        }

        public static List<string> ListarCursos()
        {
            return _dados.Favoritos.Cursos;
        }



        public static void RemoverCurso(string nomeCurso)
        {
            if (_dados.Favoritos.Cursos.Contains(nomeCurso))
            {
                _dados.Favoritos.Cursos.Remove(nomeCurso);
                Salvar();
            }
        }

        public static bool EhFavorito(string nomeCurso)
        {
            return _dados.Favoritos.Cursos.Contains(nomeCurso);
        }

        private static void Salvar()
        {
            var json = JsonSerializer.Serialize(_dados, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
    }


}
