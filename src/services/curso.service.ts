import { FirestoreService } from "./firestore.service";
import { Curso, Coordernador, Disciplina, Aluno } from "@/interfaces/types";

export interface CursoResumo extends Curso {
  coordenadorNome: string;
  quantidadeDisciplinas: number;
  quantidadeAlunos: number;
  quantidadeAlunosAtivos: number;
  engajamento: number; // % de alunos ativos em relação ao total do curso
}

type CoordenadorMinimo = Pick<Coordernador, "id" | "nome">;
type DisciplinaMinima = Pick<Disciplina, "id" | "cursoId">;
type AlunoMinimo = Pick<Aluno, "id" | "cursoId" | "status">;


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

export class CursoService {
  private firestore = new FirestoreService();

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
}
