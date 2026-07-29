import { NextRequest, NextResponse } from "next/server";
import { LoginService } from "@/services/login.service";
import { JwtService } from "@/services/jwt.service";


export async function POST(request: NextRequest) {
  try {
    const {instituicaoId, email, senha} = await request.json();

    const loginService = new LoginService();

    const usuario = await loginService.login(instituicaoId,email,senha);


    const jwtService = new JwtService();

    const token = await jwtService.gerarToken({
      id: usuario.id,
      instituicaoId: usuario.instituicaoId,
      tipo: usuario.tipo
    });
    console.log("TOKEN:", token);

    const response = NextResponse.json({
      success: true
    });

    response.cookies.set(
      "prevuni_token",
      token,
      {
        httpOnly: true,
        secure: process.env.NODE_ENV === "production",
        sameSite: "lax",
        maxAge: 60 * 60 * 8,
        path: "/"
      }
    );
    return response;
  } catch(error:any){

    return NextResponse.json(
      {
        success:false,
        message:error.message
      },
      {
        status:401
      }
    );
  }
}