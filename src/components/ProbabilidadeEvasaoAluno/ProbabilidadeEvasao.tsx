"use client";

import { useEffect, useState } from "react";

interface Props {
  alunoId: string;
}

interface Evasao {
  probabilidadeEvasaoPercent: number;
  nivelRisco: string;
}

export function ProbabilidadeEvasao({
  alunoId,
}: Props) {
  const [evasao, setEvasao] = useState<Evasao | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    async function buscarEvasao() {
      try {
        setCarregando(true);

        const resposta = await fetch(
          `/api/alunos/${alunoId}/evasao`
        );

        if (!resposta.ok) {
          setEvasao(null);
          return;
        }

        setEvasao(await resposta.json());
      } catch {
        setEvasao(null);
      } finally {
        setCarregando(false);
      }
    }

    buscarEvasao();
  }, [alunoId]);

  if (carregando) return <span>Carregando...</span>;

  if (!evasao) return <span>Sem previsão disponível</span>;

  return (
    <div>
      <p>
        Probabilidade de evasão:{" "}
        <strong>
          {evasao.probabilidadeEvasaoPercent}%
        </strong>
      </p>

      <p>
        Nível de risco: <strong>{evasao.nivelRisco}</strong>
      </p>
    </div>
  );
}