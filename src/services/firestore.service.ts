import {
  collection,
  deleteDoc,
  doc,
  getDoc,
  getDocs,
  query,
  setDoc,
  where,
  writeBatch,
  QueryDocumentSnapshot,
  DocumentData,
} from "firebase/firestore";

import { db } from "@/firebase/firebase";

// O Firestore aceita no máximo 500 operações por batch.
const TAMANHO_BATCH = 400;
// Limite do operador "in" nas versões atuais do SDK.
const TAMANHO_BLOCO_IN = 30;

export class FirestoreService {
  /** Cria ou atualiza um documento */
  async salvar<T>(caminho: string, id: string, dados: T): Promise<void> {
    await setDoc(doc(db, caminho, id), dados as DocumentData, { merge: true });
  }

  /** Busca um documento pelo id */
  async buscar<T>(caminho: string, id: string): Promise<T | null> {
    const snapshot = await getDoc(doc(db, caminho, id));

    if (!snapshot.exists()) return null;

    return { id: snapshot.id, ...snapshot.data() } as T;
  }

  /** Lista todos os documentos */
  async listar<T>(caminho: string): Promise<T[]> {
    const snapshot = await getDocs(collection(db, caminho));

    return snapshot.docs.map(
      (d: QueryDocumentSnapshot<DocumentData>) =>
        ({ id: d.id, ...d.data() } as T)
    );
  }

  /** Busca por um campo */
  async buscarPorCampo<T>(
    caminho: string,
    campo: string,
    valor: unknown
  ): Promise<T[]> {
    const q = query(collection(db, caminho), where(campo, "==", valor));
    const snapshot = await getDocs(q);

    return snapshot.docs.map(
      (d: QueryDocumentSnapshot<DocumentData>) =>
        ({ id: d.id, ...d.data() } as T)
    );
  }

  /** Exclui um documento */
  async excluir(caminho: string, id: string): Promise<void> {
    await deleteDoc(doc(db, caminho, id));
  }

  /**
   * Salva vários documentos em lote.
   * CORREÇÃO: divide em batches de 400 (limite do Firestore é 500).
   */
  async salvarLote<T extends { id: string }>(
    caminho: string,
    documentos: T[]
  ): Promise<void> {
    for (let i = 0; i < documentos.length; i += TAMANHO_BATCH) {
      const batch = writeBatch(db);
      const bloco = documentos.slice(i, i + TAMANHO_BATCH);

      bloco.forEach((item) => {
        batch.set(doc(db, caminho, item.id), item as DocumentData, {
          merge: true,
        });
      });

      await batch.commit();
    }
  }

  /**
   * Busca o documento mais recente (campo "data") de um valor.
   * CORREÇÃO: ordena no código, sem orderBy no Firestore. Assim não exige
   * índice composto e não descarta documentos sem o campo "data".
   */
  async buscarUltimo<T>(
    caminho: string,
    campo: string,
    valor: string
  ): Promise<T | null> {
    const snapshot = await getDocs(
      query(collection(db, caminho), where(campo, "==", valor))
    );

    if (snapshot.empty) return null;

    const toMillis = (d: any): number => {
      if (!d) return 0;
      if (typeof d.toMillis === "function") return d.toMillis();
      return new Date(d).getTime() || 0;
    };

    const ordenados = snapshot.docs
      .map((d) => ({ id: d.id, ...d.data() } as any))
      .sort((a, b) => toMillis(b.data) - toMillis(a.data));

    return ordenados[0] as T;
  }

  /** Busca por uma lista de valores (em blocos por causa do limite do "in") */
  async buscarPorLista<T>(
    caminho: string,
    campo: string,
    valores: string[]
  ): Promise<T[]> {
    if (valores.length === 0) return [];

    const resultados: T[] = [];

    for (let i = 0; i < valores.length; i += TAMANHO_BLOCO_IN) {
      const bloco = valores.slice(i, i + TAMANHO_BLOCO_IN);
      const q = query(collection(db, caminho), where(campo, "in", bloco));
      const snapshot = await getDocs(q);

      resultados.push(
        ...snapshot.docs.map((d) => ({ id: d.id, ...d.data() } as T))
      );
    }

    return resultados;
  }
}