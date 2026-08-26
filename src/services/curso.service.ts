import { FirestoreService } from "./firestore.service";
import { Curso, Coordernador, Disciplina, Aluno, Matricula } from "@/interfaces/types";
import { Sessao } from "./sessao.service";

export interface CursoResumo extends Curso {
  coordenadorNome: string;
  quantidadeDisciplinas: number;
  quantidadeAlunos: number;
  quantidadeAlunosAtivos: number;
  engajamento: number; // % de alunos ativos em relação ao total do curso
}

type CoordenadorMinimo = Pick<Coordernador, "id" | "nome">;
type DisciplinaMinima = Pick<Disciplina, "id" | "cursoId">;
type DisciplinaDoProfessor = Pick<Disciplina, "id" | "cursoId" | "professorId">;
type AlunoMinimo = Pick<Aluno, "id" | "cursoId" | "status">;
type MatriculaMinima = Pick<Matricula, "id" | "disciplinaId" | "alunoId">;


export function montarCursosResumo(
  cursos: Curso[],
  coordenadores: CoordenadorMinimo[],
  disciplinas: DisciplinaMinima[],
  alunos: AlunoMinimo[]
): CursoResumo[] {
  return cursos.map((curso) => {
    const coordenador = coordenadores.find(
      (item) => item.id === curso.coordenadorId
    );

    const disciplinasDoCurso = disciplinas.filter(
      (item) => item.cursoId === curso.id
    );

    const alunosDoCurso = alunos.filter(
      (item) => item.cursoId === curso.id
    );

    const alunosAtivos = alunosDoCurso.filter(
      (item) => item.status === "ativo"
    );

    const engajamento = alunosDoCurso.length
      ? Math.round((alunosAtivos.length / alunosDoCurso.length) * 100)
      : 0;

    return {
      ...curso,
      coordenadorNome: coordenador?.nome ?? "Sem coordenador",
      quantidadeDisciplinas: disciplinasDoCurso.length,
      quantidadeAlunos: alunosDoCurso.length,
      quantidadeAlunosAtivos: alunosAtivos.length,
      engajamento,
    };
  });
}

/**
 * Monta o resumo de cursos escopado a um professor: considera apenas
 * as disciplinas que ele leciona e apenas os alunos matriculados
 * nessas disciplinas (via Matricula), não todos os alunos do curso.
 */
export function montarCursosResumoProfessor(
  cursos: Curso[],
  coordenadores: CoordenadorMinimo[],
  disciplinasDoProfessor: DisciplinaDoProfessor[],
  matriculas: MatriculaMinima[],
  alunos: AlunoMinimo[]
): CursoResumo[] {
  const cursoIdsDoProfessor = new Set(
    disciplinasDoProfessor.map((item) => item.cursoId)
  );

  const cursosDoProfessor = cursos.filter((curso) =>
    cursoIdsDoProfessor.has(curso.id)
  );

  return cursosDoProfessor.map((curso) => {
    const coordenador = coordenadores.find(
      (item) => item.id === curso.coordenadorId
    );

    const disciplinasDoCurso = disciplinasDoProfessor.filter(
      (item) => item.cursoId === curso.id
    );

    const disciplinaIds = new Set(
      disciplinasDoCurso.map((item) => item.id)
    );

    const matriculasRelevantes = matriculas.filter((item) =>
      disciplinaIds.has(item.disciplinaId)
    );

    const alunoIdsUnicos = new Set(
      matriculasRelevantes.map((item) => item.alunoId)
    );

    const alunosDoCurso = alunos.filter((item) =>
      alunoIdsUnicos.has(item.id)
    );

    const alunosAtivos = alunosDoCurso.filter(
      (item) => item.status === "ativo"
    );

    const engajamento = alunosDoCurso.length
      ? Math.round((alunosAtivos.length / alunosDoCurso.length) * 100)
      : 0;

    return {
      ...curso,
      coordenadorNome: coordenador?.nome ?? "Sem coordenador",
      quantidadeDisciplinas: disciplinasDoCurso.length,
      quantidadeAlunos: alunosDoCurso.length,
      quantidadeAlunosAtivos: alunosAtivos.length,
      engajamento,
    };
  });
}

export class CursoService {
  private firestore = new FirestoreService();

  /**
   * Uso administrativo: todos os cursos da instituição, sem filtro.
   */
  async listarPorInstituicao(instituicaoId: string): Promise<CursoResumo[]> {
    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos] = await Promise.all([
      this.firestore.listar<Curso>(`${base}/cursos`),
      this.firestore.listar<Coordernador>(`${base}/coordenadores`),
      this.firestore.listar<Disciplina>(`${base}/disciplinas`),
      this.firestore.listar<Aluno>(`${base}/alunos`),
    ]);

    return montarCursosResumo(cursos, coordenadores, disciplinas, alunos);
  }

  /**
   * Cursos que o coordenador coordena (Curso.coordenadorId === coordenadorId).
   * Disciplinas e alunos exibidos são os do próprio curso, já que o
   * coordenador tem visão completa dos cursos que coordena.
   */
  async listarParaCoordenador(
    instituicaoId: string,
    coordenadorId: string
  ): Promise<CursoResumo[]> {
    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos] = await Promise.all([
      this.firestore.listar<Curso>(`${base}/cursos`),
      this.firestore.listar<Coordernador>(`${base}/coordenadores`),
      this.firestore.listar<Disciplina>(`${base}/disciplinas`),
      this.firestore.listar<Aluno>(`${base}/alunos`),
    ]);

    const cursosDoCoordenador = cursos.filter(
      (curso) => curso.coordenadorId === coordenadorId
    );

    return montarCursosResumo(
      cursosDoCoordenador,
      coordenadores,
      disciplinas,
      alunos
    );
  }

  /**
   * Cursos que contêm ao menos uma disciplina lecionada pelo professor.
   * Dentro de cada curso, só entram na contagem as disciplinas dele
   * e os alunos matriculados nessas disciplinas (via Matricula).
   */
  async listarParaProfessor(
    instituicaoId: string,
    professorId: string
  ): Promise<CursoResumo[]> {
    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos, matriculas] =
      await Promise.all([
        this.firestore.listar<Curso>(`${base}/cursos`),
        this.firestore.listar<Coordernador>(`${base}/coordenadores`),
        this.firestore.listar<Disciplina>(`${base}/disciplinas`),
        this.firestore.listar<Aluno>(`${base}/alunos`),
        this.firestore.listar<Matricula>(`${base}/matriculas`),
      ]);

    const disciplinasDoProfessor = disciplinas.filter(
      (disciplina) => disciplina.professorId === professorId
    );

    return montarCursosResumoProfessor(
      cursos,
      coordenadores,
      disciplinasDoProfessor,
      matriculas,
      alunos
    );
  }

  /**
   * Ponto único de entrada: decide o escopo correto com base no tipo
   * de usuário logado. Use este método nas páginas em vez de chamar
   * listarPorInstituicao diretamente.
   */
  async listarPorSessao(sessao: Sessao): Promise<CursoResumo[]> {
    if (sessao.tipo === "coordenador") {
      return this.listarParaCoordenador(sessao.instituicaoId, sessao.id);
    }

    if (sessao.tipo === "professor") {
      return this.listarParaProfessor(sessao.instituicaoId, sessao.id);
    }

    // admin (ou qualquer outro tipo futuro): visão completa
    return this.listarPorInstituicao(sessao.instituicaoId);
  }
}