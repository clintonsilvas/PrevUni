import "./alunoCard.style.css";
import { AlunoIcon } from "../icons/icons";

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
  evasao?: {
    id: string;
    alunoId: string;
    probabilidadeEvasaoPercent: number | null;
    nivelRisco: "baixo" | "moderado" | "alto" | "sem_historico";
  }
  onClick?: () => void;
}

export default function AlunoCard({
  aluno,
  curso,
  disciplinas = [],
  evasao,
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
          <p>
            Probabilidade de Evasão:{" "}
            {evasao
              ? `${evasao.probabilidadeEvasaoPercent}%`
              : "Não calculada"}
          </p>

          <p>
            Risco:{" "}
            {evasao
              ? evasao.nivelRisco
              : "Não calculado"}
          </p>
        </div>
        <button className="aluno-card-button">Acessar</button>
      </div>
    </div>
  );
}