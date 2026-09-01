import { FirestoreService } from "./firestore.service";
import { Disciplina, Curso, Professor, Matricula } from "@/interfaces/types";
import { Sessao } from "./sessao.service";

export interface DisciplinaResumo extends Disciplina {
    cursoNome: string;
    professorNome: string;
    quantidadeAlunos: number;
}

type CursoMinimo = Pick<Curso, "id" | "nome" | "coordenadorId">;
type ProfessorMinimo = Pick<Professor, "id" | "nome">;
type MatriculaMinima = Pick<Matricula, "id" | "disciplinaId" | "alunoId">;

function montarDisciplinasResumo(
    disciplinas: Disciplina[],
    cursos: CursoMinimo[],
    professores: ProfessorMinimo[],
    matriculas: MatriculaMinima[]
): DisciplinaResumo[] {
    return disciplinas.map((disciplina) => {
        const curso = cursos.find((item) => item.id === disciplina.cursoId);
        const professor = professores.find(
            (item) => item.id === disciplina.professorId
        );

        const alunoIdsUnicos = new Set(
            matriculas
                .filter((matricula) => matricula.disciplinaId === disciplina.id)
                .map((matricula) => matricula.alunoId)
        );

        return {
            ...disciplina,
            cursoNome: curso?.nome ?? "Curso não encontrado",
            professorNome: professor?.nome ?? "Sem professor",
            quantidadeAlunos: alunoIdsUnicos.size,
        };
    });
}

export class DisciplinaService {
    private firestore = new FirestoreService();

    /**
     * Uso administrativo: todas as disciplinas da instituição, sem filtro.
     */
    async listarPorInstituicao(
        instituicaoId: string
    ): Promise<DisciplinaResumo[]> {
        const base = `instituicoes/${instituicaoId}`;

        const [disciplinas, cursos, professores, matriculas] = await Promise.all([
            this.firestore.listar<Disciplina>(`${base}/disciplinas`),
            this.firestore.listar<Curso>(`${base}/cursos`),
            this.firestore.listar<Professor>(`${base}/professores`),
            this.firestore.listar<Matricula>(`${base}/matriculas`),
        ]);

        return montarDisciplinasResumo(disciplinas, cursos, professores, matriculas);
    }

    /**
     * Disciplinas dos cursos que o coordenador coordena
     * (Curso.coordenadorId === coordenadorId).
     */
    async listarParaCoordenador(
        instituicaoId: string,
        coordenadorId: string
    ): Promise<DisciplinaResumo[]> {
        const base = `instituicoes/${instituicaoId}`;

        const [disciplinas, cursos, professores, matriculas] = await Promise.all([
            this.firestore.listar<Disciplina>(`${base}/disciplinas`),
            this.firestore.listar<Curso>(`${base}/cursos`),
            this.firestore.listar<Professor>(`${base}/professores`),
            this.firestore.listar<Matricula>(`${base}/matriculas`),
        ]);

        const cursoIdsDoCoordenador = new Set(
            cursos
                .filter((curso) => curso.coordenadorId === coordenadorId)
                .map((curso) => curso.id)
        );

        const disciplinasDoCoordenador = disciplinas.filter((disciplina) =>
            cursoIdsDoCoordenador.has(disciplina.cursoId)
        );

        return montarDisciplinasResumo(
            disciplinasDoCoordenador,
            cursos,
            professores,
            matriculas
        );
    }

    /**
     * Disciplinas que o próprio professor leciona
     * (Disciplina.professorId === professorId).
     */
    async listarParaProfessor(
        instituicaoId: string,
        professorId: string
    ): Promise<DisciplinaResumo[]> {
        const base = `instituicoes/${instituicaoId}`;

        const [disciplinas, cursos, professores, matriculas] = await Promise.all([
            this.firestore.listar<Disciplina>(`${base}/disciplinas`),
            this.firestore.listar<Curso>(`${base}/cursos`),
            this.firestore.listar<Professor>(`${base}/professores`),
            this.firestore.listar<Matricula>(`${base}/matriculas`),
        ]);

        const disciplinasDoProfessor = disciplinas.filter(
            (disciplina) => disciplina.professorId === professorId
        );

        return montarDisciplinasResumo(
            disciplinasDoProfessor,
            cursos,
            professores,
            matriculas
        );
    }

    /**
     * Ponto único de entrada: decide o escopo correto com base no tipo
     * de usuário logado.
     */
    async listarPorSessao(sessao: Sessao): Promise<DisciplinaResumo[]> {
        if (sessao.tipo === "coordenador") {
            return this.listarParaCoordenador(sessao.instituicaoId, sessao.id);
        }

        if (sessao.tipo === "professor") {
            return this.listarParaProfessor(sessao.instituicaoId, sessao.id);
        }

        return this.listarPorInstituicao(sessao.instituicaoId);
    }
}