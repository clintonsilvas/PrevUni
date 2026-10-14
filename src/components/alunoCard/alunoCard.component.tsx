import "./alunoCard.style.css";
import { AlunoIcon } from "../icons/icons";
import { ProbabilidadeEvasao} from "../ProbabilidadeEvasaoAluno/ProbabilidadeEvasao"

export interface AlunoCardProps {
  aluno: {
    id: string;
    nome: string;
    email: string;
  };
  curso?: {
    id: string;
    nome: string;
  } | null;
  disciplinas?: {
    id: string;
    nome: string;
  }[];
  onClick?: () => void;
}

export default function AlunoCard({
  aluno,
  curso,
  disciplinas = [],
  onClick,
}: AlunoCardProps) {
  const qtdDisciplinas = disciplinas.length;

  return (
    <div className="aluno-card" onClick={onClick}> 
      <div className="aluno-card-left">
        <AlunoIcon size={32} />
        <div className="aluno-card-text">
          <h2>{aluno.nome}</h2>
          <p>{aluno.email}</p>
          <span>{curso?.nome ?? "Curso não informado"}</span>
          <span>
            {qtdDisciplinas} {qtdDisciplinas === 1 ? "disciplina" : "disciplinas"}
          </span>
        </div>
      </div>

      <div className="aluno-card-right">
        <div className="aluno-card-evasao">
          <ProbabilidadeEvasao alunoId={aluno.id} />
        </div>
        <button className="aluno-card-button">Acessar</button>
      </div>
    </div>
  );
}