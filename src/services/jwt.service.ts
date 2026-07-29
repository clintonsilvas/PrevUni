import { SignJWT, jwtVerify } from "jose";

const secret = new TextEncoder().encode(
  process.env.JWT_SECRET
);
console.log("JWT SECRET:", process.env.JWT_SECRET);
export class JwtService {

  async gerarToken(payload: {
    id: string;
    instituicaoId: string;
    tipo: "coordenador" | "professor";
  }) {

    return await new SignJWT(payload)
      .setProtectedHeader({
        alg: "HS256",
      })
      .setIssuedAt()
      .setExpirationTime("8h")
      .sign(secret);

  }


  async validarToken(token: string) {

    try {

      const { payload } = await jwtVerify(
        token,
        secret
      );

      return payload;

    } catch {

      return null;

    }

  }

}