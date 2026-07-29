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

export class FirestoreService {
  /**
   * Cria ou atualiza um documento
   */
  async salvar<T>(
    caminho: string,
    id: string,
    dados: T
  ): Promise<void> {
    await setDoc(
      doc(db, caminho, id),
      dados as DocumentData,
      {
        merge: true,
      }
    );
  }

  /**
   * Busca um documento pelo id
   */
  async buscar<T>(
    caminho: string,
    id: string
  ): Promise<T | null> {
    const snapshot = await getDoc(
      doc(db, caminho, id)
    );

    if (!snapshot.exists()) {
      return null;
    }

    return {
      id: snapshot.id,
      ...snapshot.data(),
    } as T;
  }

  /**
   * Lista todos os documentos
   */
  async listar<T>(
    caminho: string
  ): Promise<T[]> {
    const snapshot = await getDocs(
      collection(db, caminho)
    );

    return snapshot.docs.map(
      (doc: QueryDocumentSnapshot<DocumentData>) =>
        ({
          id: doc.id,
          ...doc.data(),
        } as T)
    );
  }

  /**
   * Busca por um campo
   */
  async buscarPorCampo<T>(
    caminho: string,
    campo: string,
    valor: unknown
  ): Promise<T[]> {
    const q = query(
      collection(db, caminho),
      where(campo, "==", valor)
    );

    const snapshot = await getDocs(q);

    return snapshot.docs.map(
      (doc: QueryDocumentSnapshot<DocumentData>) =>
        ({
          id: doc.id,
          ...doc.data(),
        } as T)
    );
  }

  /**
   * Exclui um documento
   */
  async excluir(
    caminho: string,
    id: string
  ): Promise<void> {
    await deleteDoc(
      doc(db, caminho, id)
    );
  }

  /**
   * Salva vários documentos em lote
   */
  async salvarLote<T extends { id: string }>(
    caminho: string,
    documentos: T[]
  ): Promise<void> {

    const batch = writeBatch(db);

    documentos.forEach((item) => {

      const referencia = doc(
        db,
        caminho,
        item.id
      );

      batch.set(
        referencia,
        item as DocumentData,
        {
          merge: true,
        }
      );

    });

    await batch.commit();
  }
}