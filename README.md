# Waitify API

<div align="center">

[![Go](https://img.shields.io/badge/Go-%2300ADD8.svg?logo=go&logoColor=white)](#)
[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=fff)](#)
[![AWS](https://img.shields.io/badge/AWS-%23FF9900.svg?logo=amazon-aws&logoColor=white)](#)
[![Google Cloud](https://img.shields.io/badge/Google%20Cloud-%234285F4.svg?logo=google-cloud&logoColor=white)](#)
[![DigitalOcean](https://img.shields.io/badge/DigitalOcean-%230167ff.svg?logo=digitalOcean&logoColor=white)](#)
[![Stripe](https://img.shields.io/badge/Stripe-5851DD?logo=stripe&logoColor=fff)](#)

<h3>Système de file d'attente virtuelle par QR code pour commerçants</h3>

API REST sécurisée gérant l'authentification, les queues temps réel et la facturation automatique.

[Demo](https://waitify.fr) · [Documentation](https://github.com/StevenYAMBOS/waitify-api/tree/prod/documentation) · [Signaler un bug](https://github.com/StevenYAMBOS/waitify-api/issues) · [Nouvelle fonctionnalité](https://github.com/StevenYAMBOS/waitify-api/issues)

</div>

## À propos

Waitify est un SaaS français de gestion de files d'attente virtuelles par QR code. La solution permet aux commerçants de digitaliser leurs files d'attente et aux clients d'attendre sans contrainte physique.

## Technologies

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Runtime | Go | 1.24.3 |
| Framework | Aucun | Aucune |
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
go run cmd/main.go

# Build
go build -o cmd/waitify-api

# Production
./waitify-api
```

L'API sera accessible sur `http://localhost:3000`

## Modèles de données

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

<div align="center">

**Développé par [Steven YAMBOS](https://www.linkedin.com/in/steven-yambos/)**
</div>
