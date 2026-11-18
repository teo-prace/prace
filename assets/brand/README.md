# Arquivos de marca

Originais e versões que não são servidas pelo site (o site usa `src/Portfolio.Web/wwwroot/img`).

| Arquivo | Uso |
|---|---|
| `teo-avatar-original.png` | Original do retrato (1199×1312, fundo transparente). Não editar; gerar versões a partir dele. |
| `teo-avatar-512-dark.png` | Quadrado no rosto, com o fundo escuro do site. Foto de perfil (LinkedIn, GitHub, WhatsApp). |

## Versões do site (`wwwroot/img`)

| Arquivo | Tamanho | Onde |
|---|---|---|
| `teo-480.webp` | 480×525 | Retrato (telas normais) |
| `teo-960.webp` | 960×1050 | Retrato (telas de alta densidade) |
| `teo-avatar-512.webp` | 512×512 | Avatar quadrado, transparente |

Geradas com ImageSharp: WebP com perda, qualidade 82, redimensionamento Lanczos3.
Recorte do avatar: x=190, y=0, 880×880 no original.
Ao trocar o retrato, gere de novo os três arquivos com os mesmos nomes e tamanhos.
