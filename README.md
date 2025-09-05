# Waitify API

<div align="center">

[![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?logo=typescript&logoColor=fff)](#)
[![NodeJS](https://img.shields.io/badge/Node.js-6DA55F?logo=node.js&logoColor=white)](#)
[![Express.js](https://img.shields.io/badge/Express.js-%23404d59.svg?logo=express&logoColor=%2361DAFB)](#)
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
| Runtime | Node.js | 18+ |
| Framework | Express.js | 4.x |
| Base de données | Supabase | PostgreSQL |
| Paiements | Stripe | API v2023 |
| Authentification | Supabase Auth | JWT |
| SMS | ? | - |

## Installation

### Prérequis
- Node.js 18 ou supérieur
- Compte Supabase configuré
- Clés API Stripe (test/prod)
- Accès API SMS (Twilio/Orange)

### Configuration environnement
```bash
cp .env.example .env
```

Variables d'environnement requises :
```bash
# Database
SUPABASE_URL=your_supabase_url
SUPABASE_ANON_KEY=your_anon_key
SUPABASE_SERVICE_ROLE_KEY=your_service_key

# Stripe
STRIPE_PUBLISHABLE_KEY=pk_test_xxx
STRIPE_SECRET_KEY=sk_test_xxx
STRIPE_WEBHOOK_SECRET=whsec_xxx

# SMS Provider pas encore défini

# App Config
PORT=3000
NODE_ENV=development
JWT_SECRET=your_jwt_secret
CORS_ORIGIN=http://localhost:5173 # tout dépend ici, 5173 sert d'exemple
```

### Lancement
```bash
npm install
npm run dev
```

L'API sera accessible sur `http://localhost:3000`

## Architecture API

### Authentification
Toutes les routes protégées nécessitent un token JWT Bearer dans l'header Authorization.

```bash
Authorization: Bearer <supabase_jwt_token>
```

### Routes principales

#### Authentification
```
POST /auth/register     # Inscription utilisateur
POST /auth/login        # Connexion
POST /auth/logout       # Déconnexion
GET  /auth/profile      # Profil utilisateur
PUT  /auth/profile      # Mise à jour profil
```

#### Business management
```
GET    /business           # Détails du business
PUT    /business           # Mise à jour paramètres
POST   /business/qr-code   # Génération QR code
PUT    /business/queue     # Activation/pause queue
```

#### File d'attente
```
POST   /queue/join         # Inscription client (public)
GET    /queue/status/:id   # Position client (public)
DELETE /queue/cancel/:id   # Annulation client (public)
GET    /queue/list         # Liste complète (privé)
POST   /queue/next         # Client suivant (privé)
PUT    /queue/client/:id   # Marquer servi/manqué (privé)
```

#### Analytics
```
GET /analytics/dashboard    # Métriques temps réel
GET /analytics/daily        # Statistiques quotidiennes
GET /analytics/weekly       # Analyse hebdomadaire
GET /analytics/export       # Export données CSV
```

#### Billing et webhooks
```
GET  /billing/invoices      # Liste des factures
POST /billing/webhooks      # Webhooks Stripe
GET  /billing/usage         # Consommation SMS
```

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
Chaque utilisateur ne peut accéder qu'à ses propres données via les politiques Supabase.

### Validation des données
- Validation Joi sur toutes les entrées
- Nettoyage des numéros de téléphone
- Rate limiting sur les inscriptions
- Protection CSRF et XSS

### CORS
Configuration stricte limitée aux domaines autorisés en production.

## Monitoring

### Logs structurés
```javascript
logger.info('Queue joined', {
  business_id: 'uuid',
  phone: '06xxxxxxxx',
  position: 3,
  wait_time: 12
});
```

### Métriques surveillées
- Temps de réponse API
- Taux de succès SMS
- Erreurs base de données
- Consommation ressources

### Health check
```
GET /health
```
Retourne le statut des services externes (Supabase, Stripe, SMS).

## Scripts utiles

```bash
# Développement
npm run dev          # Serveur avec hot-reload
npm run dev:debug    # Mode debug avec logs détaillés

# Tests
npm test             # Suite complète
npm run test:unit    # Tests unitaires
npm run test:api     # Tests d'intégration

# Production
npm start            # Serveur production
npm run build        # Build optimisé

# Database
npm run db:migrate   # Migrations Supabase
npm run db:seed      # Données de test
npm run db:reset     # Reset complet
```

## Déploiement

### Variables production
Configurer les variables d'environnement sur la plateforme de déploiement :
- URLs Supabase production
- Clés Stripe live
- Webhooks sécurisés
- CORS origins production

### Monitoring production
- Logs centralisés via service externe
- Alertes sur erreurs critiques
- Surveillance performance base
- Backup automatique quotidien

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

<div align="center">

**Développé avec ❤️ par l'équipe SOBMAY**
</div>
