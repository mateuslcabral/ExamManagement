"use client";

import { useActionState, useEffect, useState } from "react";
import type { ExameCatalogo } from "@/lib/api";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { atualizarItemCatalogo, criarItemCatalogo, type EstadoCatalogo } from "./actions";

export function FormularioCatalogo({
  item,
  diasRevisao,
  aoConcluir,
}: {
  item?: ExameCatalogo;
  diasRevisao: number;
  aoConcluir?: () => void;
}) {
  const acaoServidor = item ? atualizarItemCatalogo.bind(null, item.id) : criarItemCatalogo;
  const [estado, acao, pendente] = useActionState<EstadoCatalogo, FormData>(acaoServidor, {});

  useEffect(() => {
    if (estado.sucesso && item) aoConcluir?.();
  }, [estado, item, aoConcluir]);

  // Em modo "novo", cada sucesso remonta os campos (key) e limpa o formulário.
  const chave = item ? item.id : (estado.sucesso ?? "");

  return (
    <form action={acao} className="mt-4 space-y-4">
      <CamposCatalogo key={chave} item={item} diasRevisao={diasRevisao} estado={estado} />

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}
      {estado.sucesso && !item && (
        <p role="status" className="rounded-lg bg-sucesso/10 px-3 py-2 text-sm text-sucesso">
          {estado.sucesso}
        </p>
      )}

      <div className="flex justify-end gap-2">
        {item && (
          <Botao type="button" variante="fantasma" onClick={aoConcluir} disabled={pendente}>
            Cancelar
          </Botao>
        )}
        <Botao type="submit" className={item ? "" : "w-full"} carregando={pendente}>
          {item ? "Salvar" : "Adicionar ao catálogo"}
        </Botao>
      </div>
    </form>
  );
}

function CamposCatalogo({ item, diasRevisao, estado }: { item?: ExameCatalogo; diasRevisao: number; estado: EstadoCatalogo }) {
  const [prazo, setPrazo] = useState(item ? String(item.prazoExecucaoDias) : "");
  const prazoNum = Number(prazo);
  const previsao = prazo && Number.isInteger(prazoNum) && prazoNum > 0 ? prazoNum + diasRevisao : null;

  return (
    <>
      <Campo label="Nome do exame" name="nome" required autoComplete="off" defaultValue={item?.nome} erro={estado.campos?.nome} />
      <div className="grid gap-4 sm:grid-cols-2">
        <Campo
          label="Prazo de execução (dias)"
          name="prazoExecucaoDias"
          type="number"
          min={1}
          required
          value={prazo}
          onChange={(e) => setPrazo(e.target.value)}
          erro={estado.campos?.prazoExecucaoDias}
          dica={previsao ? `Entrega ao paciente: ${previsao} dias (+${diasRevisao} de revisão)` : "Não inclui a revisão da Cligen"}
        />
        <Campo
          label="Preço de referência (R$)"
          name="precoReferencia"
          inputMode="decimal"
          required
          placeholder="0,00"
          defaultValue={item ? item.precoReferencia.toFixed(2).replace(".", ",") : undefined}
          erro={estado.campos?.precoReferencia}
        />
      </div>
    </>
  );
}
