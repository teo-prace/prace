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
$appId = az ad app create --display-name gh-portfolio-deploy --query appId -o tsv
az ad sp create --id $appId
az role assignment create --assignee $appId --role Contributor --scope $rg

# Confiança no GitHub (troque <usuario>/<repo>)
@'
{
  "name": "github-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<usuario>/<repo>:environment:production",
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

## 7. Alerta de orçamento (recomendado)

No portal do Azure: **Cost Management → Budgets → Add**, valor um pouco acima do esperado (ex.: US$ 20/mês), com alerta por e-mail em 50%, 80% e 100%.

## Observações

- **Economizar**: `--parameters minReplicas=0` desliga a instância quando ninguém acessa. O custo cai para
  perto de zero, mas o primeiro visitante depois de um tempo parado espera alguns segundos.
- **Logs**: portal do Azure → Container App `portfolio` → **Log stream**.
