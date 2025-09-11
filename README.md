# Waitify API

<div align="center">

[![Go](https://img.shields.io/badge/Go-%2300ADD8.svg?logo=go&logoColor=white)](#)
[![AWS](https://img.shields.io/badge/AWS-%23FF9900.svg?logo=amazon-aws&logoColor=white)](#)
[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)
[![Stripe](https://img.shields.io/badge/Stripe-5851DD?logo=stripe&logoColor=fff)](#)

<h3>Système de file d'attente virtuelle par QR code pour commerçants</h3>

API REST sécurisée gérant l'authentification, les queues temps réel et la facturation automatique.

[Demo](https://waitify.fr) · [Documentation](https://github.com/StevenYAMBOS/waitify-api/wiki) · [Signaler un bug](https://github.com/StevenYAMBOS/waitify-api/issues) · [Nouvelle fonctionnalité](https://github.com/StevenYAMBOS/waitify-api/issues)

</div>

## À propos

Waitify est un SaaS français de gestion de files d'attente virtuelles par QR code. La solution permet aux commerçants de digitaliser leurs files d'attente et aux clients d'attendre sans contrainte physique.

## Technologies

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Runtime | Go | 1.21+ |
| Framework | Gin/Echo | Latest |
| Base de données | PostgreSQL | 15+ |
| Infrastructure | AWS | RDS/Lambda/ECS |
| Paiements | Stripe | API v2023 |
| Authentification | JWT | RS256 |
| SMS | AWS SNS | Latest |

## Installation

### Prérequis

- Go 1.21 ou supérieur
- PostgreSQL 15 ou supérieur
- Compte AWS configuré
- Clés API Stripe (test/prod)

### Lancement

```bash
# Installation des dépendances
go mod download

# Développement
go run main.go

# Build
go build -o waitify-api

# Production
./waitify-api
```

L'API sera accessible sur `http://localhost:8080`

## Modèles de données

### Queue entry

```json
{
  "id": "uuid",
  "phone": "0123456789",
  "position": 3,
  "estimated_wait_time": 12,
  "status": "waiting",
  "created_at": "2024-01-15T10:30:00Z"
}
```

### Business status

```json
{
  "is_queue_active": true,
  "is_queue_paused": false,
  "current_queue_size": 8,
  "average_wait_time": 15,
  "today_served": 42
}
```

### Analytics response

```json
{
  "today": {
    "clients_served": 42,
    "clients_missed": 3,
    "average_wait_time": 12,
    "peak_hour": 14,
    "abandonment_rate": 7.1
  },
  "week": {
    "total_clients": 280,
    "busiest_day": "saturday",
    "revenue_potential": 156.00
  }
}
```

## Logique métier

### Système de queue

1. Client scanne QR code unique du business
2. Inscription avec numéro de téléphone
3. Attribution position automatique + estimation temps
4. SMS de confirmation envoyé immédiatement
5. SMS de rappel quand 2 clients restent devant
6. SMS final quand c'est le tour du client
7. Timer 5 minutes avant passage automatique au suivant

### Gestion des abandons

- Annulation manuelle : position libérée, SMS confirmation
- Timeout : passage automatique, SMS "tour manqué"
- Recalcul automatique des positions restantes
- Notification clients suivants (temps réduit)

### Facturation automatique

- Calcul mensuel basé sur la consommation SMS
- 19€/mois incluant 1000 SMS
- 0.03€ par SMS supplémentaire
- Génération facture via Stripe
- Suspension automatique en cas d'impayé

## Sécurité

### Row Level Security (RLS)

Chaque utilisateur ne peut accéder qu'à ses propres données via les politiques PostgreSQL.

### Validation des données

- Validation struct avec tags Go
- Nettoyage des numéros de téléphone
- Rate limiting avec middleware
- Protection CSRF et XSS

### CORS

Configuration stricte limitée aux domaines autorisés en production.

## Monitoring

### Logs structurés

```go
log.WithFields(log.Fields{
    "business_id": businessID,
    "phone": "06xxxxxxxx",
    "position": 3,
    "wait_time": 12,
}).Info("Queue joined")
```

### Métriques surveillées

- Temps de réponse API
- Taux de succès SMS
- Erreurs base de données
- Consommation ressources AWS

### Health check

```
GET /health
```

Retourne le statut des services externes (PostgreSQL, Stripe, AWS SNS).

## Scripts utiles

```bash
# Développement
go run main.go          # Serveur avec hot-reload
go run -race main.go    # Mode debug avec détection race conditions

# Tests
go test ./...           # Suite complète
go test -v ./internal/  # Tests unitaires détaillés
go test -bench=.        # Tests de performance

# Production
go build -ldflags="-s -w" -o waitify-api  # Build optimisé
./waitify-api                             # Serveur production

# Database
migrate -path ./migrations -database postgres://... up    # Migrations
go run cmd/seed/main.go                                    # Données de test
psql -d waitify -f scripts/reset.sql                      # Reset complet
```

## Déploiement AWS

### Infrastructure

- **ECS Fargate** : Containers serverless
- **RDS PostgreSQL** : Base de données managée
- **Application Load Balancer** : Distribution du trafic
- **CloudWatch** : Monitoring et logs
- **SNS** : Notifications SMS

### Variables production

Configuration via AWS Systems Manager Parameter Store :

- Credentials base de données RDS
- Clés Stripe live
- Secrets JWT
- Configuration CORS

### Monitoring production

- CloudWatch Logs centralisés
- Alertes CloudWatch sur erreurs critiques
- RDS Performance Insights
- Backup automatique RDS quotidien

## Webhooks Stripe

### Configuration requise

URL webhook : `https://api.waitify.fr/billing/webhooks`

Événements écoutés :

- `invoice.payment_succeeded`
- `invoice.payment_failed`
- `customer.subscription.deleted`

### Gestion des échecs de paiement

1. Premier échec : email de relance automatique
2. Deuxième échec : suspension partielle (lecture seule)
3. Troisième échec : suspension complète du compte

## Limites et quotas

| Ressource | Limite | Action |
|-----------|--------|---------|
| Queue size | 200 clients | Inscription refusée |
| SMS/mois | 1000 inclus | Facturation +0.03€ |
| API calls | 1000/min | Rate limiting |
| Timeout client | 5 minutes | Passage automatique |

## Codes d'erreur

| Code | Description | Action recommandée |
|------|-------------|-------------------|
| 4001 | Queue fermée | Réessayer plus tard |
| 4002 | Queue pleine | Réessayer plus tard |
| 4003 | Client déjà inscrit | Vérifier statut existant |
| 4004 | Business suspendu | Contacter support |
| 5001 | Erreur SMS | Réessayer, fallback notification |
| 5002 | Erreur Stripe | Vérifier configuration |

<div align="center">

**Développé avec ❤️ par l'équipe SOBMAY**
</div>
