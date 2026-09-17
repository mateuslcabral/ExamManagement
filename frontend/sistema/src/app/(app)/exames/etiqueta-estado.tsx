import type { EstadoExame } from "@/lib/api";
import { ROTULOS_ESTADO } from "@/lib/formatos";
import { Etiqueta } from "@/components/ui/etiqueta";

const COR: Record<EstadoExame, "amarelo" | "teal" | "verde"> = {
  AguardandoAmostra: "amarelo",
  AmostraAcolhida: "teal",
  LaudoParceiroPronto: "teal",
  LaudoCligenParaRevisao: "teal",
  LaudoRevisado: "teal",
  Disponibilizado: "verde",
};

export function EtiquetaEstado({ estado }: { estado: EstadoExame }) {
  return <Etiqueta cor={COR[estado]}>{ROTULOS_ESTADO[estado]}</Etiqueta>;
}
