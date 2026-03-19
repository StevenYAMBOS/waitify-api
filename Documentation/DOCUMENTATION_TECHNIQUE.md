# Documentation technique - Waitify API

**Dernière mise à jour :** 30 octobre 2025  
**Auteur :** Steven YAMBOS  
**Version :** 1.0.0

---

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Contexte et objectifs](#contexte-et-objectifs)
3. [Architecture technique](#architecture-technique)
4. [Types d'utilisateurs](#types-dutilisateurs)
5. [Fonctionnalités principales](#fonctionnalités-principales)
6. [Base de données](#base-de-données)
7. [Système de files d'attente](#système-de-files-dattente)
8. [Authentification et sécurité](#authentification-et-sécurité)
9. [Gestion des abonnements](#gestion-des-abonnements)
10. [API et endpoints](#api-et-endpoints)
11. [Technologies utilisées](#technologies-utilisées)
12. [Organisation du code](#organisation-du-code)
13. [Déploiement et infrastructure](#déploiement-et-infrastructure)

---

## Vue d'ensemble

**Waitify** est une **Software as a Service (SaaS)** française de gestion de files d'attente virtuelles par **QR Code** pour commerçants. La solution permet aux établissements de digitaliser leurs files d'attente physiques et aux clients d'attendre sans contrainte spatiale.

### Principe de fonctionnement

Le système fonctionne selon un modèle simple :

1. **Commerçant** : Crée son établissement sur la plateforme et génère un **QR Code unique**
2. **Client** : Scanne le QR Code avec son smartphone
3. **Inscription** : Le client s'inscrit avec son nom et numéro de téléphone
4. **Position** : Le système attribue automatiquement une position dans la file
5. **Notifications** : Le client reçoit des **SMS** pour suivre son avancement
6. **Service** : Le commerçant appelle le client suivant via l'interface

### Valeur ajoutée

- **Pour les commerçants** : Réduction des files d'attente physiques, meilleure organisation, statistiques détaillées
- **Pour les clients** : Attente libre, notifications en temps réel, pas de contrainte physique

---

## Contexte et objectifs

### Problématique résolue

Les commerces de proximité (boulangeries, pharmacies, coiffeurs, etc.) font face à plusieurs défis :

- **Files d'attente physiques** qui encombrent l'espace de vente
- **Perte de clients** qui abandonnent en voyant la file
- **Gestion manuelle** inefficace des clients en attente
- **Absence de données** sur les flux et les temps d'attente

### Solution proposée

Waitify digitalise complètement le processus :

- **QR Code permanent** par établissement
- **File d'attente virtuelle** gérée automatiquement
- **Notifications SMS** pour informer les clients
- **Tableau de bord** avec statistiques et analytics
- **Multi-établissements** pour les commerçants avec plusieurs points de vente

### Modèle économique

- **Essai gratuit** : 14 jours sans engagement
- **Abonnement mensuel** : Plans Basic (19€), Pro (49€), Enterprise (99€)
- **Facturation SMS** : 1000 SMS inclus, puis 0,03€ par SMS supplémentaire
- **Limite d'établissements** : Selon le plan d'abonnement

---

## Architecture technique

### Stack technologique

| Composant | Technologie | Version | Rôle |
|-----------|------------|---------|------|
| **Runtime** | Node.js | > 20.0 | Environnement d'exécution |
| **Langage** | TypeScript | 5.x | Langage de programmation typé |
| **Framework** | Express.js | 5.1.0 | Framework web pour API REST |
| **Base de données** | PostgreSQL | 15+ | Base de données relationnelle |
| **Authentification** | JWT (JSON Web Token) | RS256 | Tokens d'authentification |
| **Paiements** | Stripe | API v2023 | Gestion des paiements |
| **SMS** | AWS SNS | Latest | Envoi de notifications SMS |
| **Infrastructure** | AWS | RDS/Lambda/ECS | Hébergement cloud |

### Architecture en couches

```text
┌─────────────────────────────────────────┐
│         Client (Web/Mobile)            │
└─────────────────┬───────────────────────┘
                  │
                  │ HTTPS/REST API
                  │
┌─────────────────▼───────────────────────┐
│         Express.js Server               │
│  ┌───────────────────────────────────┐  │
│  │  Routes (auth, businesses, queues) │  │
│  └───────────────┬─────────────────────┘  │
│                  │                        │
│  ┌───────────────▼─────────────────────┐  │
│  │  Controllers (logique métier)      │  │
│  └───────────────┬─────────────────────┘  │
│                  │                        │
│  ┌───────────────▼─────────────────────┐  │
│  │  Middlewares (auth, validation)    │  │
│  └───────────────┬───────────────────────┘  │
└─────────────────┼───────────────────────┘
                  │
                  │ SQL Queries
                  │
┌─────────────────▼───────────────────────┐
│      PostgreSQL Database                │
│  ┌───────────────────────────────────┐  │
│  │  Tables + Triggers + RLS          │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
                  │
                  │ API Calls
                  │
┌─────────────────▼───────────────────────┐
│  Services externes (Stripe, AWS SNS)   │
└─────────────────────────────────────────┘
```

### Répartition des responsabilités

#### **PostgreSQL (Base de données)**

- **Stockage** des données utilisateurs, établissements, files d'attente
- **Triggers automatiques** pour le recalcul des positions
- **Row Level Security (RLS)** pour l'isolation des données
- **Contraintes d'intégrité** pour garantir la cohérence
- **Index optimisés** pour les performances

#### **TypeScript/Express.js (API)**

- **Validation métier** des requêtes
- **Calcul des temps d'attente** estimés
- **Gestion des erreurs** et messages utilisateur
- **Intégrations externes** (Stripe, SMS)
- **Authentification JWT** et autorisations

---

## Types d'utilisateurs

### 1. Commerçant (Utilisateur authentifié)

**Rôle** : Propriétaire ou gestionnaire d'un ou plusieurs établissements

**Fonctionnalités** :

- **Gestion des établissements** : Création, modification, suppression
- **Activation des files** : Ouvrir/fermer les files d'attente
- **Gestion des clients** : Appeler le suivant, marquer comme servi
- **Statistiques** : Tableaux de bord avec analytics
- **Configuration** : Horaires, temps de service, messages personnalisés
- **Facturation** : Suivi de la consommation SMS et factures

**Limites selon le plan** :

- **Basic** : 1 établissement maximum
- **Pro** : 5 établissements maximum
- **Enterprise** : Établissements illimités

### 2. Client (Utilisateur public)

**Rôle** : Personne qui rejoint une file d'attente via QR Code

**Fonctionnalités** :

- **Inscription** : Rejoindre une file via QR Code
- **Suivi** : Consulter sa position et temps d'attente
- **Annulation** : Annuler sa place dans la file
- **Notifications** : Recevoir des SMS d'information

**Accès** : Public, sans authentification requise (via QR Code uniquement)

---

## Fonctionnalités principales

### 1. Gestion des établissements

#### Création d'un établissement

Lors de la création, le système :

1. Génère un **UUID unique** pour l'établissement
2. Crée un **`qr_code_token`** unique (UUID v4)
3. Construit l'URL du QR Code : `https://app.waitify.fr/q/{qr_code_token}`
4. Génère l'image du QR Code (PDF/PNG) pour impression
5. Définit les valeurs par défaut selon le type d'établissement

#### Types d'établissements supportés

Le système supporte **30+ types** d'établissements avec des temps de service par défaut :

- **Commerce** : Boulangerie, pharmacie, garage, etc.
- **Services** : Coiffeur, salon de beauté, tatoueur, etc.
- **Professions libérales** : Médecin, dentiste, avocat, notaire, etc.
- **Services publics** : Préfecture, mairie, CAF, etc.
- **Autre** : Type personnalisable

#### Configuration d'un établissement

Chaque établissement peut être configuré avec :

- **Informations** : Nom, adresse, téléphone, ville, code postal
- **Horaires d'ouverture** : Format JSON par jour de la semaine
- **Temps de service moyen** : En secondes (utilisé pour les estimations)
- **Taille maximale de file** : Limite de clients simultanés (1-200)
- **Timeout client** : Délai avant passage automatique (1-30 minutes)
- **Messages personnalisés** : Texte inclus dans les SMS
- **Notifications SMS** : Activation/désactivation
- **Avance automatique** : Passage automatique au suivant si timeout

### 2. Système de files d'attente

#### Cycle de vie d'une file

Une file d'attente n'est **pas un objet explicite** en base de données. Elle existe implicitement via les entrées dans `queue_entries` avec le statut `waiting`.

**États d'une file** :

- **Inactive** : `is_queue_active = false` - Aucun client ne peut s'inscrire
- **Active** : `is_queue_active = true` - Les clients peuvent s'inscrire
- **En pause** : `is_queue_paused = true` - Temporairement suspendue

#### Inscription d'un client

**Flux complet** :

```text
1. Client scanne QR Code
   ↓
2. Redirection vers https://app.waitify.fr/q/{token}
   ↓
3. Formulaire web (nom + téléphone)
   ↓
4. POST /queues/join
   ↓
5. Validations API :
   - Établissement actif et file ouverte
   - File non pleine (max_queue_size)
   - Client pas déjà inscrit (même téléphone + statut 'waiting')
   - Format téléphone valide (français)
   ↓
6. Calcul position initiale : COUNT(waiting) + 1
   ↓
7. Calcul temps d'attente : (clients devant × temps_service) / 60
   ↓
8. INSERT dans queue_entries
   ↓
9. Trigger PostgreSQL recalcule toutes les positions
   ↓
10. SMS de confirmation envoyé
```

#### Gestion des positions

Les positions sont **entièrement automatisées** via des **triggers PostgreSQL** :

- **Calcul** : `ROW_NUMBER() OVER (ORDER BY created_at ASC)`
- **Filtres** : Même `BusinessId` + `status = 'waiting'`
- **Recalcul automatique** : Après chaque insertion, mise à jour ou suppression
- **Cohérence garantie** : Atomicité transactionnelle, pas de race conditions

#### États d'une entrée de file

**Machine à états** :

```text
waiting (initial)
  ├──→ called (commerçant appelle)
  │     ├──→ served (client présent)
  │     └──→ missed (timeout 5 min)
  │
  └──→ cancelled (client annule)
```

**Transitions autorisées** :

| De | Vers | Action | Recalcul positions |
|----|------|--------|-------------------|
| `waiting` | `called` | Commerçant appelle | ✅ Oui |
| `called` | `served` | Client servi | ✅ Oui |
| `called` | `missed` | Timeout 5 min | ✅ Oui |
| `waiting` | `cancelled` | Client annule | ✅ Oui |

**États finaux** (ne recalculent plus) :

- `served` : Client servi avec succès
- `missed` : Client absent lors de son appel
- `cancelled` : Client a annulé sa place

### 3. Notifications SMS

#### Types de messages

Le système envoie **5 types** de SMS :

1. **Confirmation** : À l'inscription

   ```text
   "Votre place #3 chez [Business] est confirmée. 
   Temps d'attente estimé: ~12min. 
   Rescannez le QR code pour suivre votre position."
   ```

2. **Rappel** : Quand 2 clients restent devant

   ```text
   "Rappel: Plus que 2 clients devant vous chez [Business]."
   ```

3. **Votre tour** : Quand c'est le tour du client

   ```text
   "C'est votre tour chez [Business]! 
   Présentez-vous au comptoir maintenant."
   ```

4. **Tour manqué** : Après timeout

   ```text
   "Votre tour chez [Business] est passé. 
   Rescannez le QR code pour vous réinscrire."
   ```

5. **Annulation** : Si le client annule

   ```text
   "Votre place chez [Business] a été annulée."
   ```

#### Gestion des quotas

- **Suivi** : Chaque SMS est loggé dans `sms_logs` avec le coût
- **Quotas mensuels** : Selon le plan d'abonnement
- **Dépassement** : Facturation à 0,03€ par SMS supplémentaire
- **Limitation** : Un SMS par type toutes les 5 minutes maximum (anti-spam)

### 4. Statistiques et analytics

#### Métriques collectées

Pour chaque établissement, le système calcule quotidiennement :

- **Clients servis** : Nombre total de clients effectivement servis
- **Clients manqués** : Nombre de clients absents (timeout)
- **Clients annulés** : Nombre d'annulations
- **Temps d'attente moyen** : En minutes
- **Temps de service moyen** : En secondes
- **Heure de pointe** : Période la plus chargée
- **Taille maximale de file** : Pic atteint dans la journée
- **Taux d'abandon** : Pourcentage de clients ayant abandonné
- **SMS envoyés** : Nombre total de notifications

#### Tableau de bord

Le commerçant peut consulter :

- **Vue quotidienne** : Statistiques du jour
- **Vue hebdomadaire** : Comparaison sur 7 jours
- **Vue mensuelle** : Tendances sur 30 jours
- **Comparaison multi-établissements** : Pour les plans Pro/Enterprise

### 5. Gestion des abonnements

#### Plans disponibles

| Plan | Prix/mois | Établissements | SMS inclus | Fonctionnalités |
|------|-----------|----------------|------------|-----------------|
| **Basic** | 19€ | 1 | 1000 | Analytics basiques, Support email |
| **Pro** | 49€ | 5 | 2500 | Analytics avancés, Support prioritaire, API access, Branding personnalisé |
| **Enterprise** | 99€ | Illimité | 5000 | Analytics avancés, Support téléphone, API access, Branding personnalisé, Gestionnaire dédié |

#### Période d'essai

- **Durée** : 14 jours gratuits
- **Fonctionnalités** : Accès complet à toutes les fonctionnalités
- **Limite** : 1 établissement maximum pendant l'essai
- **Conversion** : Passage automatique à l'abonnement payant après l'essai

#### Facturation

- **Période** : Mensuelle (du 1er au dernier jour du mois)
- **Calcul** : Prix de base + SMS supplémentaires (0,03€/SMS)
- **Détail** : Répartition de la consommation SMS par établissement
- **Paiement** : Stripe (carte bancaire)
- **Facture** : Génération automatique via Stripe

---

## Base de données

### Architecture PostgreSQL

La base de données utilise **PostgreSQL 15+** avec les extensions suivantes :

- **`uuid-ossp`** : Génération d'identifiants UUID
- **`pg_trgm`** : Expressions régulières avancées pour la recherche

### Tables principales

#### 1. `users` - Comptes utilisateurs

**Description** : Représente les comptes utilisateurs de la plateforme. Stocke uniquement les informations personnelles et d'authentification.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `google_id` : Identifiant Google OAuth (optionnel)
- `email` : Adresse email unique (identifiant de connexion)
- `password` : Hash bcrypt (peut être NULL pour OAuth)
- `first_name`, `last_name` : Nom et prénom
- `phone_number` : Numéro de téléphone
- `profile_picture` : URL de la photo de profil
- `is_active` : Compte actif/suspendu
- `auth_provider` : 'google' ou 'facebook'
- `subscription_status` : 'trial', 'active', 'suspended', 'cancelled'
- `SubscriptionPlanId` : Référence vers `subscription_plans`
- `trial_ends_at` : Date de fin de l'essai gratuit
- `created_at`, `updated_at`, `last_login` : Timestamps

**Contraintes** :

- Email unique et format validé
- Numéro de téléphone format français (`+33` ou `0`)
- Statut d'abonnement dans la liste autorisée

#### 2. `businesses` - Établissements

**Description** : Représente chaque établissement géré par un utilisateur. Contient tous les paramètres opérationnels.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `UserId` : Référence vers `users` (propriétaire)
- `name` : Nom commercial
- `business_type` : Type d'activité (30+ types supportés)
- `phone_number`, `address`, `city`, `zip_code`, `country` : Coordonnées
- `qr_code_token` : Token unique pour le QR Code (UNIQUE)
- `average_service_time` : Temps moyen en secondes
- `is_queue_active` : File ouverte/fermée
- `is_queue_paused` : File en pause
- `max_queue_size` : Limite de clients (1-200)
- `opening_hours` : Horaires au format JSONB
- `custom_message` : Message personnalisé pour SMS
- `sms_notifications_enabled` : Activation SMS
- `auto_advance_enabled` : Passage automatique
- `client_timeout_minutes` : Délai avant timeout (1-30 min)
- `is_active` : Établissement actif/inactif
- `created_at`, `updated_at` : Timestamps

**Format `opening_hours` (JSONB)** :

```json
{
  "monday": {"open": "08:00", "close": "18:00", "closed": false},
  "tuesday": {"open": "08:00", "close": "18:00", "closed": false},
  "wednesday": {"open": "08:00", "close": "12:00", "closed": false},
  "thursday": {"open": "08:00", "close": "18:00", "closed": false},
  "friday": {"open": "08:00", "close": "18:00", "closed": false},
  "saturday": {"open": "08:00", "close": "17:00", "closed": false},
  "sunday": {"closed": true}
}
```

#### 3. `queue_entries` - Entrées de file d'attente

**Description** : Gère les inscriptions dans les files d'attente. Cœur opérationnel du système.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `BusinessId` : Référence vers `businesses`
- `phone` : Numéro de téléphone du client (format français)
- `client_name` : Nom ou prénom (optionnel)
- `position` : Rang dans la file (recalculé automatiquement)
- `estimated_wait_time` : Temps d'attente estimé en minutes
- `status` : 'waiting', 'called', 'served', 'missed', 'cancelled'
- `called_at` : Timestamp de l'appel
- `served_at` : Timestamp du service
- `actual_service_time` : Durée réelle en secondes
- `sms_sent_count` : Nombre de SMS envoyés
- `last_sms_sent_at` : Dernier SMS envoyé
- `created_at`, `updated_at` : Timestamps

**Index optimisés** :

- `idx_queue_entries_business_status` : Pour filtrer par établissement et statut
- `idx_queue_entries_active_position` : Pour les clients en attente uniquement
- `idx_queue_entries_waiting_by_business` : Pour le tri par position

#### 4. `subscription_plans` - Plans d'abonnement

**Description** : Définit les différents plans tarifaires avec leurs limites.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `name` : Nom unique ('basic', 'pro', 'enterprise')
- `price_cents` : Prix mensuel en centimes
- `max_businesses` : Nombre maximum d'établissements (-1 = illimité)
- `sms_quota_monthly` : Quota SMS inclus
- `features` : Fonctionnalités au format JSONB
- `is_active` : Plan proposable aux nouveaux clients
- `created_at`, `updated_at` : Timestamps

#### 5. `sms_logs` - Journal des SMS

**Description** : Journal exhaustif de tous les SMS envoyés. Essentiel pour la facturation et l'audit.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `BusinessId` : Référence vers `businesses`
- `QueueEntryId` : Référence vers `queue_entries` (optionnel)
- `phone` : Numéro destinataire
- `message_type` : 'confirmation', 'reminder', 'your_turn', 'missed', 'cancelled'
- `message_content` : Texte exact envoyé
- `status` : 'pending', 'sent', 'delivered', 'failed'
- `provider_response` : Réponse JSON de l'API SMS
- `cost_cents` : Coût unitaire (3 centimes)
- `sent_at`, `delivered_at` : Timestamps

#### 6. `analytics_daily` - Statistiques quotidiennes

**Description** : Métriques quotidiennes par établissement pour les tableaux de bord.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `BusinessId` : Référence vers `businesses`
- `date` : Date des statistiques (UNIQUE par établissement)
- `total_clients_served` : Clients servis
- `total_clients_missed` : Clients manqués
- `total_clients_cancelled` : Clients annulés
- `total_clients_registered` : Total d'inscriptions
- `average_wait_time` : Temps d'attente moyen (minutes)
- `average_service_time` : Temps de service moyen (secondes)
- `peak_hour` : Heure de pointe (0-23)
- `peak_queue_size` : Taille maximale de file
- `abandonment_rate` : Taux d'abandon (%)
- `sms_sent_count` : Nombre de SMS envoyés
- `revenue_potential_lost` : Estimation manque à gagner (centimes)
- `busiest_time_start`, `busiest_time_end` : Période la plus chargée
- `created_at` : Timestamp de génération

#### 7. `billings` - Facturation

**Description** : Facturation consolidée par utilisateur incluant tous ses établissements.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `UserId` : Référence vers `users`
- `SubscriptionPlanId` : Référence vers `subscription_plans`
- `billing_period_start`, `billing_period_end` : Période de facturation
- `base_price_cents` : Prix de base de l'abonnement
- `active_businesses_count` : Nombre d'établissements actifs
- `sms_included` : Quota SMS inclus
- `sms_used` : SMS consommés
- `sms_overage` : SMS dépassant le quota
- `sms_overage_cost_cents` : Coût des SMS supplémentaires
- `sms_usage_by_business` : Détail JSON par établissement
- `total_amount_cents` : Montant total de la facture
- `status` : 'pending', 'paid', 'failed', 'refunded', 'cancelled'
- `stripe_invoice_id`, `stripe_payment_intent_id` : Références Stripe
- `paid_at` : Timestamp de paiement
- `due_date` : Date limite de paiement
- `created_at` : Timestamp de génération

#### 8. `system_configs` - Configuration système

**Description** : Configuration centralisée pour les paramètres globaux.

**Colonnes principales** :

- `id` : UUID (clé primaire)
- `key` : Clé unique de configuration
- `value` : Valeur (stockée en texte)
- `data_type` : 'string', 'integer', 'decimal', 'boolean', 'json'
- `description` : Description du paramètre
- `is_public` : Accessible via l'API publique
- `updated_at` : Timestamp de modification

### Triggers PostgreSQL

#### 1. Recalcul automatique des positions

**Fonction** : `recalculate_queue_positions()`

**Déclencheurs** :

- **INSERT** : Après insertion d'un nouveau client
- **UPDATE** : Après changement de statut
- **DELETE** : Après suppression d'une entrée

**Logique** :

```sql
UPDATE queue_entries
SET position = new_position
FROM (
    SELECT 
        id, 
        ROW_NUMBER() OVER (ORDER BY created_at ASC) as new_position
    FROM queue_entries
    WHERE BusinessId = COALESCE(NEW.BusinessId, OLD.BusinessId)
      AND status = 'waiting'
) AS subquery
WHERE queue_entries.id = subquery.id;
```

#### 2. Mise à jour automatique des timestamps

**Fonction** : `update_updated_at_column()`

**Application** : Sur toutes les tables avec colonne `updated_at`

#### 3. Validation des limites par plan

**Fonction** : `check_business_limit()`

**Déclencheur** : **BEFORE INSERT** sur `businesses`

**Logique** : Vérifie que l'utilisateur n'a pas dépassé la limite d'établissements selon son plan

#### 4. Validation du changement de plan

**Fonction** : `validate_business_count_on_plan_change()`

**Déclencheur** : **BEFORE UPDATE** sur `users.SubscriptionPlanId`

**Logique** : Empêche la rétrogradation si l'utilisateur a trop d'établissements

### Row Level Security (RLS)

**Principe** : Chaque utilisateur ne peut accéder qu'à ses propres données.

**Mécanisme** :

1. **Variable de session** : `app.current_user_id` définie avant chaque requête
2. **Politiques RLS** : Appliquées automatiquement par PostgreSQL
3. **Isolation garantie** : Impossible d'accéder aux données d'un autre utilisateur

**Exemple de politique** :

```sql
CREATE POLICY "Users manage own businesses" ON businesses
    FOR ALL USING (UserId = current_setting('app.current_user_id')::UUID);
```

**Accès public** : Les clients peuvent consulter les files via le token QR Code uniquement.

---

## Système de files d'attente

### Architecture du système

Le système de files d'attente est un **mixte PostgreSQL + TypeScript** :

- **PostgreSQL** : Recalcul automatique des positions via triggers
- **TypeScript** : Validation métier, calcul des temps d'attente, gestion des erreurs

### Calcul du temps d'attente

**Formule** :

```text
estimated_wait_minutes = (clients_ahead × average_service_time) / 60
```

**Variables** :

- `clients_ahead` : Nombre de clients avec `status='waiting'` ET `created_at < current_client.created_at`
- `average_service_time` : Depuis `businesses.average_service_time` (en secondes)

**Exemple** :

```text
Coiffeur : average_service_time = 2700s (45 min)
File actuelle : 2 clients en attente

Nouveau client :
→ Position = 3
→ Temps estimé = (2 × 2700) / 60 = 90 minutes
```

### Gestion des timeouts

**Mécanisme** : Job CRON ou background worker

**Fonctionnement** :

1. Vérifie toutes les minutes les clients avec `status = 'called'`
2. Si `called_at < NOW() - INTERVAL '5 minutes'` :
   - Passe le statut à `missed`
   - Envoie un SMS "Tour manqué"
   - Recalcule les positions (via trigger)
   - Appelle automatiquement le client suivant (si `auto_advance_enabled`)

### Flux complet : Exemple avec 3 clients

```text
T=10:00:00
├─ Alice scanne QR Code
├─ POST /queues/join { phone: "+33612345678", name: "Alice" }
├─ API calcule : position = 1, temps = 0 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice position = 1

T=10:01:30
├─ Bob scanne QR Code
├─ POST /queues/join { phone: "+33698765432", name: "Bob" }
├─ API calcule : position = 2, temps = 2 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice=1, Bob=2

T=10:02:15
├─ Charlie scanne QR Code
├─ POST /queues/join { phone: "+33687654321", name: "Charlie" }
├─ API calcule : position = 3, temps = 4 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice=1, Bob=2, Charlie=3

T=10:05:00
├─ Commerçant appelle Alice
├─ PATCH /queues/{alice_id}/status { status: "called" }
├─ SMS envoyé à Alice : "C'est votre tour !"
└─ Trigger recalcule : Bob=1, Charlie=2

T=10:07:00
├─ Commerçant confirme service Alice
├─ PATCH /queues/{alice_id}/served
└─ Trigger recalcule : Bob=1, Charlie=2 (inchangé)

État final :
| Client  | Status  | Position |
|---------|---------|----------|
| Alice   | served  | ❌       |
| Bob     | waiting | 1        |
| Charlie | waiting | 2        |
```

### Optimisations

#### Index partiels

```sql
-- Accélère les requêtes sur les clients en attente uniquement
CREATE INDEX idx_waiting_only 
    ON queue_entries(BusinessId, position) 
    WHERE status = 'waiting';
```

#### Vue matérialisée pour statistiques

```sql
CREATE MATERIALIZED VIEW queue_stats AS
SELECT 
    BusinessId,
    COUNT(*) FILTER (WHERE status = 'waiting') as waiting_count,
    AVG(estimated_wait_time) FILTER (WHERE status = 'waiting') as avg_wait,
    MAX(position) as max_position
FROM queue_entries
GROUP BY BusinessId;

-- Rafraîchir toutes les 5 minutes
REFRESH MATERIALIZED VIEW queue_stats;
```

---

## Authentification et sécurité

### Authentification JWT

**Mécanisme** : **JSON Web Token (JWT)** avec algorithme **RS256**

**Flux de connexion** :

1. **POST /auth/login** : Email + mot de passe
2. **Validation** : Vérification des identifiants en base
3. **Génération** : Création d'un token JWT signé
4. **Réponse** : Token retourné au client
5. **Utilisation** : Token inclus dans le header `Authorization: Bearer {token}`

**Contenu du token** :

- `userId` : Identifiant de l'utilisateur
- `email` : Email de l'utilisateur
- `iat` : Date d'émission
- `exp` : Date d'expiration (24h par défaut)

### OAuth (Google/Facebook)

**Support** : Authentification via Google OAuth 2.0 et Facebook

**Flux** :

1. Redirection vers le fournisseur OAuth
2. Autorisation utilisateur
3. Récupération des informations (email, nom, photo)
4. Création ou connexion du compte
5. Génération d'un token JWT

**Note** : Pour OAuth, le champ `password` reste `NULL` dans la table `users`.

### Middleware d'authentification

**Fichier** : `src/auth/middlewares/authMiddleware.ts`

**Fonction** : `authMiddleware`

**Actions** :

1. Extraction du token depuis le header `Authorization`
2. Vérification de la signature JWT
3. Validation de l'expiration
4. Injection de l'utilisateur dans `req.user`
5. Passage au middleware suivant ou erreur 401

### Middleware de vérification de propriété

**Fonction** : `checkBusinessOwnership`

**Actions** :

1. Récupération de l'ID de l'établissement depuis les paramètres
2. Vérification en base que `businesses.UserId = req.user.id`
3. Erreur 403 si l'utilisateur n'est pas propriétaire

### Sécurité au niveau des données (RLS)

**Principe** : Isolation des données au niveau de la base de données

**Mécanisme** :

1. **Variable de session** : `SET app.current_user_id = 'uuid'` avant chaque requête
2. **Politiques RLS** : Appliquées automatiquement par PostgreSQL
3. **Isolation garantie** : Impossible d'accéder aux données d'un autre utilisateur

**Tables protégées** :

- `users` : Accès uniquement à son propre compte
- `businesses` : Accès uniquement à ses propres établissements
- `queue_entries` : Accès via les établissements
- `sms_logs` : Accès via les établissements
- `analytics_daily` : Accès via les établissements
- `billings` : Accès uniquement à ses propres factures

### Validation des données

**Numéros de téléphone** :

- Format français : `+33` ou `0` suivi de 9 chiffres
- Validation via regex : `^(\+33|0)[1-9][0-9]{8}$`
- Normalisation avant stockage

**Emails** :

- Format standard : `^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$`
- Unicité garantie en base

**Mots de passe** :

- Minimum 8 caractères
- Au moins une majuscule et un chiffre
- Hash avec **bcrypt** ou **argon2**

### Protection CSRF et XSS

- **Headers de sécurité** : CORS configuré
- **Rate limiting** : Limitation des requêtes par IP
- **Validation d'entrée** : Nettoyage de tous les inputs
- **Échappement** : Protection contre les injections SQL

---

## Gestion des abonnements

### Plans d'abonnement

#### Plan Basic (19€/mois)

- **Établissements** : 1 maximum
- **SMS inclus** : 1000/mois
- **Analytics** : Basiques
- **Support** : Email
- **API** : Non

#### Plan Pro (49€/mois)

- **Établissements** : 5 maximum
- **SMS inclus** : 2500/mois
- **Analytics** : Avancés
- **Support** : Prioritaire
- **API** : Oui
- **Branding** : Personnalisé

#### Plan Enterprise (99€/mois)

- **Établissements** : Illimité
- **SMS inclus** : 5000/mois
- **Analytics** : Avancés
- **Support** : Téléphone
- **API** : Oui
- **Branding** : Personnalisé
- **Gestionnaire** : Dédié

### Essai gratuit

- **Durée** : 14 jours gratuits
- **Fonctionnalités** : Accès complet
- **Limite** : 1 établissement
- **Conversion** : Automatique après l'essai

### Calcul et génération de factures

#### Calcul mensuel

```text
total_amount = base_price + (sms_overage × 0.03)
```

**Détail** :

- `base_price` : Prix du plan d'abonnement
- `sms_overage` : SMS dépassant le quota inclus
- `0.03` : Coût unitaire en euros (3 centimes)

#### Génération de facture

1. **Fin de période** : Le dernier jour du mois
2. **Calcul** : Consommation SMS de tous les établissements
3. **Détail** : Répartition par établissement (JSON)
4. **Création** : Enregistrement dans `billings`
5. **Stripe** : Génération de la facture Stripe
6. **Paiement** : Prélèvement automatique
7. **Notification** : Email avec la facture

#### Gestion des impayés

- **Tentative** : 3 tentatives de paiement
- **Suspension** : Compte suspendu après échec
- **Notification** : Emails d'alerte
- **Réactivation** : Automatique après paiement réussi

---

## API et endpoints

### Structure des routes

L'API est organisée en **4 modules principaux** :

1. **Authentification** : `/auth`
2. **Utilisateurs** : `/users`
3. **Établissements** : `/businesses`
4. **Files d'attente** : `/queues`

### Routes d'authentification

#### POST `/auth/register`

**Description** : Inscription d'un nouvel utilisateur

**Body** :

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "first_name": "Jean",
  "last_name": "Dupont"
}
```

**Réponse** :

```json
{
  "message": "Utilisateur créé avec succès",
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "first_name": "Jean",
    "last_name": "Dupont"
  }
}
```

#### POST `/auth/login`

**Description** : Connexion d'un utilisateur

**Body** :

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

**Réponse** :

```json
{
  "message": "Connexion réussie",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "User": {
    "id": "uuid",
    "email": "user@example.com"
  }
}
```

#### GET `/auth/protected`

**Description** : Route de test pour vérifier l'authentification

**Headers** : `Authorization: Bearer {token}`

**Réponse** : 200 OK si authentifié

### Routes des établissements

#### POST `/businesses`

**Description** : Création d'un nouvel établissement

**Headers** : `Authorization: Bearer {token}`

**Body** :

```json
{
  "name": "Boulangerie Martin",
  "businessType": "bakery",
  "phoneNumber": "+33612345678",
  "address": "123 Rue de la République",
  "city": "Paris",
  "zipCode": "75001",
  "country": "France"
}
```

**Réponse** :

```json
{
  "Business": {
    "id": "uuid",
    "name": "Boulangerie Martin",
    "qrCodeToken": "uuid",
    ...
  },
  "QRCode": "data:image/png;base64,..."
}
```

#### GET `/businesses/:id`

**Description** : Récupération d'un établissement

**Headers** : `Authorization: Bearer {token}`

**Réponse** :

```json
{
  "Business": {
    "id": "uuid",
    "name": "Boulangerie Martin",
    "isQueueActive": true,
    ...
  }
}
```

#### PATCH `/businesses/:id`

**Description** : Mise à jour d'un établissement

**Headers** : `Authorization: Bearer {token}`

**Body** : Champs à modifier (partiels)

**Réponse** :

```json
{
  "message": "Établissement mis à jour avec succès",
  "Business": { ... }
}
```

#### DELETE `/businesses/:id`

**Description** : Suppression d'un établissement

**Headers** : `Authorization: Bearer {token}`

**Réponse** : 204 No Content

#### GET `/users/:id/businesses`

**Description** : Liste de tous les établissements d'un utilisateur

**Headers** : `Authorization: Bearer {token}`

**Réponse** :

```json
{
  "businesses": [
    {
      "id": "uuid",
      "name": "Boulangerie Martin",
      ...
    }
  ]
}
```

### Routes des files d'attente

#### POST `/queues/join`

**Description** : Rejoindre une file d'attente (client)

**Body** :

```json
{
  "id": "business-uuid",
  "phone": "+33612345678",
  "clientName": "Alice"
}
```

**Réponse** :

```json
{
  "message": "Vous avez été ajouté à la file d'attente avec succès",
  "Entry": {
    "id": "uuid",
    "position": 3,
    "estimatedWaitTime": 12,
    "status": "waiting"
  }
}
```

#### GET `/queues/:entryId/status`

**Description** : Consulter sa position dans la file

**Réponse** :

```json
{
  "position": 2,
  "estimatedWaitTime": 8,
  "status": "waiting"
}
```

#### DELETE `/queues/:entryId/cancel`

**Description** : Annuler sa place dans la file

**Réponse** : 204 No Content

#### PATCH `/businesses/:id/status`

**Description** : Activer/désactiver la file d'attente (commerçant)

**Headers** : `Authorization: Bearer {token}`

**Body** :

```json
{
  "isQueueActive": true
}
```

#### POST `/queues/:businessId/next`

**Description** : Appeler le client suivant (commerçant)

**Headers** : `Authorization: Bearer {token}`

**Réponse** :

```json
{
  "message": "Client appelé avec succès",
  "Client": {
    "id": "uuid",
    "phone": "+33612345678",
    "clientName": "Alice",
    "position": 1
  }
}
```

#### PATCH `/queues/:entryId/served`

**Description** : Marquer un client comme servi (commerçant)

**Headers** : `Authorization: Bearer {token}`

**Réponse** :

```json
{
  "message": "Client marqué comme servi avec succès"
}
```

### Codes de statut HTTP

- **200 OK** : Requête réussie
- **201 Created** : Ressource créée
- **204 No Content** : Suppression réussie
- **400 Bad Request** : Requête invalide
- **401 Unauthorized** : Non authentifié
- **403 Forbidden** : Accès refusé
- **404 Not Found** : Ressource introuvable
- **500 Internal Server Error** : Erreur serveur

---

## Technologies utilisées

### Backend

#### Node.js

- **Version** : > 20.0
- **Rôle** : Runtime JavaScript côté serveur
- **Avantages** : Asynchrone, écosystème riche, performance

#### TypeScript

- **Version** : 5.x
- **Rôle** : Langage de programmation typé
- **Avantages** : Typage statique, détection d'erreurs, meilleure maintenabilité

#### Express.js

- **Version** : 5.1.0
- **Rôle** : Framework web minimaliste
- **Avantages** : Léger, flexible, middleware system

### Système de base de données

#### PostgreSQL

- **Version** : 15+
- **Rôle** : Base de données relationnelle
- **Fonctionnalités utilisées** :
  - **UUID** : Identifiants uniques
  - **JSONB** : Stockage de données structurées
  - **Triggers** : Automatisation des calculs
  - **Row Level Security** : Isolation des données
  - **Index partiels** : Optimisation des performances

### Authentification

#### JWT (JSON Web Token)

- **Algorithme** : RS256
- **Rôle** : Tokens d'authentification stateless
- **Avantages** : Pas de session serveur, scalable

#### OAuth 2.0

- **Fournisseurs** : Google, Facebook
- **Rôle** : Authentification sociale
- **Avantages** : Expérience utilisateur simplifiée

### Paiements

#### Stripe

- **Version** : API v2023
- **Rôle** : Gestion des paiements et factures
- **Fonctionnalités** :
  - Paiements récurrents
  - Génération de factures
  - Gestion des abonnements
  - Webhooks pour les événements

### Notifications

#### AWS SNS (Simple Notification Service)

- **Rôle** : Envoi de SMS
- **Avantages** : Scalable, fiable, coût optimisé

### Sécurité

#### bcrypt / argon2

- **Rôle** : Hashage des mots de passe
- **Avantages** : Résistant aux attaques par force brute

### Développement

#### ESLint

- **Rôle** : Linter pour TypeScript
- **Avantages** : Qualité de code, cohérence

#### Jest

- **Rôle** : Framework de tests
- **Avantages** : Tests unitaires et d'intégration

#### Nodemon

- **Rôle** : Redémarrage automatique en développement
- **Avantages** : Productivité accrue

---

## Organisation du code

### Structure des dossiers

```text
waitify-api/
├── src/
│   ├── auth/              # Authentification
│   │   ├── controllers/  # Logique métier
│   │   ├── middlewares/  # Middlewares d'auth
│   │   ├── models/       # Modèles TypeScript
│   │   └── routes/       # Définition des routes
│   │
│   ├── businesses/       # Gestion des établissements
│   │   ├── controllers/
│   │   ├── models/
│   │   └── routes/
│   │
│   ├── queues/           # Files d'attente
│   │   ├── controllers/
│   │   ├── models/
│   │   └── routes/
│   │
│   ├── users/            # Gestion des utilisateurs
│   │   ├── controllers/
│   │   ├── models/
│   │   └── routes/
│   │
│   ├── config/           # Configuration
│   │   ├── constants.ts  # Constantes de l'application
│   │   ├── database.ts   # Connexion PostgreSQL
│   │   └── envVariables.ts # Variables d'environnement
│   │
│   ├── server.ts         # Point d'entrée de l'application
│   └── tests/            # Tests unitaires
│
├── dist/                 # Code compilé JavaScript
├── documentation/        # Documentation technique
├── package.json          # Dépendances npm
├── tsconfig.json         # Configuration TypeScript
└── README.md             # Documentation utilisateur
```

### Patterns utilisés

#### MVC (Model-View-Controller)

- **Models** : Définition des structures de données TypeScript
- **Controllers** : Logique métier et gestion des requêtes
- **Routes** : Définition des endpoints et middleware

#### Separation of Concerns

- Chaque module est indépendant (auth, businesses, queues, users)
- Logique métier isolée dans les controllers
- Validation centralisée dans les middlewares

#### Constants Pattern

- Toutes les constantes dans `src/config/constants.ts`
- Messages d'erreur centralisés
- Routes définies une seule fois

### Conventions de nommage

- **Fichiers** : camelCase (ex: `authControllers.ts`)
- **Classes** : PascalCase (ex: `BusinessController`)
- **Fonctions** : camelCase (ex: `getBusinessById`)
- **Constantes** : UPPER_SNAKE_CASE (ex: `HTTP_STATUS`)
- **Interfaces** : PascalCase (ex: `Business`)

---

## Déploiement et infrastructure

### Environnements de déploiement

#### Environnement de développement

- **Local** : Node.js + PostgreSQL local
- **Scripts** : `npm run dev` (watch mode)
- **Port** : Configurable via `.env`

#### Pré-production

- **Branche** : `pre-prod`
- **Usage** : Démonstrations clients
- **Stabilité** : Code validé depuis `dev`

#### Production

- **Branche** : `prod`
- **Infrastructure** : AWS
- **Stabilité** : Code entièrement testé

### Infrastructure AWS

#### RDS (Relational Database Service)

- **Service** : PostgreSQL managé
- **Avantages** : Sauvegardes automatiques, haute disponibilité
- **Configuration** : Multi-AZ pour la redondance

#### ECS (Elastic Container Service)

- **Service** : Conteneurs Docker
- **Avantages** : Scalabilité automatique
- **Configuration** : Load balancer, auto-scaling

#### Lambda

- **Service** : Fonctions serverless
- **Usage** : Jobs CRON (timeout clients, facturation)
- **Avantages** : Coût optimisé, déclenchement automatique

#### SNS (Simple Notification Service)

- **Service** : Envoi de SMS
- **Configuration** : Intégration avec opérateurs français
- **Coût** : ~0,03€ par SMS

### Docker

#### Dockerfile

Le projet inclut un `Dockerfile` pour la containerisation :

```dockerfile
FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
EXPOSE 3000
CMD ["npm", "start"]
```

### Variables d'environnement

Fichier `.env` (non versionné) :

```env
# Server
SERVER_PORT=3000
NODE_ENV=production

# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_USER=waitify
DATABASE_PASSWORD=secret
DATABASE_NAME=waitify_db

# JWT
JWT_SECRET=your-secret-key
JWT_EXPIRES_IN=24h

# Stripe
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...

# AWS
AWS_REGION=eu-west-1
AWS_SNS_TOPIC_ARN=arn:aws:sns:...
```

### Monitoring

- **Logs** : CloudWatch (AWS)
- **Métriques** : Performance, erreurs, latence
- **Alertes** : Notifications en cas d'erreur critique

---

## Conclusion

Waitify est une **solution SaaS complète** de gestion de files d'attente virtuelles, conçue pour être **scalable**, **sécurisée** et **facile à utiliser**. L'architecture mixte **PostgreSQL + TypeScript** garantit à la fois l'intégrité des données et la flexibilité du développement.

### Points clés

- **Architecture robuste** : Triggers PostgreSQL + API REST
- **Sécurité renforcée** : RLS, JWT, validation stricte
- **Scalabilité** : Multi-établissements, multi-utilisateurs
- **Expérience utilisateur** : QR Code simple, notifications SMS
- **Monétisation** : Plans flexibles, facturation automatique

### Évolutions futures

- **WebSockets** : Notifications en temps réel
- **Application mobile** : iOS et Android
- **API publique** : Intégrations tierces
- **Analytics avancés** : Machine learning pour prédictions
- **Multi-langues** : Support international

---

**Documentation maintenue par** : Steven YAMBOS  
**Contact** : [LinkedIn](https://www.linkedin.com/in/steven-yambos/)  
**Dernière mise à jour** : 30 octobre 2025
