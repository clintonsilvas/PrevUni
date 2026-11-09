import { NextResponse } from "next/server";

import { SessaoService } from "@/services/sessao.service";
import { InstituicaoService } from "@/services/instituicao.service";
import { CursoService } from "@/services/curso.service";
import { DisciplinaService } from "@/services/disciplina.service";



export async function GET() {
  try {
    const sessaoService = new SessaoService();
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {            

      return NextResponse.json(
        { sucesso: false, mensagem: "Não autenticado." },
        { status: 401 }
      );
    }

      const instituicaoService = new InstituicaoService();
      const instituicao = await instituicaoService.buscarPorId(
          sessao.instituicaoId
      );
      
      const cursoService = new CursoService();
      const disciplinaService = new DisciplinaService();

      const [cursos, disciplinas] = await Promise.all([
          cursoService.listarPorSessao(sessao),
          disciplinaService.listarPorSessao(sessao),
      ]);

     

      
      const totalAlunos = cursos.reduce(
          (total, curso) => total + (curso.quantidadeAlunos ?? 0),
          0
      );

      const dados = {
          usuario: {
              id: sessao.id,
              nome: sessao.nome,
              email: sessao.email,
              tipo: sessao.tipo,
          },

          instituicao: instituicao
              ? {
                  nome: instituicao.nome,
                  sigla: instituicao.sigla,
                  cidade: instituicao.cidade,
                  estado: instituicao.estado,
                  dominio: instituicao.dominio,
              }
              : null,

          cursos,
          disciplinas,

          estatisticas: {
              totalCursos: cursos.length,
              totalDisciplinas: disciplinas.length,
              totalAlunos,
          },
      };

    return NextResponse.json({
      sucesso: true,
      dados,
    });

  } catch (erro) {
    if (erro instanceof Error) {
        console.error("Mensagem:", erro.message);
        console.error("Stack:", erro.stack);
    } else {
        console.error(erro);
    }     

    return NextResponse.json(
      {
        sucesso: false,
        mensagem: "Erro ao carregar os dados do usuário.",
      },
      { status: 500 }
    );
  }
}