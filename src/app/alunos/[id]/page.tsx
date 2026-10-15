import { DadosAluno } from "@/components/DadosAluno/DadosAluno"; // ou onde estiver o seu componente

interface PageProps {
  params: {
    id: string;
  };
}

export default async function AlunoPage({ params }: PageProps) {
  const { id } = await params; 

  return (
    <main>
      <DadosAluno alunoId={id} />
    </main>
  );
}