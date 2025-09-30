# Waitify API

<div align="center">

<<<<<<< HEAD
[![Go](https://img.shields.io/badge/Go-%2300ADD8.svg?logo=go&logoColor=white)](#)
[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)
[![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=fff)](#)
[![AWS](https://img.shields.io/badge/AWS-%23FF9900.svg?logo=amazon-aws&logoColor=white)](#)
[![Google Cloud](https://img.shields.io/badge/Google%20Cloud-%234285F4.svg?logo=google-cloud&logoColor=white)](#)
[![DigitalOcean](https://img.shields.io/badge/DigitalOcean-%230167ff.svg?logo=digitalOcean&logoColor=white)](#)
[![Stripe](https://img.shields.io/badge/Stripe-5851DD?logo=stripe&logoColor=fff)](#)
=======
![Java](https://img.shields.io/badge/Java-ED8B00?style=for-the-badge&logo=openjdk&logoColor=white)
![Spring Boot](https://img.shields.io/badge/Spring_Boot-6DB33F?style=for-the-badge&logo=spring-boot&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![AWS](https://img.shields.io/badge/AWS-FF9900?style=for-the-badge&logo=amazonaws&logoColor=white)
![Google Cloud](https://img.shields.io/badge/Google_Cloud-4285F4?style=for-the-badge&logo=googlecloud&logoColor=white)
![DigitalOcean](https://img.shields.io/badge/DigitalOcean-0080FF?style=for-the-badge&logo=digitalocean&logoColor=white)
![Stripe](https://img.shields.io/badge/Stripe-008CDD?style=for-the-badge&logo=stripe&logoColor=white)
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4

<h3>Système de file d'attente virtuelle par QR code pour commerçants</h3>

API REST sécurisée gérant l'authentification, les queues temps réel et la facturation automatique.

[Demo](https://waitify.fr) · [Documentation](https://github.com/StevenYAMBOS/waitify-api/tree/prod/documentation) · [Signaler un bug](https://github.com/StevenYAMBOS/waitify-api/issues) · [Nouvelle fonctionnalité](https://github.com/StevenYAMBOS/waitify-api/issues)

</div>

## À propos

Waitify est un SaaS français de gestion de files d'attente virtuelles par QR code. La solution permet aux commerçants de digitaliser leurs files d'attente et aux clients d'attendre sans contrainte physique.

## Technologies

| Composant | Technologie | Version |
|-----------|-------------|---------|
<<<<<<< HEAD
| Runtime | Go | 1.24.3 |
| Framework | Aucun | Aucune |
| Base de données | PostgreSQL | 15+ |
| Infrastructure | AWS | RDS/Lambda/ECS |
=======
| Runtime | Java | 21 |
| Framework | Spring Boot | 3.x |
| Base de données | PostgreSQL | 15+ |
| Infrastructure | AWS/GCP/DO | Multi-cloud |
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4
| Paiements | Stripe | API v2023 |
| Authentification | JWT | RS256 |
| SMS | AWS SNS | Latest |

## Installation

### Prérequis
<<<<<<< HEAD

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

=======
- Java 21 ou supérieur
- PostgreSQL 15 ou supérieur
- Maven 3.9+
- Clés API Stripe (test/prod)

### Lancement
```bash
# Installation des dépendances
mvn clean install

# Développement
mvn spring-boot:run

# Build
mvn package

# Production
java -jar target/waitify-api.jar
```

L'API sera accessible sur `http://localhost:8080`

## Logique métier

### Système de queue
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4
1. Client scanne QR code unique du business
2. Inscription avec numéro de téléphone
3. Attribution position automatique + estimation temps
4. SMS de confirmation envoyé immédiatement
5. SMS de rappel quand 2 clients restent devant
6. SMS final quand c'est le tour du client
7. Timer 5 minutes avant passage automatique au suivant

### Gestion des abandons
<<<<<<< HEAD

=======
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4
- Annulation manuelle : position libérée, SMS confirmation
- Timeout : passage automatique, SMS "tour manqué"
- Recalcul automatique des positions restantes
- Notification clients suivants (temps réduit)

### Facturation automatique
<<<<<<< HEAD

=======
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4
- Calcul mensuel basé sur la consommation SMS
- 19€/mois incluant 1000 SMS
- 0.03€ par SMS supplémentaire
- Génération facture via Stripe
- Suspension automatique en cas d'impayé

<<<<<<< HEAD
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
=======
---

<div align="center">

### Développé par Steven YAMBOS

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/steven-yambos/)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/StevenYAMBOS)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/StevenYambos)
[![Stack Overflow](https://img.shields.io/badge/Stack_Overflow-F58025?style=for-the-badge&logo=stackoverflow&logoColor=white)](https://stackoverflow.com/users/17386694/steven-yambos)

</div>
>>>>>>> 4563f6b1509c93d488f1f815f313b6c2126a4fa4
