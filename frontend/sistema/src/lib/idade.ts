/** Regra P19: menor de idade = não completou 18 anos hoje. Espelha Paciente.EhMenorDeIdade no domínio. */
export function ehMenorDeIdade(dataNascimento: string): boolean {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(dataNascimento);
  if (!m) return false;
  const nasc = new Date(Date.UTC(+m[1], +m[2] - 1, +m[3]));
  const hoje = new Date();
  const limite = new Date(Date.UTC(hoje.getFullYear() - 18, hoje.getMonth(), hoje.getDate()));
  return nasc > limite;
}

export function formatarData(iso: string): string {
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso);
  return m ? `${m[3]}/${m[2]}/${m[1]}` : iso;
}

export function formatarTelefone(digitos: string): string {
  const d = digitos.replace(/\D/g, "");
  const local = d.length > 11 ? d.slice(-11) : d;
  const ddi = d.length > 11 ? `+${d.slice(0, d.length - 11)} ` : "";
  if (local.length === 11) return `${ddi}(${local.slice(0, 2)}) ${local.slice(2, 7)}-${local.slice(7)}`;
  if (local.length === 10) return `${ddi}(${local.slice(0, 2)}) ${local.slice(2, 6)}-${local.slice(6)}`;
  return digitos;
}

export function formatarMoeda(valor: number): string {
  return valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

export function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}
