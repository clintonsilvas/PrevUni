"use client";

import { useState, useEffect } from "react";
import ModalInstituicao from "./ModalInstituicao";
import ModalIntegracao from "./ModalIntegracao";
import styles from "./dashboard.module.css";
import {Instituicao} from "@/interfaces/types"
import { useRouter } from "next/navigation";

export default function Instituicoes() {
  const router = useRouter();
  const [instituicoes, setInstituicoes] = useState<Instituicao[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [modalInstituicaoAberto, setModalInstituicaoAberto] = useState(false);
  const [modalIntegracaoAberto, setModalIntegracaoAberto] = useState(false);
  const [instituicaoEditando, setInstituicaoEditando] = useState<Instituicao | null>(null);
  const [instituicaoIntegracao, setInstituicaoIntegracao] = useState<Instituicao | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    async function buscarInstituicoes() {
      try{
        const resposta = await fetch("/api/instituicoes");        

        if (resposta.status === 401) {router.push("/login");  return;    }

        const json = await resposta.json();

        if (!resposta.ok || !json.success) {
          throw new Error(json.message ?? "Não foi possível carregar os instituicoes cadastradas.");
        }      
        setInstituicoes(json.data);
      }catch {
        setErro("Não foi possível carregar os instituicoes cadastradas.");
      } finally {
        setCarregando(false);
      }  
    }
    buscarInstituicoes();
  }, []); 
  
  if (carregando) {
    return <p>Carregando instituições...</p>;
  }
  return (
    <>
      <div className={styles.toolbar}>
        <div className={styles.total}>
          <strong>
            {instituicoes.length}
          </strong>
          <span>
            instituições cadastradas
          </span>
        </div>

        <button
          type="button"
          className={styles.addButton}
          onClick={() => {
            setInstituicaoEditando(null);
            setModalInstituicaoAberto(true);
          }}
        >
          <span>+</span>
          Adicionar instituição
        </button>
      </div>

      <div className={styles.cards}>
        {instituicoes.map((instituicao) => (
          <article
            key={instituicao.id}
            className={styles.card}
          >
            <div className={styles.cardTop}>
              <div className={styles.institutionIcon}>
                {instituicao.sigla.substring(0, 2)}
              </div>
              <div className={styles.cardTitle}>
                <h3>
                  {instituicao.nome}
                </h3>
                <span>
                  {instituicao.sigla}
                </span>
              </div>              
            </div>

            <div className={styles.info}>
              <div>
                <span>
                  Domínio
                </span>
                <strong>
                  {instituicao.dominio}
                </strong>
              </div>

              <div>
                <span>
                  Localização
                </span>

                <strong>
                  {instituicao.cidade} - {instituicao.estado}
                </strong>
              </div>

            </div>

            <div className={styles.cardBottom}>

              <span className={styles.api}>
                API configurada
              </span>
              <button
                type="button"
                className={styles.manageButton}
                onClick={() => {
                  setInstituicaoIntegracao(instituicao);
                  setModalIntegracaoAberto(true);
                }}
              >
                Verificar Integracao
              </button>

              <button
                type="button"
                className={styles.manageButton}
                onClick={() => {
                  setInstituicaoEditando(instituicao);
                  setModalInstituicaoAberto(true);
                }}
              >
                Gerenciar
              </button>
            </div>
          </article>
        ))}
      </div>

      {modalInstituicaoAberto && (
        <ModalInstituicao
          instituicao={instituicaoEditando}
          fechar={() => {
            setModalInstituicaoAberto(false)
            setInstituicaoEditando(null);
          }}
          salvo={async () => {
            const resposta = await fetch("/api/instituicoes");

            if (resposta.status === 401) {
              router.push("/login");
              return;
            }

            const json = await resposta.json();

            if (!resposta.ok || !json.success) {
              throw new Error(json.message ?? "Erro ao atualizar instituições.");
            }

            setInstituicoes(json.data);
            setModalInstituicaoAberto(false);
            setInstituicaoEditando(null);
          }}
        />
      )}
      {modalIntegracaoAberto && (
        <ModalIntegracao
          instituicao={instituicaoIntegracao}
          fechar={() => {
            setModalIntegracaoAberto(false);
            setInstituicaoIntegracao(null);
          }}
        />
      )}

    </>
  );
}