import { FirestoreService } from "./firestore.service";
import { Curso, Coordernador, Disciplina, Aluno, Matricula, DadosEvasao } from "@/interfaces/types";
import { Sessao } from "./sessao.service";

export interface AlunoCompleto {
  aluno: Aluno;
  curso: Curso | null;
  coordenador: Coordernador | null;
  disciplinas: Disciplina[];
  matriculas: Matricula[]; 
}

function montarAlunosCompletos(
  alunos: Aluno[],
  cursos: Curso[],
  coordenadores: Coordernador[],
  disciplinas: Disciplina[],
  matriculas: Matricula[]
): AlunoCompleto[] {

  return alunos.map((aluno) => {

    // Curso do aluno
    const curso =
      cursos.find((curso) => curso.id === aluno.cursoId) ?? null;

    // Coordenador do curso
    const coordenador = curso
      ? coordenadores.find(
          (coordenador) =>
            coordenador.id === curso.coordenadorId
        ) ?? null
      : null;

    // Matrículas do aluno
    const matriculasDoAluno = matriculas.filter(
      (matricula) => matricula.alunoId === aluno.id
    );

    // Disciplinas que o aluno cursa
    const disciplinasDoAluno = matriculasDoAluno
      .map((matricula) =>
        disciplinas.find(
          (disciplina) =>
            disciplina.id === matricula.disciplinaId
        )
      )
      .filter(
        (disciplina): disciplina is Disciplina =>
          disciplina !== undefined
      );

    return {
      aluno,
      curso,
      coordenador,
      disciplinas: disciplinasDoAluno,
      matriculas: matriculasDoAluno,
    };
  });
}

export class AlunoService {
  private firestore = new FirestoreService();
  

  async listarAlunosCompletos(
    instituicaoId: string
  ): Promise<AlunoCompleto[]> {

    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos, matriculas] =
      await Promise.all([
        this.firestore.listar<Curso>(`${base}/cursos`),
        this.firestore.listar<Coordernador>(`${base}/coordenadores`),
        this.firestore.listar<Disciplina>(`${base}/disciplinas`),
        this.firestore.listar<Aluno>(`${base}/alunos`),
        this.firestore.listar<Matricula>(`${base}/matriculas`),
      ]);

    return montarAlunosCompletos(
      alunos,
      cursos,
      coordenadores,
      disciplinas,
      matriculas
    );
  }
  async listarParaAdmin(
    instituicaoId: string
  ): Promise<AlunoCompleto[]> {

    return this.listarAlunosCompletos(instituicaoId);
  }

  /**
   * Alunos dos cursos que o coordenador coordena (Aluno.coordenadorId === coordenadorId).
   * Disciplinas e alunos exibidos são os do próprio curso, já que o
   * coordenador tem visão completa dos alunos que coordena.
   */
  async listarParaCoordenador(
    instituicaoId: string,
    coordenadorId: string
  ): Promise<AlunoCompleto[]> {

    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos, matriculas] =
      await Promise.all([
        this.firestore.listar<Curso>(`${base}/cursos`),
        this.firestore.listar<Coordernador>(`${base}/coordenadores`),
        this.firestore.listar<Disciplina>(`${base}/disciplinas`),
        this.firestore.listar<Aluno>(`${base}/alunos`),
        this.firestore.listar<Matricula>(`${base}/matriculas`),
      ]);

    const cursosDoCoordenador = cursos.filter(
      curso => curso.coordenadorId === coordenadorId
    );

    const cursoIds = new Set(
      cursosDoCoordenador.map(curso => curso.id)
    );

    const alunosDoCoordenador = alunos.filter(
      aluno => cursoIds.has(aluno.cursoId)
    );

    return montarAlunosCompletos(
      alunosDoCoordenador,
      cursosDoCoordenador,
      coordenadores,
      disciplinas,
      matriculas
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
  ): Promise<AlunoCompleto[]> {

    const base = `instituicoes/${instituicaoId}`;

    const [cursos, coordenadores, disciplinas, alunos, matriculas] =
      await Promise.all([
        this.firestore.listar<Curso>(`${base}/cursos`),
        this.firestore.listar<Coordernador>(`${base}/coordenadores`),
        this.firestore.listar<Disciplina>(`${base}/disciplinas`),
        this.firestore.listar<Aluno>(`${base}/alunos`),
        this.firestore.listar<Matricula>(`${base}/matriculas`),
      ]);

    const disciplinasDoProfessor = disciplinas.filter(disciplina => disciplina.professorId === professorId);
    const disciplinaIds = new Set(disciplinasDoProfessor.map(disciplina => disciplina.id));
    const matriculasDoProfessor = matriculas.filter(matricula => disciplinaIds.has(matricula.disciplinaId));
    const alunoIds = new Set(matriculasDoProfessor.map(matricula => matricula.alunoId));
    const alunosDoProfessor = alunos.filter(aluno => alunoIds.has(aluno.id));

    return montarAlunosCompletos(
      alunosDoProfessor,
      cursos,
      coordenadores,
      disciplinas,
      matriculasDoProfessor
    );
  }

  /**
   * Ponto único de entrada: decide o escopo correto com base no tipo
   * de usuário logado. Use este método nas páginas em vez de chamar
   * listarPorInstituicao diretamente.
   */
  async listarPorSessao(
    sessao: Sessao
  ): Promise<AlunoCompleto[]> {

    switch (sessao.tipo) {

      case "admin":
        return this.listarParaAdmin(
          sessao.instituicaoId
        );

      case "coordenador":
        return this.listarParaCoordenador(
          sessao.instituicaoId,
          sessao.id
        );

      case "professor":
        return this.listarParaProfessor(
          sessao.instituicaoId,
          sessao.id
        );

      default:
        return [];
    }
  }

  async buscarDadosAluno(
    instituicaoId: string,
    alunoId: string
  ): Promise<Aluno | null> {   
    return this.firestore.buscar<Aluno>(
      `instituicoes/${instituicaoId}/alunos`,      
      alunoId      
    );
  }

  async buscarAlunoCompletoPorId(
    instituicaoId: string,
    alunoId: string
  ): Promise<AlunoCompleto | null> {
    const base = `instituicoes/${instituicaoId}`;

    const aluno = await this.firestore.buscar<Aluno>(
      `${base}/alunos`,
      alunoId
    );

    if (!aluno) return null;

    const [matriculas, curso] = await Promise.all([
      this.firestore.buscarPorCampo<Matricula>(
        `${base}/matriculas`,
        "alunoId",
        alunoId
      ),
      this.firestore.buscar<Curso>(
        `${base}/cursos`,
        aluno.cursoId
      ),
    ]);

    const disciplinaIds = [...new Set(
      matriculas.map(m => m.disciplinaId)
    )];

    const disciplinas = (
      await Promise.all(
        disciplinaIds.map(id =>
          this.firestore.buscar<Disciplina>(
            `${base}/disciplinas`,
            id
          )
        )
      )
    ).filter((d): d is Disciplina => d !== null);

    const coordenador = curso
      ? await this.firestore.buscar<Coordernador>(
          `${base}/coordenadores`,
          curso.coordenadorId
        )
      : null;

    return montarAlunosCompletos(
      [aluno],
      curso ? [curso] : [],
      coordenador ? [coordenador] : [],
      disciplinas,
      matriculas
    )[0] ?? null;
  }
}