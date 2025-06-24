using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Front.Models
{
    public class AlunoNome
    {
        public string UserId { get; set; }
        public string Nome { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AlunoNome other &&
                   UserId == other.UserId &&
                   Nome == other.Nome;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, Nome);
        }
    }

    public class AppFavoritos
    {
        public Favoritos Favoritos { get; set; } = new();
    }

    public class Favoritos
    {
        public List<string> Cursos { get; set; } = new();
        public List<AlunoNome> Alunos { get; set; } = new();
    }

    public class FavoritoService
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
            _dados.Favoritos.Cursos ??= new List<string>();
            _dados.Favoritos.Alunos ??= new List<AlunoNome>();
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
                Console.WriteLine($"Erro ao adicionar nomeCurso: {ex.Message}");
                throw;
            }
        }

        public static void AdicionarAluno(string nome, string id)
        {
            var aux = new AlunoNome { Nome = nome, UserId = id };

            try
            {
                if (!_dados.Favoritos.Alunos.Contains(aux))
                {
                    _dados.Favoritos.Alunos.Add(aux);
                    Salvar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar aluno: {ex.Message}");
                throw;
            }
        }

        public static List<string> ListarCursos() => _dados.Favoritos.Cursos;

        public static List<AlunoNome> ListarAluno() => _dados.Favoritos.Alunos;

        public static void RemoverCurso(string nomeCurso)
        {
            if (_dados.Favoritos.Cursos.Contains(nomeCurso))
            {
                _dados.Favoritos.Cursos.Remove(nomeCurso);
                Salvar();
            }
        }

        public static void RemoverAluno(string nomeAluno, string id)
        {
            var aux = new AlunoNome { Nome = nomeAluno, UserId = id };
            if (_dados.Favoritos.Alunos.Contains(aux))
            {
                _dados.Favoritos.Alunos.Remove(aux);
                Salvar();
            }
        }

        public static bool EhFavorito(string nome)
        {
            return _dados.Favoritos.Cursos.Contains(nome) ||
                   _dados.Favoritos.Alunos.Any(a => a.UserId == nome);
        }

        private static void Salvar()
        {
            var json = JsonSerializer.Serialize(_dados, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
    }
}
