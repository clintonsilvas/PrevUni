import Link from "next/link";
import { LivroIcon, DisciplinaIcon, AlunosIcon } from "@/components/icons/icons";
import { CursoResumo } from "@/services/curso.service";
import "./courseCard.style.css";

interface CourseCardProps {
  curso: CursoResumo;
}

function CourseCard({ curso }: CourseCardProps) {
  return (
    <div className="course-card">
      <div className="course-card-top">
        <LivroIcon size={28} />
      </div>

      <div className="course-card-info">
        <h3>{curso.nome}</h3>
        <p>{curso.coordenadorNome}</p>
      </div>

      <div className="course-card-badges">
        <span className="course-card-badge course-card-badge-green">
          <DisciplinaIcon size={16} />
          {curso.quantidadeDisciplinas}
        </span>

        <span className="course-card-badge course-card-badge-blue">
          <AlunosIcon size={16} />
          {curso.quantidadeAlunos}
        </span>
      </div>

      <Link href={`/cursos/${curso.id}`} className="course-card-button">
        Acessar
      </Link>
    </div>
  );
}

export default CourseCard;