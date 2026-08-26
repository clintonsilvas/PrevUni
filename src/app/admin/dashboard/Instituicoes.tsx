"use client";

import { useState, useEffect } from "react";
import ModalInstituicao from "./ModalInstituicao";
import styles from "./dashboard.module.css";
import {Instituicao} from "@/interfaces/types"
import {InstituicaoService} from "@/services/instituicao.service"

export default function Instituicoes() {
  const [instituicoes, setInstituicoes] = useState<Instituicao[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [modalAberto, setModalAberto] =  useState(false);  
  
  useEffect(() => {
    async function carregar() {
      try {
        const service = new InstituicaoService();
        const dados = await service.listar();
        setInstituicoes(dados);
      } catch (error) {
        console.error("Erro ao buscar instituições:", error);
      } finally {
        setCarregando(false);
      }
    }
    carregar();
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
          onClick={() => setModalAberto(true)}
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
              >
                Gerenciar
              </button>
            </div>
          </article>
        ))}
      </div>

      {modalAberto && (
        <ModalInstituicao
          fechar={() => setModalAberto(false)}
        />
      )}

    </>
  );
}