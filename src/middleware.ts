import { NextRequest, NextResponse } from "next/server";
import { JwtService } from "@/services/jwt.service";


export async function middleware(
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

  const valido = await jwt.validarToken(token);


  if (!valido) {
    return NextResponse.redirect(
      new URL("/login", request.url)
    );
  }
  return NextResponse.next();
}


export const config = {
  matcher:[
    "/dashboard/:path*",
    "/cursos/:path*"
  ]
};