"use client";

import { useEffect, useMemo, useState } from "react";
import styles from "./HistoricoEvasao.module.css";

interface DadosEntrada {
  total_ch_cumprida: number;
  taxa_aprovacao: number;
  total_reprovacoes_pendencias: number;
  percentual_conclusao_curso: number;
  media_notas: number;
}

interface PrevisaoEvasao {
  id: string;
  alunoId: string;
  probabilidadeEvasaoPercent: number;
  nivelRisco: string;
  dadosEntrada?: DadosEntrada;
  data: string | Date | {
    seconds: number;
    nanoseconds?: number;
    type?: string;
  };
}

interface Props {
  alunoId: string;
}

function obterData(data: PrevisaoEvasao["data"]): Date {
  if (data instanceof Date) return data;

  if (typeof data === "string") return new Date(data);

  if (data && typeof data === "object" && "seconds" in data) {
    return new Date(data.seconds * 1000);
  }

  return new Date(NaN);
}

function formatarData(data: Date) {
  return data.toLocaleDateString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "2-digit",
  });
}

function formatarDataCompleta(data: Date) {
  return data.toLocaleString("pt-BR", {
    dateStyle: "long",
    timeStyle: "short",
  });
}

function formatarValor(valor: number | undefined) {
  return valor == null
    ? "Não informado"
    : valor.toLocaleString("pt-BR");
}

