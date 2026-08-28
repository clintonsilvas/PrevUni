import Link from "next/link";
import { DisciplinaIcon, AlunosIcon } from "@/components/icons/icons";
import { DisciplinaResumo } from "@/services/disciplina.service";
import "./disciplinaCard.style.css";

interface DisciplinaCardProps {
    disciplina: DisciplinaResumo;
}

function DisciplinaCard({ disciplina }: DisciplinaCardProps) {
    return (
        <div className="disciplina-card">
            <div className="disciplina-card-top">
                <DisciplinaIcon size={28} />
            </div>

            <div className="disciplina-card-info">
                <h3>{disciplina.nome}</h3>
                <p>{disciplina.professorNome}</p>
                <span className="disciplina-card-curso">{disciplina.cursoNome}</span>
            </div>

            <div className="disciplina-card-badges">
                <span className="disciplina-card-badge disciplina-card-badge-green">
                    {disciplina.periodo}º período
                </span>

                <span className="disciplina-card-badge disciplina-card-badge-blue">
                    <AlunosIcon size={16} />
                    {disciplina.quantidadeAlunos}
                </span>
            </div>

            <Link
                href={`/disciplinas/${disciplina.id}`}
                className="disciplina-card-button"
            >
                Acessar
            </Link>
        </div>
    );
}

export default DisciplinaCard;