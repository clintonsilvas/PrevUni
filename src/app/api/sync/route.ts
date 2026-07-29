import { NextResponse } from "next/server";
import { SyncService } from "@/services/sync.service";

export async function POST() {

    const sync = new SyncService();

    await sync.sincronizar("inst-001");

    return NextResponse.json({
        sucesso: true
    });

}