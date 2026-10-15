"use client";

import { useEffect, useState } from "react";
import { Aluno } from "@/interfaces/types";


interface Props {
  alunoId: string;
}



export function DadosAluno({
  alunoId,
}: Props) {
  const [aluno, setAluno] = useState<Aluno | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    async function buscarDadosAluno() {
      try {
        setCarregando(true);

        const resposta = await fetch(
          `/api/test/${alunoId}`
        );

        if (!resposta.ok) {
          setAluno(null);
          return;
        }

        setAluno(await resposta.json());
      } catch {
        setAluno(null);
      } finally {
        setCarregando(false);
      }
    }

    buscarDadosAluno();
  }, [alunoId]);

  if (carregando) {
    return <span>Carregando aluno</span>;
  }

  if (!aluno) {
    return <span>Aluno null</span>;
  }
  
  return (
    <div className="aluno-container">
      <p>{aluno.nome} </p>
      <p>{aluno.email}</p>
    </div>
  );
}