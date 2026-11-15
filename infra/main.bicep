// Infraestrutura do site no Azure Container Apps.
// Uso: veja docs/azure.md

@description('Região do Azure. Brazil South fica mais perto do público brasileiro.')
param location string = resourceGroup().location

@description('Prefixo dos nomes dos recursos.')
param appName string = 'portfolio'

@description('Imagem do contêiner, ex.: ghcr.io/seu-usuario/portfolio:latest')
param image string

@description('Chave da API do Resend (opcional). Sem ela, o formulário só registra no log.')
@secure()
param resendApiKey string = ''

@description('Remetente do e-mail de contato (domínio verificado no Resend).')
param emailFrom string = ''

@description('Para onde vão as mensagens do formulário.')
param emailTo string = ''

@description('Instâncias sempre ligadas. 1 = abre rápido (custo fixo pequeno). 0 = desliga sem acesso (quase grátis, primeiro acesso lento).')
@minValue(0)
@maxValue(1)
param minReplicas int = 1

var emailEnabled = !empty(resendApiKey) && !empty(emailFrom) && !empty(emailTo)

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${appName}-logs'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource env 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${appName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: location
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      secrets: emailEnabled ? [
        { name: 'resend-api-key', value: resendApiKey }
      ] : []
    }
    template: {
      containers: [
        {
          name: appName
          image: image
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: concat([
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
          ], emailEnabled ? [
            { name: 'Email__ResendApiKey', secretRef: 'resend-api-key' }
            { name: 'Email__From', value: emailFrom }
            { name: 'Email__To', value: emailTo }
          ] : [])
          probes: [
            {
              type: 'Liveness'
              httpGet: { path: '/healthz', port: 8080 }
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: { path: '/healthz', port: 8080 }
              periodSeconds: 10
            }
          ]
        }
      ]
      // minReplicas = 1: uma instância sempre ligada, sem atraso no primeiro acesso.
      // Para economizar, faça o deploy com minReplicas=0 (escala até zero).
      scale: {
        minReplicas: minReplicas
        maxReplicas: 1
      }
    }
  }
}

output url string = 'https://${app.properties.configuration.ingress.fqdn}'
output appName string = app.name
