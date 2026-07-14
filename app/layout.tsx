import type { Metadata } from "next";
import "./globals.css";
import { Montserrat } from "next/font/google";

const montserrat = Montserrat({
  subsets: ["latin"],
  variable: "--font-montserrat",
});


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
      <body className={montserrat.variable}>{children}</body>
    </html>
  );
}
