"use client";

import Link from "next/link";
import { useActionState, useState } from "react";
import type { Paciente } from "@/lib/api";
import { ehMenorDeIdade } from "@/lib/idade";
import { Botao } from "@/components/ui/botao";
import { Campo } from "@/components/ui/campo";
import { Selecao } from "@/components/ui/selecao";
import { atualizarPaciente, criarPaciente, type EstadoSalvar } from "./actions";

export function FormularioPaciente({ paciente }: { paciente?: Paciente }) {
  const acaoServidor = paciente ? atualizarPaciente.bind(null, paciente.id) : criarPaciente;
  const [estado, acao, pendente] = useActionState<EstadoSalvar, FormData>(acaoServidor, {});

  // Valores: após erro do servidor vêm em estado.valores; senão, do paciente em edição.
  const v = (campo: keyof NonNullable<EstadoSalvar["valores"]>, original = "") => estado.valores?.[campo] ?? original;

  const [dataNascimento, setDataNascimento] = useState(v("dataNascimento", paciente?.dataNascimento.slice(0, 10)));
  const menor = ehMenorDeIdade(dataNascimento);
  const r = paciente?.responsavelLegal;

  return (
    <form action={acao} className="space-y-6">
      <fieldset className="space-y-4">
        <legend className="text-sm font-semibold uppercase tracking-wider text-texto-suave">Dados do paciente</legend>

        <Campo label="Nome completo" name="nome" required defaultValue={v("nome", paciente?.nome)} erro={estado.campos?.nome} />

        <div className="grid gap-4 sm:grid-cols-3">
          <Campo
            label="Data de nascimento"
            name="dataNascimento"
            type="date"
            required
            value={dataNascimento}
            onChange={(e) => setDataNascimento(e.target.value)}
            erro={estado.campos?.dataNascimento}
            dica={menor ? "Menor de idade — responsável obrigatório" : undefined}
          />
          <Selecao
            label="Tipo de documento"
            name="tipoDocumento"
            required
            defaultValue={v("tipoDocumento", paciente?.tipoDocumento ?? "Cpf")}
            erro={estado.campos?.tipoDocumento}
          >
            <option value="Cpf">CPF</option>
            <option value="Passaporte">Passaporte</option>
          </Selecao>
          <Campo
            label="Número do documento"
            name="numeroDocumento"
            required
            autoComplete="off"
            defaultValue={v("numeroDocumento", paciente?.numeroDocumentoFormatado)}
            erro={estado.campos?.numeroDocumento}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Campo
            label={menor ? "E-mail (do responsável)" : "E-mail"}
            name="email"
            type="email"
            required
            autoComplete="off"
            defaultValue={v("email", paciente?.email)}
            erro={estado.campos?.email}
            dica="Destino das mensagens e do acesso ao portal"
          />
          <Campo
            label={menor ? "Telefone / WhatsApp (do responsável)" : "Telefone / WhatsApp"}
            name="telefone"
            type="tel"
            required
            autoComplete="off"
            placeholder="(31) 99999-9999"
            defaultValue={v("telefone", paciente?.telefone)}
            erro={estado.campos?.telefone}
          />
        </div>
      </fieldset>

      {menor && (
        <fieldset className="space-y-4 rounded-xl border border-alerta/40 bg-alerta/5 p-4">
          <legend className="px-1 text-sm font-semibold uppercase tracking-wider text-alerta">Responsável legal</legend>
          <p className="text-xs text-texto-suave">
            Paciente menor de idade: o responsável é atribuído no cadastro e recebe todas as comunicações.
          </p>
          <Campo
            label="Nome do responsável"
            name="responsavelNome"
            required
            defaultValue={v("responsavelNome", r?.nome)}
            erro={estado.campos?.responsavelNome}
          />
          <div className="grid gap-4 sm:grid-cols-3">
            <Selecao
              label="Tipo de documento"
              name="responsavelTipoDocumento"
              required
              defaultValue={v("responsavelTipoDocumento", r?.tipoDocumento ?? "Cpf")}
              erro={estado.campos?.responsavelTipoDocumento}
            >
              <option value="Cpf">CPF</option>
              <option value="Passaporte">Passaporte</option>
            </Selecao>
            <Campo
              label="Número do documento"
              name="responsavelNumeroDocumento"
              required
              autoComplete="off"
              defaultValue={v("responsavelNumeroDocumento", r?.numeroDocumentoFormatado)}
              erro={estado.campos?.responsavelNumeroDocumento}
            />
            <Campo
              label="Parentesco"
              name="responsavelParentesco"
              placeholder="Mãe, pai, tutor…"
              defaultValue={v("responsavelParentesco", r?.parentesco ?? "")}
              erro={estado.campos?.responsavelParentesco}
            />
          </div>
        </fieldset>
      )}

      {estado.erro && (
        <p role="alert" className="rounded-lg bg-erro/10 px-3 py-2 text-sm text-erro">
          {estado.erro}
        </p>
      )}

      <div className="flex flex-wrap items-center justify-end gap-3">
        <Link href={paciente ? `/pacientes/${paciente.id}` : "/pacientes"} className="text-sm font-medium text-texto-suave hover:text-texto">
          Cancelar
        </Link>
        <Botao type="submit" carregando={pendente}>
          {paciente ? "Salvar alterações" : "Cadastrar paciente"}
        </Botao>
      </div>
    </form>
  );
}
