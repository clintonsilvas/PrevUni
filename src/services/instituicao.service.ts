import { FirestoreService } from "./firestore.service";
import { Instituicao } from "@/interfaces/types"

export class InstituicaoService {
  private firestore = new FirestoreService();
  private colecao = "instituicoes";

  async buscarPorDominio(dominio: string) {
    const resultado = await this.firestore.buscarPorCampo<Instituicao>(
      this.colecao,
      "dominio",
      dominio
    );
    return resultado.length ? resultado[0] : null;
  }

  async buscarPorId(id: string): Promise<Instituicao | null> {
    return await this.firestore.buscar<Instituicao>(this.colecao, id);
  }

  async listar(): Promise<Instituicao[]> {
    return await this.firestore.listar<Instituicao>(this.colecao);
  } 

  async criar(
    dados: Omit<Instituicao, "id">
  ): Promise<Instituicao> {

    const id = crypto.randomUUID();

    const instituicao: Instituicao = {
      id,
      ...dados,
    };

    await this.firestore.salvar(
      this.colecao,
      id,
      instituicao
    );

    return instituicao;
  }

  async atualizar(
    id: string,
    dados: Partial<Omit<Instituicao, "id">>
  ): Promise<Instituicao> {

    const instituicaoExistente =
      await this.buscarPorId(id);

    if (!instituicaoExistente) {
      throw new Error("Instituição não encontrada.");
    }

    const instituicaoAtualizada: Instituicao = {
      ...instituicaoExistente,
      ...dados,
      id,
    };

    await this.firestore.salvar(
      this.colecao,
      id,
      instituicaoAtualizada
    );

    return instituicaoAtualizada;
  }

  async validarApi(
    url: string
  ): Promise<{
    valida: boolean;
    mensagem: string;
  }> {
    try {
      const resposta = await fetch(url);

      if (!resposta.ok) {
        return {
          valida: false,
          mensagem: `API respondeu com status ${resposta.status}.`,
        };
      }

      const dados = await resposta.json();

      // Estrutura principal
      if (!dados || typeof dados !== "object") {
        return {
          valida: false,
          mensagem: "Resposta da API não é um objeto válido.",
        };
      }

      // Instituição
      if (!dados.instituicao || typeof dados.instituicao !== "object") {
        return {
          valida: false,
          mensagem: "Campo 'instituicao' não encontrado ou inválido.",
        };
      }

      const instituicao = dados.instituicao;

      if (
        typeof instituicao.id !== "string" ||
        typeof instituicao.nome !== "string" ||
        typeof instituicao.sigla !== "string" ||
        typeof instituicao.cnpj !== "string" ||
        typeof instituicao.cidade !== "string" ||
        typeof instituicao.estado !== "string" ||        
        typeof instituicao.dominio !== "string"
      ) {
        return {
          valida: false,
          mensagem: "Estrutura de 'instituicao' inválida.",
        };
      }

      // Arrays
      const arrays = [
        "coordenadores",
        "professores",
        "cursos",
        "disciplinas",
        "alunos",
        "matriculas",
      ];

      for (const campo of arrays) {
        if (!Array.isArray(dados[campo])) {
          return {
            valida: false,
            mensagem: `Campo '${campo}' deve ser um array.`,
          };
        }
      }

      // Coordenadores
      for (const coordenador of dados.coordenadores) {
        if (
          !coordenador ||
          typeof coordenador !== "object" ||
          typeof coordenador.id !== "string" ||
          typeof coordenador.instituicaoId !== "string" ||
          typeof coordenador.nome !== "string" ||
          typeof coordenador.email !== "string" ||
          typeof coordenador.matricula !== "string"
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de um coordenador é inválida.",
          };
        }
      }

      // Professores
      for (const professor of dados.professores) {
        if (
          !professor ||
          typeof professor !== "object" ||
          typeof professor.id !== "string" ||
          typeof professor.instituicaoId !== "string" ||
          typeof professor.nome !== "string" ||
          typeof professor.email !== "string" ||
          typeof professor.matricula !== "string"
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de um professor é inválida.",
          };
        }
      }

      // Cursos
      for (const curso of dados.cursos) {
        if (
          !curso ||
          typeof curso !== "object" ||
          typeof curso.id !== "string" ||
          typeof curso.nome !== "string" ||
          typeof curso.quantidadePeriodos !== "number" ||
          typeof curso.instituicaoId !== "string" ||
          typeof curso.coordenadorId !== "string"
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de um curso é inválida.",
          };
        }
      }

      // Disciplinas
      for (const disciplina of dados.disciplinas) {
        if (
          !disciplina ||
          typeof disciplina !== "object" ||
          typeof disciplina.id !== "string" ||
          typeof disciplina.nome !== "string" ||
          typeof disciplina.periodo !== "number" ||
          typeof disciplina.cargaHoraria !== "number" ||
          typeof disciplina.cursoId !== "string" ||
          typeof disciplina.professorId !== "string"
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de uma disciplina é inválida.",
          };
        }
      }

      // Alunos
      for (const aluno of dados.alunos) {
        if (
          !aluno ||
          typeof aluno !== "object" ||
          typeof aluno.id !== "string" ||
          typeof aluno.nome !== "string" ||
          typeof aluno.cpf !== "string" ||
          typeof aluno.email !== "string" ||
          typeof aluno.cursoId !== "string" ||
          typeof aluno.periodoAtual !== "number" ||
          typeof aluno.status !== "string" ||
          typeof aluno.dataIngresso !== "string" &&
          !(aluno.dataIngresso instanceof Date)
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de um aluno é inválida.",
          };
        }

        if (
          aluno.status !== "ativo" &&
          aluno.status !== "inativo"
        ) {
          return {
            valida: false,
            mensagem: "Status de aluno inválido.",
          };
        }
      }

      // Matrículas
      for (const matricula of dados.matriculas) {
        if (
          !matricula ||
          typeof matricula !== "object" ||
          typeof matricula.id !== "string" ||
          typeof matricula.alunoId !== "string" ||
          typeof matricula.disciplinaId !== "string" ||
          typeof matricula.semestre !== "string" ||
          typeof matricula.cargaHorariaCumprida !== "number" ||
          typeof matricula.nota !== "number" &&
          matricula.nota !== null
        ) {
          return {
            valida: false,
            mensagem: "Estrutura de uma matrícula é inválida.",
          };
        }

        if (
          matricula.situacao !== "aprovado" &&
          matricula.situacao !== "matriculado" &&
          matricula.situacao !== "pendente" &&
          matricula.situacao !== "reprovado"
        ) {
          return {
            valida: false,
            mensagem: "Situação de matrícula inválida.",
          };
        }
      }

      return {
        valida: true,
        mensagem: "API compatível com o PrevUni.",
      };

    } catch (error) {
      console.error("Erro ao validar API:", error);

      return {
        valida: false,
        mensagem: "Não foi possível acessar a API.",
      };
    }
  }

  async buscarTodasComApiValida(): Promise<Instituicao[]> {
    const todas = await this.listar();

    const resultados = await Promise.all(
      todas.map(async (instituicao) => ({
        instituicao,
        resultado: await this.validarApi(instituicao.api),
      }))
    );

    return resultados
      .filter(({ resultado }) => resultado.valida)
      .map(({ instituicao }) => instituicao);
  }
}
