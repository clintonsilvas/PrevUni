export interface Instituicao {
  id: string;
  nome: string;
  sigla: string;
  cnpj: string;
  cidade: string;
  estado: string;
}

export interface Coordernador {
  id: string;
  instituicaoId: string;
  nome: string;
  email: string;
  matricula: string; //será usada como senha
}

export interface Professor {
  id: string;
  instituicaoId: string;
  nome: string;
  email: string;
  matricula: string; //será usada como senha
}

export interface Curso {
  id: string;
  nome: string;
  quantidadePeriodos: number;
  instituicaoId: string;
  coordenadorId: string;
}

export interface Disciplina {
  id: string;
  nome: string;
  periodo: number;
  cargaHoraria: number; //em horas
  cursoId: string;
  professorId: string;
}

export interface Aluno {
  id: string;
  nome: string;
  cpf: string;
  email: string;
  dataNascimento: Date;
  cursoId: string;
  periodoAtual: number;
  status: "ativo" | "inativo";
  dataIngresso: Date;
}

export interface Matricula {
  id: string;
  alunoId: string;
  disciplinaId: string;
  semestre: string;
  situacao: "aprovado" | "matriculado" | "pendente";
  cargaHorariaCumprida: number;
  nota: number | null; // caso for null, o aluno ainda não cursou essa disciplina
}

export interface PrevUniData {
  instituicao: Instituicao;
  coordenadores: Coordernador[];
  professores: Professor[];
  cursos: Curso[];
  disciplinas: Disciplina[];
  alunos: Aluno[];
  matriculas: Matricula[];
}

/*
Essas são as regras:
* Uma instituição pode ter um ou mais cursos
* Uma instituição pode ter um ou mais coordrnadores
* Uma instituição pode ter um ou mais professores
* Um curso deve ter um cordernador
* Um curso pode ter uma ou mais disciplinas
* Um curso pode ter um ou mais alunos
* Uma disciplina deve ter um professor
* Uma disciplina pode ter uma mais matrículas
* Uma matrícula deve ter um aluno
*/
