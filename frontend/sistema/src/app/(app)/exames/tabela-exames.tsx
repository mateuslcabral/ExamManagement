import Link from "next/link";
import type { ExameResumo } from "@/lib/api";
import { ESTADOS, ORIGENS } from "@/lib/exames";
import { formatarData, formatarMoeda } from "@/lib/idade";
import { Etiqueta, type CorEtiqueta } from "@/components/ui/etiqueta";

const CORES_ESTADO: Record<ExameResumo["estado"], CorEtiqueta> = {
  AguardandoAmostra: "amarelo",
  AmostraAcolhida: "teal",
  LaudoParceiroPronto: "teal",
  LaudoCligenParaRevisao: "teal",
  LaudoRevisado: "verde",
  DisponibilizadoAoPaciente: "verde",
};

export function EtiquetaEstado({ estado }: { estado: ExameResumo["estado"] }) {
  return <Etiqueta cor={CORES_ESTADO[estado]}>{ESTADOS[estado]}</Etiqueta>;
}

export function TabelaExames({
  exames,
  mostrarPaciente = true,
  vazio = "Nenhum exame cadastrado.",
}: {
  exames: ExameResumo[];
  mostrarPaciente?: boolean;
  vazio?: string;
}) {
  const colunas = mostrarPaciente ? 8 : 7;
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-borda text-left text-xs uppercase tracking-wider text-texto-suave">
            <th className="px-4 py-3 font-semibold">Exame</th>
            {mostrarPaciente && <th className="px-4 py-3 font-semibold">Paciente</th>}
            <th className="px-4 py-3 font-semibold">Entrada</th>
            <th className="px-4 py-3 font-semibold">Previsão</th>
            <th className="px-4 py-3 font-semibold">Origem</th>
            <th className="px-4 py-3 font-semibold">Médico</th>
            <th className="px-4 py-3 font-semibold">Preço</th>
            <th className="px-4 py-3 font-semibold">Situação</th>
          </tr>
        </thead>
        <tbody>
          {exames.length === 0 && (
            <tr>
              <td colSpan={colunas} className="px-4 py-10 text-center text-texto-suave">
                {vazio}
              </td>
            </tr>
          )}
          {exames.map((e) => (
            <tr key={e.id} className="border-b border-borda/60 last:border-0 hover:bg-fundo">
              <td className="px-4 py-3">
                <Link href={`/exames/${e.id}`} className="font-medium text-primaria hover:underline">
                  {e.exameNome}
                </Link>
                {e.quantidadeAnexos > 0 && (
                  <span className="ml-2 text-xs text-texto-suave" title="Anexos">
                    📎 {e.quantidadeAnexos}
                  </span>
                )}
              </td>
              {mostrarPaciente && (
                <td className="px-4 py-3">
                  <Link href={`/pacientes/${e.pacienteId}`} className="text-texto hover:underline">
                    {e.pacienteNome}
                  </Link>
                </td>
              )}
              <td className="px-4 py-3 text-texto">{formatarData(e.dataEntrada)}</td>
              <td className="px-4 py-3 text-texto">
                {e.dataLiberacaoPrevista ? formatarData(e.dataLiberacaoPrevista) : <span className="text-texto-suave">—</span>}
              </td>
              <td className="px-4 py-3 text-texto">{ORIGENS[e.origem]}</td>
              <td className="px-4 py-3 text-texto">
                {e.nomeMedico}
                <span className="ml-1 text-xs text-texto-suave">({e.tipoMedico === "Interno" ? "interno" : "externo"})</span>
              </td>
              <td className="px-4 py-3 text-texto">{formatarMoeda(e.preco)}</td>
              <td className="px-4 py-3">
                {e.excluido ? <Etiqueta cor="vermelho">Excluído</Etiqueta> : <EtiquetaEstado estado={e.estado} />}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
