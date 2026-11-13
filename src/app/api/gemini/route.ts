
import { NextRequest, NextResponse } from "next/server";
import { GeminiService } from "@/services/gemini.service";
import { AlunoService } from "@/services/aluno.service";
import { SessaoService } from "@/services/sessao.service";

const gemini = new GeminiService();
const alunoService = new AlunoService();

export async function POST(request: NextRequest) {
  try {
    const sessaoService = new SessaoService();
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {
      return NextResponse.json(
        { sucesso: false, erro: "Não autenticado." },
        { status: 401 }
      );
    }

    const body = await request.json();
    const alunoId = body.alunoId;
    const prompt = body.prompt;

    if (
      typeof alunoId !== "string" ||
      typeof prompt !== "string" ||
      !alunoId.trim() ||
      !prompt.trim()
    ) {
      return NextResponse.json(
        { sucesso: false, erro: "Aluno e pergunta são obrigatórios." },
        { status: 400 }
      );
    }

    const aluno = await alunoService.buscarAlunoCompletoPorId(
      sessao.instituicaoId,
      alunoId
    );

    if (!aluno) {
      return NextResponse.json(
        { sucesso: false, erro: "Aluno não encontrado." },
        { status: 404 }
      );
    }
    
    const resposta = await gemini.perguntar<{ resposta: string }>({
      prompt: `
      Você é o assistente acadêmico do PREVUNI, um sistema de prevenção
      à evasão no ensino superior.

      Responda à pergunta do usuário usando os dados acadêmicos fornecidos.
      Se os dados não forem suficientes, informe isso claramente.
      Não invente notas, faltas, informações ou probabilidades.
      Considere a privacidade dos dados do aluno.

      Pergunta do usuário:
      ${prompt}

      Retorne exclusivamente um JSON no formato:
      {"resposta": "Sua resposta em português"}
      `,
      contexto: aluno,
    });

    if (!resposta.sucesso || !resposta.dados) {
      return NextResponse.json(
        {
          sucesso: false,
          erro: resposta.erro ?? "Não foi possível obter uma resposta da IA.",
        },
        { status: 502 }
      );
    }

    return NextResponse.json({
      sucesso: true,
      resposta: resposta.dados.resposta,
    });
  } catch (error) {
    console.error("Erro na API Gemini:", error);

    return NextResponse.json(
      {
        sucesso: false,
        erro: "Erro ao processar a pergunta. Tente novamente.",
      },
      { status: 500 }
    );
  }
}
