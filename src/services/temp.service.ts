
import {
  doc,
  getDoc,
  getDocs,
  collection,
  setDoc,
  deleteDoc,
} from "firebase/firestore";

import { db } from "@/firebase/firebase";

export async function migrarInstituicao() {
  const idAntigo = "f33da182-d506-42b3-bf60-f577b60e1fb7";
  const idNovo = "INS-001";

  const caminhoAntigo = `instituicoes/${idAntigo}`;
  const caminhoNovo = `instituicoes/${idNovo}`;

  // 1. Busca a instituição atual
  const instituicaoRef = doc(db, caminhoAntigo);
  const instituicaoSnap = await getDoc(instituicaoRef);

  if (!instituicaoSnap.exists()) {
    throw new Error("Instituição não encontrada.");
  }

  // 2. Copia os dados da instituição
  await setDoc(doc(db, caminhoNovo), {
    ...instituicaoSnap.data(),
    id: idNovo,
  });

  // 3. Migra as subcoleções
  const subcolecoes = [
    "cursos",
    "coordenadores",
    "professores",
    "disciplinas",
    "alunos",
    "matriculas",
  ];

  for (const nome of subcolecoes) {
    const snapshot = await getDocs(
      collection(db, `${caminhoAntigo}/${nome}`)
    );

    for (const documento of snapshot.docs) {
      await setDoc(
        doc(db, `${caminhoNovo}/${nome}`, documento.id),
        {
          ...documento.data(),
          instituicaoId: idNovo,
        }
      );
    }
  }

  console.log("Migração concluída. Verifique os dados antes de excluir a instituição antiga.");
}
