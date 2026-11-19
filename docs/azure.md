# Publicar no Azure

Arquitetura:

```
GitHub (push na main)
  └─ GitHub Actions: testes → imagem Docker → ghcr.io (GitHub Container Registry, grátis)
                                                  └─ Azure Container Apps (1 instância sempre ligada)
                                                        └─ seu domínio, com HTTPS grátis
```

Custo esperado: poucos dólares por mês (estimativa de US$ 5 a 15; confira na
[calculadora de preços](https://azure.microsoft.com/pricing/calculator/)), porque uma instância fica
sempre ligada para o site abrir rápido. Para custo perto de zero, faça o deploy com `minReplicas=0`
(o primeiro acesso depois de um tempo parado fica lento). Configure o **alerta de orçamento** (passo 7) no primeiro dia.

---

## 1. Pré-requisitos (uma vez só)

```powershell
winget install Microsoft.AzureCLI
winget install GitHub.cli          # opcional, facilita os passos com o GitHub
```

- Conta no Azure (https://azure.microsoft.com/free)
- Repositório no GitHub com este projeto

## 2. Primeiro push e imagem

1. Crie o repositório no GitHub e faça o push da branch `main`.
2. O workflow **CI/CD** roda: testes → imagem em `ghcr.io/<usuario>/<repo>`.
   O passo de deploy é pulado (ainda não existe Azure configurado). Isso é esperado.
3. No GitHub, vá em **seu perfil → Packages → (o pacote) → Package settings → Change visibility → Public**.
   Assim o Azure consegue baixar a imagem sem senha.

## 3. Criar a infraestrutura

```powershell
az login
az group create --name rg-portfolio --location brazilsouth

az deployment group create `
  --resource-group rg-portfolio `
  --template-file infra/main.bicep `
  --parameters image=ghcr.io/<usuario>/<repo>:latest
```

A saída mostra a `url` do site (algo como `https://portfolio.<...>.brazilsouth.azurecontainerapps.io`).

## 4. Deploy automático (GitHub Actions → Azure, sem senha guardada)

Usa OIDC: o GitHub prova a identidade para o Azure, sem segredo de longa duração.

```powershell
$sub = az account show --query id -o tsv
$tenant = az account show --query tenantId -o tsv
$rg = az group show -n rg-portfolio --query id -o tsv

# Identidade do pipeline
$appId = az ad app create --display-name gh-prace-deploy --query appId -o tsv
az ad sp create --id $appId
Start-Sleep -Seconds 30
az role assignment create --assignee $appId --role Contributor --scope $rg

# Confiança no GitHub. O subject precisa ser EXATAMENTE o que o GitHub envia.
# Repositórios novos usam o formato com IDs numéricos: repo:<usuario>@<id-usuario>/<repo>@<id-repo>:environment:production
# (os IDs aparecem na mensagem de erro AADSTS700213 do primeiro deploy, ou em
#  https://api.github.com/users/<usuario> e https://api.github.com/repos/<usuario>/<repo>, campo "id").
# Neste projeto: repo:teo-prace@355556173/prace@1459210037:environment:production
@'
{
  "name": "github-production-ids",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:teo-prace@355556173/prace@1459210037:environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}
'@ | Out-File -Encoding utf8 fed.json
az ad app federated-credential create --id $appId --parameters fed.json
Remove-Item fed.json

"AZURE_CLIENT_ID=$appId"; "AZURE_TENANT_ID=$tenant"; "AZURE_SUBSCRIPTION_ID=$sub"
```

No GitHub, em **Settings → Secrets and variables → Actions**:

| Tipo | Nome | Valor |
|---|---|---|
| Secret | `AZURE_CLIENT_ID` | (saída acima) |
| Secret | `AZURE_TENANT_ID` | (saída acima) |
| Secret | `AZURE_SUBSCRIPTION_ID` | (saída acima) |
| Variable | `AZURE_RESOURCE_GROUP` | `rg-portfolio` |
| Variable | `AZURE_CONTAINERAPP_NAME` | `portfolio` |

A partir daí, todo push na `main` testa, gera a imagem e atualiza o site.

## 5. Domínio próprio

1. Compre o domínio (Registro.br para `.com.br`, ou outro registrador para `.dev`/`.com`).
2. Adicione o domínio com certificado gratuito:

```powershell
az containerapp hostname add --hostname www.seudominio.com.br -g rg-portfolio -n portfolio
# O comando mostra os registros DNS (TXT de verificação + CNAME) para criar no registrador.
# Depois de criados:
az containerapp hostname bind --hostname www.seudominio.com.br -g rg-portfolio -n portfolio `
  --environment portfolio-env --validation-method CNAME
```

> **Atenção:** depois de ligar o domínio, confira se `customDomain` e `customDomainCertificateName`
> em `infra/main.bicep` batem com o que está no Azure
> (`az containerapp env certificate list -g rg-portfolio -n portfolio-env --managed-certificates-only -o table`).
> Rodar `az deployment group create` com o Bicep desatualizado **desliga o domínio**.
> Para só trocar a versão do site, use `az containerapp update --image` (é o que o GitHub Actions faz).

## 6. E-mail do formulário (Resend)

1. Crie a conta em https://resend.com e verifique seu domínio (registros DNS que eles indicam).
2. Gere uma API key.
3. Rode o deploy da infraestrutura de novo com os parâmetros de e-mail:

```powershell
az deployment group create -g rg-portfolio --template-file infra/main.bicep `
  --parameters image=ghcr.io/<usuario>/<repo>:latest `
               resendApiKey=<sua-chave> emailFrom="Site <contato@seudominio.com.br>" emailTo=<seu-email>
```

A chave fica como **secret** no Azure. Nunca coloque no `appsettings.json` nem no Git.

## 7. Proteção de custo

A assinatura é Pay-As-You-Go: o Azure **não tem trava de gasto**, só avisos. Por isso são três camadas.
Atenção: a conta é cobrada em **BRL**; todos os valores abaixo estão na moeda da conta.

**a) Orçamento com alerta** (`orcamento-mensal`, por API; `az consumption budget list` não mostra):
e-mail para teo@prace.dev em 50%, 80% e 100% do gasto real e 100% da previsão do mês.

**b) Política "Somente recursos do site Prace"** (`tipos-permitidos`, regra pronta "Allowed resource types"):
na assinatura só podem ser criados Container Apps, o ambiente, certificados e Log Analytics.
Erro `RequestDisallowedByPolicy` = essa política. Para liberar um tipo novo, recrie a atribuição com a lista atualizada.

**c) Guarda de custo** (`.github/workflows/cost-guard.yml`): a cada 6 h lê o gasto do mês e, se passar de
`COST_LIMIT` (variável do GitHub, padrão 120), bloqueia todo acesso e zera as instâncias. Domínio e certificado
continuam configurados. O workflow falha de propósito para o GitHub mandar e-mail.

- Religar: **Actions → Guarda de custo → Run workflow → religar**.
- Testar o desligamento: mesmo caminho, opção **desligar** (depois **religar**).
- Requisito, uma vez só: a identidade do deploy precisa ler custos.

```powershell
$sub = az account show --query id -o tsv
$sp  = az ad sp show --id <AZURE_CLIENT_ID> --query id -o tsv
az role assignment create --assignee-object-id $sp --assignee-principal-type ServicePrincipal `
  --role "Cost Management Reader" --scope "/subscriptions/$sub"
```

O custo no Azure chega com 8 a 24 h de atraso; a guarda limita o estrago, não zera.

## Observações

- **Economizar**: `--parameters minReplicas=0` desliga a instância quando ninguém acessa. O custo cai para
  perto de zero, mas o primeiro visitante depois de um tempo parado espera alguns segundos.
- **Logs**: portal do Azure → Container App `portfolio` → **Log stream**.
