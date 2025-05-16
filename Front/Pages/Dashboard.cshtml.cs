using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Security.Cryptography.X509Certificates;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public Curso Cursos { get; set; } = new();
        public void OnGet(string nome, int quantidadeAlunos)
        {
            Cursos = new Curso()
            {
                curso = nome,
                alunos = quantidadeAlunos

            };
        }
    }
}
