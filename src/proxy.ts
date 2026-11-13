import { NextRequest, NextResponse } from "next/server";
import { JwtService } from "@/services/jwt.service";

const REGRAS_DE_ACESSO: { rota: string; papeisPermitidos: string[] }[] = [
  { rota: "/coordenadores", papeisPermitidos: ["admin"] },
  { rota: "/professores", papeisPermitidos: ["admin", "coordenador"] },
];

export async function proxy(
  request: NextRequest
) {

  const token = request.cookies.get(
    "prevuni_token"
  )?.value;

  if (!token) {
    return NextResponse.redirect(
      new URL("/login", request.url)
    );
  }

  const jwt = new JwtService();

  const sessao = await jwt.validarToken(token);

  if (!sessao) {
    return NextResponse.redirect(
      new URL("/login", request.url)
    );
  }

  const regraViolada = REGRAS_DE_ACESSO.find(
    (regra) =>
      request.nextUrl.pathname.startsWith(regra.rota) &&
      !regra.papeisPermitidos.includes(sessao.tipo as string)
  );

  if (regraViolada) {
    return NextResponse.redirect(
      new URL("/cursos", request.url)
    );
  }

  return NextResponse.next();
}


export const config = {
  matcher: [
    "/dashboard/:path*",
    "/cursos/:path*",
    "/alunos/:path*",
    "/disciplinas/:path*",
    "/coordenadores/:path*",
    "/professores/:path*",
    "/meus-dados/:path*"
  ]
};