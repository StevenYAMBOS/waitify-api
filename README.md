# Waitify API

<div align="center">

[![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)
[![Microsoft Azure](https://custom-icon-badges.demolab.com/badge/Microsoft%20Azure-0089D6?logo=msazure&logoColor=white)](#)
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

| Composant        | Technologie | Version        |
| ---------------- | ----------- | -------------- |
| Runtime          | Node.js     | > 20.0         |
| Langage          | C#          | 14             |
| Framework        | ASP.NET     | 10.0           |
| Base de données  | PostgreSQL  | 17+            |
| Infrastructure   | MC Azure    | RDS/Lambda/ECS |
| Paiements        | Stripe      | API v2023      |
| Authentification | JWT         | RS256          |
| SMS              | AWS SNS     | Latest         |

## Installation

### Prérequis

- .NETde 8.0 ou supérieur
- PostgreSQL 16 ou supérieur
- Compte Microsoft Azure configuré
- Clés API Stripe (test/prod)

### Lancement

```bash
# Installation des dépendances
nuget update

# Développement
dotnet run

# Build
dotnet build
```

L'API sera accessible sur `http://localhost:{port}/swagger/index.html`

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

- Nettoyage des numéros de téléphone
- Rate limiting avec middleware
- Protection CSRF et XSS

<div align="center">

Développé par **[Steven YAMBOS](https://www.linkedin.com/in/steven-yambos/)**

</div>