export function HistoricoEvasao({ alunoId }: Props) {
  const [historico, setHistorico] = useState<PrevisaoEvasao[]>([]);
  const [selecionadoId, setSelecionadoId] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(false);

  useEffect(() => {
    let ativo = true;

    async function buscarHistorico() {
      try {
        setCarregando(true);
        setErro(false);

        const resposta = await fetch(
          `/api/alunos/evasao/${encodeURIComponent(alunoId)}`
        );

        if (!resposta.ok) {
          throw new Error("Falha ao buscar histórico");
        }

        const resultado = await resposta.json();

        if (!resultado.success || !Array.isArray(resultado.data)) {
          throw new Error("Resposta inválida");
        }

        const ordenado = resultado.data
          .filter(
            (item: PrevisaoEvasao) =>
              Number.isFinite(item.probabilidadeEvasaoPercent) &&
              Number.isFinite(obterData(item.data).getTime())
          )
          .sort(
            (a: PrevisaoEvasao, b: PrevisaoEvasao) =>
              obterData(a.data).getTime() -
              obterData(b.data).getTime()
          );

        if (ativo) {
          setHistorico(ordenado);
          setSelecionadoId(ordenado.at(-1)?.id ?? null);
        }
      } catch {
        if (ativo) setErro(true);
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    buscarHistorico();

    return () => {
      ativo = false;
    };
  }, [alunoId]);

  const selecionado = historico.find(
    (item) => item.id === selecionadoId
  );

  const grafico = useMemo(() => {
    const largura = 700;
    const altura = 250;
    const margem = { top: 20, right: 20, bottom: 42, left: 48 };

    const areaLargura = largura - margem.left - margem.right;
    const areaAltura = altura - margem.top - margem.bottom;

    const pontos = historico.map((item, index) => {
      const valor = Math.min(
        100,
        Math.max(0, item.probabilidadeEvasaoPercent)
      );

      const x =
        margem.left +
        (historico.length === 1
          ? areaLargura / 2
          : (index / (historico.length - 1)) * areaLargura);

      const y = margem.top + (1 - valor / 100) * areaAltura;

      return { item, x, y, valor };
    });

    return {
      largura,
      altura,
      margem,
      areaAltura,
      pontos,
      linha: pontos
        .map((p, i) => `${i === 0 ? "M" : "L"} ${p.x} ${p.y}`)
        .join(" "),
    };
  }, [historico]);

  if (carregando) {
    return (
      <section className={styles.container}>
        <div className={styles.cabecalho}>
          <div className={styles.skeletonTitulo} />
          <div className={styles.skeletonTexto} />
        </div>
        <div className={styles.skeletonGrafico} />
        <p className={styles.mensagem}>Carregando histórico de evasão...</p>
      </section>
    );
  }

  if (erro) {
    return (
      <section className={styles.container}>
        <p className={styles.mensagem}>
          Não foi possível carregar o histórico de evasão.
        </p>
      </section>
    );
  }

  if (historico.length === 0) {
    return (
      <section className={styles.container}>
        <h3 className={styles.titulo}>Histórico de evasão</h3>
        <p className={styles.mensagem}>
          Ainda não existem previsões para este aluno.
        </p>
      </section>
    );
  }

  const corRisco = (risco: string) => {
    switch (risco.toLowerCase()) {
      case "baixo":
        return styles.riscoBaixo;
      case "medio":
      case "médio":
        return styles.riscoMedio;
      case "alto":
        return styles.riscoAlto;
      default:
        return styles.riscoDesconhecido;
    }
  };

  const dados = selecionado?.dadosEntrada;

  return (
    <section className={styles.container}>
      <header className={styles.cabecalho}>
        <div>
          <h3 className={styles.titulo}>Histórico de evasão</h3>
          <p className={styles.subtitulo}>
            Evolução da probabilidade de evasão ao longo do tempo
          </p>
        </div>

        <span className={styles.total}>
          {historico.length} {historico.length === 1 ? "previsão" : "previsões"}
        </span>
      </header>

      <div className={styles.graficoWrapper}>
        <div className={styles.legenda}>
          <span className={styles.legendaPonto} />
          Probabilidade de evasão
        </div>

        <div className={styles.graficoScroll}>
          <svg
            className={styles.grafico}
            viewBox={`0 0 ${grafico.largura} ${grafico.altura}`}
            role="img"
            aria-label="Gráfico histórico de probabilidade de evasão. Selecione um ponto para ver os detalhes."
          >
            {[0, 25, 50, 75, 100].map((valor) => {
              const y =
                grafico.margem.top +
                (1 - valor / 100) * grafico.areaAltura;

              return (
                <g key={valor}>
                  <line
                    x1={grafico.margem.left}
                    x2={grafico.largura - grafico.margem.right}
                    y1={y}
                    y2={y}
                    className={styles.gradeLinha}
                  />
                  <text
                    x={grafico.margem.left - 10}
                    y={y + 4}
                    textAnchor="end"
                    className={styles.textoEixo}
                  >
                    {valor}%
                  </text>
                </g>
              );
            })}

            {historico.length > 1 && (
              <path
                d={`${grafico.linha} L ${
                  grafico.pontos[grafico.pontos.length - 1].x
                } ${
                  grafico.altura - grafico.margem.bottom
                } L ${
                  grafico.pontos[0].x
                } ${
                  grafico.altura - grafico.margem.bottom
                } Z`}
                className={styles.areaGrafico}
              />
            )}

            {historico.length > 1 && (
              <path
                d={grafico.linha}
                className={styles.linhaGrafico}
              />
            )}

            {grafico.pontos.map((ponto, index) => {
              const ativo = ponto.item.id === selecionadoId;
              const mostrarData =
                historico.length <= 7 ||
                index === 0 ||
                index === historico.length - 1 ||
                ativo;

              return (
                <g key={ponto.item.id}>
                  <circle
                    cx={ponto.x}
                    cy={ponto.y}
                    r={ativo ? 8 : 5}
                    className={
                      ativo
                        ? styles.pontoSelecionado
                        : styles.ponto
                    }
                    role="button"
                    tabIndex={0}
                    aria-label={`Previsão de ${formatarData(
                      obterData(ponto.item.data)
                    )}: ${ponto.valor.toLocaleString("pt-BR")}%`}
                    onClick={() => setSelecionadoId(ponto.item.id)}
                    onKeyDown={(event) => {
                      if (event.key === "Enter" || event.key === " ") {
                        event.preventDefault();
                        setSelecionadoId(ponto.item.id);
                      }
                    }}
                  />

                  {ativo && (
                    <text
                      x={ponto.x}
                      y={ponto.y - 15}
                      textAnchor="middle"
                      className={styles.valorPonto}
                    >
                      {ponto.valor.toLocaleString("pt-BR")}%
                    </text>
                  )}

                  {mostrarData && (
                    <text
                      x={ponto.x}
                      y={grafico.altura - 15}
                      textAnchor="middle"
                      className={styles.textoEixo}
                    >
                      {formatarData(obterData(ponto.item.data))}
                    </text>
                  )}
                </g>
              );
            })}
          </svg>
        </div>

        <p className={styles.instrucao}>
          Selecione um ponto do gráfico para consultar a previsão.
        </p>
      </div>

      {selecionado && (
        <div className={styles.detalhes} aria-live="polite">
          <div className={styles.detalhesCabecalho}>
            <div>
              <span className={styles.rotulo}>Previsão selecionada</span>
              <h4>{formatarDataCompleta(obterData(selecionado.data))}</h4>
            </div>

            <span className={`${styles.risco} ${corRisco(selecionado.nivelRisco)}`}>
              Risco {selecionado.nivelRisco}
            </span>
          </div>

          <div className={styles.resumo}>
            <div>
              <span>Probabilidade de evasão</span>
              <strong className={styles.probabilidade}>
                {selecionado.probabilidadeEvasaoPercent.toLocaleString(
                  "pt-BR",
                  { maximumFractionDigits: 2 }
                )}%
              </strong>
            </div>
            <div>
              <span>Nível de risco</span>
              <strong>{selecionado.nivelRisco}</strong>
            </div>
          </div>

          <h4 className={styles.tituloDados}>Indicadores utilizados na previsão</h4>

          {dados ? (
            <div className={styles.gradeDados}>
              <div className={styles.campo}>
                <span>Carga horária cumprida</span>
                <strong>{formatarValor(dados.total_ch_cumprida)}</strong>
              </div>
              <div className={styles.campo}>
                <span>Taxa de aprovação</span>
                <strong>{formatarValor(dados.taxa_aprovacao)}</strong>
              </div>
              <div className={styles.campo}>
                <span>Reprovações e pendências</span>
                <strong>
                  {formatarValor(dados.total_reprovacoes_pendencias)}
                </strong>
              </div>
              <div className={styles.campo}>
                <span>Conclusão do curso</span>
                <strong>
                  {formatarValor(dados.percentual_conclusao_curso)}
                </strong>
              </div>
              <div className={styles.campo}>
                <span>Média das notas</span>
                <strong>{formatarValor(dados.media_notas)}</strong>
              </div>
            </div>
          ) : (
            <p className={styles.mensagem}>
              Os indicadores não estão disponíveis para esta previsão.
            </p>
          )}
        </div>
      )}
    </section>
  );
}