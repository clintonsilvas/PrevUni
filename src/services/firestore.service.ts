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
} from "firebase/firestore";

import { db } from "@/firebase/firebase";

export class FirestoreService {
  /**
   * Cria ou atualiza um documento
   */
  async salvar(
    caminho: string,
    id: string,
    dados: any
  ): Promise<void> {
    await setDoc(
      doc(db, caminho, id),
      dados,
      {
        merge: true,
      }
    );
  }

  /**
   * Busca um documento pelo id
   */
  async buscar(
    caminho: string,
    id: string
  ) {
    const snapshot = await getDoc(
      doc(db, caminho, id)
    );

    if (!snapshot.exists()) {
      return null;
    }

    return snapshot.data();
  }

  /**
   * Lista todos os documentos
   */
  async listar(
    caminho: string
  ) {
    const snapshot = await getDocs(
      collection(db, caminho)
    );

    return snapshot.docs.map(doc => ({
      id: doc.id,
      ...doc.data(),
    }));
  }

  /**
   * Busca por um campo
   */
  async buscarPorCampo(
    caminho: string,
    campo: string,
    valor: any
  ) {
    const q = query(
      collection(db, caminho),
      where(campo, "==", valor)
    );

    const snapshot = await getDocs(q);

    return snapshot.docs.map(doc => ({
      id: doc.id,
      ...doc.data(),
    }));
  }

  /**
   * Exclui um documento
   */
  async excluir(
    caminho: string,
    id: string
  ) {
    await deleteDoc(
      doc(db, caminho, id)
    );
  }

  /**
   * Salva vários documentos em lote
   */
  async salvarLote(
    caminho: string,
    documentos: any[]
  ) {

    const batch = writeBatch(db);

    documentos.forEach(item => {

      const referencia = doc(
        db,
        caminho,
        item.id
      );

      batch.set(
        referencia,
        item,
        {
          merge: true,
        }
      );

    });

    await batch.commit();

  }
}