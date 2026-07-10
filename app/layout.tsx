import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "PrevUni - Prevenção Evasão Escolar",
  description: "Sistema academico de prevenção de evasão de aluno no ensino superior",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-br">
      <body>{children}</body>
    </html>
  );
}
