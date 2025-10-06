# Base de données

**Mise à jour :** 06-10-2025

**Par :** [Steven YAMBOS](https://www.linkedin.com/in/steven-yambos/)

[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)

## Bonnes pratiques

- Les tables sont au pluriel et en minuscule (exemple : `users`)
- Les champs avec des références ont une majuscule et se terminent par `Id`
- Utilisation des UUID comme clés primaires pour éviter les collisions
- Contraintes de clés étrangères avec CASCADE pour maintenir l'intégrité
- Tous les timestamps incluent la timezone (TIMESTAMP WITH TIME ZONE)
- Utilisation de JSONB pour les données structurées variables
- Index optimisés pour les requêtes fréquentes
- Row Level Security (RLS) activé pour la sécurité multi-tenant
- Validation des données via contraintes CHECK
- Triggers automatiques pour la gestion des timestamps
- Convention de nommage cohérente pour tous les objets
- Architecture multi-business supportée nativement

## Configuration PostgreSQL requise

### Extensions nécessaires

```sql
-- Extension UUID pour génération d'identifiants
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Extension pour expressions régulières avancées
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
```

### Variables de session pour RLS

```sql
-- Dans votre API, définir avant chaque requête
SET app.current_user_id = 'uuid-of-authenticated-user';
```

## Architecture multi-business

L'architecture permet à un utilisateur de gérer plusieurs établissements via des plans tarifaires adaptés. La séparation entre `users` (compte utilisateur) et `businesses` (établissements) garantit une évolutivité maximale.

### Relation utilisateur-business

- **1 utilisateur → N businesses** (selon le plan d'abonnement)
- **Plan Basic** : 1 business maximum
- **Plan Pro** : 5 businesses maximum
- **Plan Enterprise** : Illimité

## Tables principales

### Table `users`

**Description :** Représente les comptes utilisateurs de la plateforme Waitify. Cette table stocke uniquement les informations personnelles et d'authentification. Les détails des établissements sont déportés dans la table `businesses` pour supporter le multi-établissement.

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    google_id VARCHAR(255),
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone_number VARCHAR(20),
    profile_picture VARCHAR(255),
    is_active BOOLEAN DEFAULT true,
    auth_provider VARCHAR(50) DEFAULT 'google',
    subscription_status VARCHAR(50) DEFAULT 'trial',
    SubscriptionPlanId UUID REFERENCES subscription_plans(id),
    trial_ends_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    last_login TIMESTAMP WITH TIME ZONE
);

-- Index pour les performances
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_subscription_plan ON users(SubscriptionPlanId);
CREATE INDEX idx_users_subscription_status ON users(subscription_status);
CREATE INDEX idx_users_active ON users(is_active) WHERE is_active = true;

-- Contraintes de validation
ALTER TABLE users ADD CONSTRAINT check_email_format CHECK (email ~* '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');
ALTER TABLE users ADD CONSTRAINT check_subscription_status CHECK (subscription_status IN ('trial', 'active', 'suspended', 'cancelled'));
ALTER TABLE users ADD CONSTRAINT check_auth_provider CHECK (auth_provider IN ('google', 'facebook'));
ALTER TABLE users ADD CONSTRAINT check_phone_number_format CHECK (phone_number IS NULL OR phone_number ~ '^(\+33|0)[1-9][0-9]{8}$');
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `google_id` : Identifiant unique partagé par Google lors de l'inscription avec Google oAuth2
- `email` : Adresse email unique servant d'identifiant de connexion
- `password` : Hash bcrypt du mot de passe, jamais stocké en clair. ⚠️ Le mot de passe n'est pas `NOT NULL` car avec l'inscription avec Google on ne récupère pas le mot de passe de l'utilisateur ⚠️
- `first_name` : Prénom de l'utilisateur
- `last_name` : Nom de famille de l'utilisateur
- `phone_number` : Numéro de téléphone de contact
- `profile_picture` : Image de profile
- `is_active` : Permet de suspendre un compte utilisateur globalement
- `auth_provider` : Application de connexion
- `subscription_status` : État global de l'abonnement utilisateur
- `SubscriptionPlanId` : Référence vers le plan d'abonnement actuel
- `trial_ends_at` : Date limite de la période d'essai gratuite de 14 jours
- `created_at` : Timestamp de création du compte
- `updated_at` : Timestamp de dernière modification
- `last_login` : Timestamp de dernière connexion

### Table `businesses`

**Description :** Représente chaque établissement géré par un utilisateur. Cette table contient tous les paramètres opérationnels spécifiques à chaque point de vente : configuration de la file d'attente, horaires, messages personnalisés.

```sql
CREATE TABLE businesses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    business_type VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20),
    address TEXT,
    city VARCHAR(100),
    zip_code VARCHAR(10),
    country VARCHAR(50) DEFAULT 'France',
    qr_code_token VARCHAR(255) UNIQUE NOT NULL,
    average_service_time INTEGER DEFAULT 300,
    is_queue_active BOOLEAN DEFAULT false,
    is_queue_paused BOOLEAN DEFAULT false,
    max_queue_size INTEGER DEFAULT 50,
    opening_hours JSONB,
    custom_message TEXT,
    sms_notifications_enabled BOOLEAN DEFAULT true,
    auto_advance_enabled BOOLEAN DEFAULT true,
    client_timeout_minutes INTEGER DEFAULT 5,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les performances multi-business
CREATE INDEX idx_businesses_user ON businesses(UserId);
CREATE INDEX idx_businesses_user_active ON businesses(UserId, is_active);
CREATE UNIQUE INDEX idx_businesses_qr_token ON businesses(qr_code_token);
CREATE INDEX idx_businesses_type ON businesses(business_type);
CREATE INDEX idx_businesses_active_by_user ON businesses(UserId, created_at) WHERE is_active = true;

-- Contraintes de validation
ALTER TABLE businesses ADD CONSTRAINT check_business_type CHECK (business_type IN (
    'bakery', 'hairdresser', 'pharmacy', 'garage', 'restaurant',
    'medical_office', 'dentist', 'veterinary', 'optician', 'bank',
    'insurance', 'notary', 'lawyer', 'accountant', 'real_estate',
    'prefecture', 'city_hall', 'family_allowance', 'employment_agency', 'public_service',
    'post_office', 'dry_cleaning', 'cobbler', 'watchmaker', 'phone_repair',
    'beauty_salon', 'massage', 'tattoo', 'nail_salon', 'barber',
    'vehicle_inspection', 'gas_station', 'auto_body', 'tire_service',
    'other'
));
ALTER TABLE businesses ADD CONSTRAINT check_service_time_positive CHECK (average_service_time > 0);
ALTER TABLE businesses ADD CONSTRAINT check_max_queue_reasonable CHECK (max_queue_size BETWEEN 1 AND 200);
ALTER TABLE businesses ADD CONSTRAINT check_timeout_reasonable CHECK (client_timeout_minutes BETWEEN 1 AND 30);
ALTER TABLE businesses ADD CONSTRAINT check_phone_number_format_business CHECK (phone_number IS NULL OR phone_number ~ '^(\+33|0)[1-9][0-9]{8}$');
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `UserId` : Référence vers le propriétaire utilisateur de l'établissement
- `name` : Nom commercial de l'établissement (ex: "Boulangerie Martin Centre-Ville")
- `business_type` : Type d'activité utilisé pour les temps de service par défaut
- `phone_number` : Numéro de téléphone spécifique à cet établissement
- `address` : Adresse physique complète de l'établissement
- `city` : Ville où se situe l'établissement
- `zip_code` : Code postal de l'établissement
- `country` : Pays de l'établissement (par défaut France)
- `qr_code_token` : Token unique pour identifier l'établissement via QR code
- `average_service_time` : Temps moyen en secondes pour servir un client
- `is_queue_active` : Contrôle global de la file d'attente (ouverte/fermée)
- `is_queue_paused` : Pause temporaire sans fermer complètement
- `max_queue_size` : Limite du nombre de clients simultanés
- `opening_hours` : Horaires d'ouverture au format JSON par jour
- `custom_message` : Message personnalisé inclus dans les SMS aux clients
- `sms_notifications_enabled` : Active/désactive l'envoi de SMS pour cet établissement
- `auto_advance_enabled` : Active le passage automatique au client suivant après timeout
- `client_timeout_minutes` : Délai avant passage automatique au suivant
- `is_active` : Permet de désactiver temporairement un établissement
- `created_at` : Timestamp de création de l'établissement
- `updated_at` : Timestamp de dernière modification

**Format JSON pour opening_hours :**

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

### Table `queue_entries`

**Description :** Gère les inscriptions dans les files d'attente de chaque établissement. Cette table est le cœur opérationnel du système, stockant les positions, estimations de temps et le cycle de vie complet de chaque client.

```sql
CREATE TABLE queue_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES businesses(id) ON DELETE CASCADE,
    phone VARCHAR(20) NOT NULL,
    client_name VARCHAR(100),
    position INTEGER NOT NULL,
    estimated_wait_time INTEGER,
    status VARCHAR(50) DEFAULT 'waiting',
    called_at TIMESTAMP WITH TIME ZONE,
    served_at TIMESTAMP WITH TIME ZONE,
    actual_service_time INTEGER,
    sms_sent_count INTEGER DEFAULT 0,
    last_sms_sent_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index optimisés pour le multi-business
CREATE INDEX idx_queue_entries_business_status ON queue_entries(BusinessId, status);
CREATE INDEX idx_queue_entries_active_position ON queue_entries(BusinessId, position) WHERE status = 'waiting';
CREATE INDEX idx_queue_entries_business_created ON queue_entries(BusinessId, created_at);
CREATE INDEX idx_queue_entries_phone_business ON queue_entries(phone, BusinessId);
CREATE INDEX idx_queue_entries_waiting_by_business ON queue_entries(BusinessId, position, created_at) WHERE status = 'waiting';

-- Index pour requêtes cross-business (performance)
CREATE INDEX idx_queue_entries_user_status ON queue_entries(
    (SELECT UserId FROM businesses WHERE id = BusinessId),
    status,
    created_at
);

-- Contraintes de validation
ALTER TABLE queue_entries ADD CONSTRAINT check_position_positive CHECK (position > 0);
ALTER TABLE queue_entries ADD CONSTRAINT check_status_valid CHECK (status IN ('waiting', 'called', 'served', 'missed', 'cancelled'));
ALTER TABLE queue_entries ADD CONSTRAINT check_phone_format CHECK (phone ~ '^(\+33|0)[1-9][0-9]{8}$');
ALTER TABLE queue_entries ADD CONSTRAINT check_estimated_wait_positive CHECK (estimated_wait_time IS NULL OR estimated_wait_time >= 0);
ALTER TABLE queue_entries ADD CONSTRAINT check_called_before_served CHECK (called_at IS NULL OR served_at IS NULL OR served_at >= called_at);
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement concerné
- `phone` : Numéro de téléphone du client (format français validé)
- `client_name` : Nom ou prénom du client (optionnel)
- `position` : Rang dans la file d'attente, recalculé automatiquement
- `estimated_wait_time` : Temps d'attente estimé en minutes au moment de l'inscription
- `status` : État du client dans le processus (waiting/called/served/missed/cancelled)
- `called_at` : Timestamp précis de l'appel du client par le commerçant
- `served_at` : Timestamp de confirmation du service effectué
- `actual_service_time` : Durée réelle du service en secondes pour améliorer les estimations
- `sms_sent_count` : Nombre total de SMS envoyés à ce client pour le billing
- `last_sms_sent_at` : Timestamp du dernier SMS pour éviter le spam
- `created_at` : Timestamp d'inscription dans la file d'attente
- `updated_at` : Timestamp de dernière modification du statut

**Cycle de vie d'une entrée :**

1. `waiting` : Client inscrit, en attente de son tour
2. `called` : Commerçant a appelé le client (SMS envoyé)
3. `served` : Client servi avec succès
4. `missed` : Client absent lors de son appel (timeout)
5. `cancelled` : Client a annulé sa place manuellement

### Table `subscription_plans`

**Description :** Définit les différents plans tarifaires avec leurs limites et fonctionnalités. Cette table permet une gestion flexible des offres commerciales et une évolution tarifaire sans modification du code.

```sql
CREATE TABLE subscription_plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) UNIQUE NOT NULL,
    price_cents INTEGER NOT NULL,
    max_businesses INTEGER NOT NULL,
    sms_quota_monthly INTEGER DEFAULT 1000,
    features JSONB,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les requêtes fréquentes
CREATE INDEX idx_subscription_plans_active ON subscription_plans(is_active);
CREATE INDEX idx_subscription_plans_name ON subscription_plans(name);

-- Contraintes de validation
ALTER TABLE subscription_plans ADD CONSTRAINT check_price_positive CHECK (price_cents >= 0);
ALTER TABLE subscription_plans ADD CONSTRAINT check_max_businesses_valid CHECK (max_businesses = -1 OR max_businesses > 0);
ALTER TABLE subscription_plans ADD CONSTRAINT check_sms_quota_positive CHECK (sms_quota_monthly > 0);
```

**Plans par défaut :**

```sql
INSERT INTO subscription_plans (name, price_cents, max_businesses, sms_quota_monthly, features) VALUES
('basic', 1900, 1, 1000, '{"analytics": "basic", "support": "email", "api_access": false}'),
('pro', 4900, 5, 2500, '{"analytics": "advanced", "support": "priority", "api_access": true, "custom_branding": true}'),
('enterprise', 9900, -1, 5000, '{"analytics": "advanced", "support": "phone", "api_access": true, "custom_branding": true, "dedicated_manager": true}');
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `name` : Nom unique du plan affiché à l'utilisateur
- `price_cents` : Prix mensuel en centimes d'euro
- `max_businesses` : Nombre maximum d'établissements autorisés (-1 pour illimité)
- `sms_quota_monthly` : Quota de SMS inclus dans l'abonnement mensuel
- `features` : Fonctionnalités JSON incluses dans le plan
- `is_active` : Indique si le plan est proposable aux nouveaux clients
- `created_at` : Timestamp de création du plan
- `updated_at` : Timestamp de dernière modification

### Table `sms_logs`

**Description :** Journal exhaustif de tous les SMS envoyés par établissement. Essentiel pour la facturation multi-business, l'audit et le monitoring des performances par établissement.

```sql
CREATE TABLE sms_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES businesses(id) ON DELETE CASCADE,
    QueueEntryId UUID REFERENCES queue_entries(id) ON DELETE SET NULL,
    phone VARCHAR(20) NOT NULL,
    message_type VARCHAR(50) NOT NULL,
    message_content TEXT NOT NULL,
    status VARCHAR(50) DEFAULT 'pending',
    provider_response JSONB,
    cost_cents INTEGER DEFAULT 3,
    sent_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    delivered_at TIMESTAMP WITH TIME ZONE
);

-- Index pour l'analyse multi-business
CREATE INDEX idx_sms_logs_business_date ON sms_logs(BusinessId, sent_at);
CREATE INDEX idx_sms_logs_business_type ON sms_logs(BusinessId, message_type);
CREATE INDEX idx_sms_logs_user_period ON sms_logs((SELECT UserId FROM businesses WHERE id = BusinessId), sent_at);
CREATE INDEX idx_sms_logs_status ON sms_logs(status);

-- Contraintes de validation
ALTER TABLE sms_logs ADD CONSTRAINT check_message_type_valid CHECK (message_type IN ('confirmation', 'reminder', 'your_turn', 'missed', 'cancelled'));
ALTER TABLE sms_logs ADD CONSTRAINT check_sms_status_valid CHECK (status IN ('pending', 'sent', 'delivered', 'failed'));
ALTER TABLE sms_logs ADD CONSTRAINT check_cost_positive CHECK (cost_cents >= 0);
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement qui a envoyé le SMS
- `QueueEntryId` : Référence vers l'entrée de queue concernée (optionnel pour SMS génériques)
- `phone` : Numéro de téléphone destinataire du SMS
- `message_type` : Catégorie du SMS pour classifier les communications
- `message_content` : Texte exact envoyé, stocké pour audit et debugging
- `status` : État de livraison du SMS (pending/sent/delivered/failed)
- `provider_response` : Réponse JSON complète de l'API SMS pour troubleshooting
- `cost_cents` : Coût unitaire en centimes pour la facturation précise
- `sent_at` : Timestamp d'envoi du SMS
- `delivered_at` : Confirmation de livraison par l'opérateur (webhook)

**Types de messages SMS :**

- `confirmation` : "Votre place #3 chez [Business] est confirmée, temps d'attente: 12min"
- `reminder` : "Plus que 2 clients devant vous chez [Business]"
- `your_turn` : "C'est votre tour chez [Business] ! Présentez-vous au comptoir"
- `missed` : "Votre tour chez [Business] est passé. Rescannez le QR code"
- `cancelled` : "Votre place chez [Business] a été annulée"

### Table `analytics_daily`

**Description :** Métriques quotidiennes par établissement pour des tableaux de bord performants. Permet des comparaisons entre établissements d'un même utilisateur et des analyses de performance globales.

```sql
CREATE TABLE analytics_daily (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES businesses(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    total_clients_served INTEGER DEFAULT 0,
    total_clients_missed INTEGER DEFAULT 0,
    total_clients_cancelled INTEGER DEFAULT 0,
    total_clients_registered INTEGER DEFAULT 0,
    average_wait_time INTEGER,
    average_service_time INTEGER,
    peak_hour INTEGER,
    peak_queue_size INTEGER,
    abandonment_rate DECIMAL(5,2),
    sms_sent_count INTEGER DEFAULT 0,
    revenue_potential_lost INTEGER DEFAULT 0,
    busiest_time_start TIME,
    busiest_time_end TIME,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(BusinessId, date)
);

-- Index pour les analyses multi-business
CREATE INDEX idx_analytics_daily_business_date ON analytics_daily(BusinessId, date DESC);
CREATE INDEX idx_analytics_daily_user_date ON analytics_daily((SELECT UserId FROM businesses WHERE id = BusinessId), date);
CREATE INDEX idx_analytics_daily_date ON analytics_daily(date);

-- Contraintes de validation
ALTER TABLE analytics_daily ADD CONSTRAINT check_abandonment_rate_valid CHECK (abandonment_rate >= 0 AND abandonment_rate <= 100);
ALTER TABLE analytics_daily ADD CONSTRAINT check_peak_hour_valid CHECK (peak_hour IS NULL OR (peak_hour >= 0 AND peak_hour <= 23));
ALTER TABLE analytics_daily ADD CONSTRAINT check_totals_positive CHECK (
    total_clients_served >= 0 AND
    total_clients_missed >= 0 AND
    total_clients_cancelled >= 0 AND
    total_clients_registered >= 0
);
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement concerné par ces statistiques
- `date` : Date des statistiques (unique par établissement)
- `total_clients_served` : Nombre de clients effectivement servis dans la journée
- `total_clients_missed` : Nombre de clients qui ont manqué leur tour (timeout)
- `total_clients_cancelled` : Nombre de clients qui ont annulé leur place
- `total_clients_registered` : Nombre total d'inscriptions dans la journée
- `average_wait_time` : Temps d'attente moyen en minutes pour cette journée
- `average_service_time` : Temps de service moyen en secondes par client
- `peak_hour` : Heure (0-23) avec la plus longue file d'attente
- `peak_queue_size` : Taille maximum de la file atteinte dans la journée
- `abandonment_rate` : Pourcentage de clients ayant annulé ou manqué leur tour
- `sms_sent_count` : Nombre total de SMS envoyés dans la journée
- `revenue_potential_lost` : Estimation du manque à gagner des abandons en centimes
- `busiest_time_start` : Heure de début de la période la plus chargée
- `busiest_time_end` : Heure de fin de la période la plus chargée
- `created_at` : Timestamp de génération de ces statistiques

### Table `billings`

**Description :** Facturation consolidée par utilisateur incluant la consommation de tous ses établissements. Gère les abonnements multi-business avec détail de l'usage par établissement.

```sql
CREATE TABLE billings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    SubscriptionPlanId UUID NOT NULL REFERENCES subscription_plans(id),
    billing_period_start DATE NOT NULL,
    billing_period_end DATE NOT NULL,
    base_price_cents INTEGER NOT NULL,
    active_businesses_count INTEGER DEFAULT 1,
    sms_included INTEGER DEFAULT 1000,
    sms_used INTEGER DEFAULT 0,
    sms_overage INTEGER DEFAULT 0,
    sms_overage_cost_cents INTEGER DEFAULT 0,
    sms_usage_by_business JSONB,
    total_amount_cents INTEGER NOT NULL,
    status VARCHAR(50) DEFAULT 'pending',
    stripe_invoice_id VARCHAR(255),
    stripe_payment_intent_id VARCHAR(255),
    paid_at TIMESTAMP WITH TIME ZONE,
    due_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour la facturation multi-business
CREATE INDEX idx_billings_user_period ON billings(UserId, billing_period_start);
CREATE INDEX idx_billings_status ON billings(status);
CREATE INDEX idx_billings_due_date ON billings(due_date);
CREATE INDEX idx_billings_subscription_plan ON billings(SubscriptionPlanId);
CREATE INDEX idx_billings_unpaid_by_user ON billings(UserId, due_date) WHERE status IN ('pending', 'failed');

-- Contraintes de validation
ALTER TABLE billings ADD CONSTRAINT check_amounts_positive CHECK (total_amount_cents >= 0 AND base_price_cents >= 0);
ALTER TABLE billings ADD CONSTRAINT check_billing_status_valid CHECK (status IN ('pending', 'paid', 'failed', 'refunded', 'cancelled'));
ALTER TABLE billings ADD CONSTRAINT check_sms_usage_logical CHECK (sms_overage >= 0 AND sms_used >= 0);
ALTER TABLE billings ADD CONSTRAINT check_period_valid CHECK (billing_period_end > billing_period_start);
ALTER TABLE billings ADD CONSTRAINT check_businesses_count_positive CHECK (active_businesses_count > 0);
ALTER TABLE billings ADD CONSTRAINT check_billing_period_sequential CHECK (billing_period_start < billing_period_end);
ALTER TABLE billings ADD CONSTRAINT check_sms_overage_calculation CHECK (
    (sms_used <= sms_included AND sms_overage = 0) OR
    (sms_used > sms_included AND sms_overage = sms_used - sms_included)
);
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `UserId` : Référence vers l'utilisateur facturé
- `SubscriptionPlanId` : Référence vers le plan d'abonnement utilisé pour cette période
- `billing_period_start` : Date de début de la période de facturation
- `billing_period_end` : Date de fin de la période de facturation
- `base_price_cents` : Prix de base de l'abonnement en centimes
- `active_businesses_count` : Nombre d'établissements actifs pendant la période
- `sms_included` : Quota SMS compris dans l'abonnement mensuel
- `sms_used` : Nombre total de SMS consommés pendant la période
- `sms_overage` : SMS dépassant le quota (sms_used - sms_included si positif)
- `sms_overage_cost_cents` : Facturation supplémentaire à 3 centimes par SMS
- `sms_usage_by_business` : Détail JSON de la consommation par établissement
- `total_amount_cents` : Montant total de la facture en centimes
- `status` : État de la facture (pending/paid/failed/refunded/cancelled)
- `stripe_invoice_id` : Référence de la facture Stripe
- `stripe_payment_intent_id` : Référence Stripe pour le suivi des paiements
- `paid_at` : Timestamp de confirmation du paiement
- `due_date` : Date limite de paiement (généralement +30 jours)
- `created_at` : Timestamp de génération de la facture

### Table `system_configs`

**Description :** Configuration système centralisée incluant les paramètres spécifiques au multi-business comme les temps de service par défaut et les limites par plan.

```sql
CREATE TABLE system_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key VARCHAR(100) UNIQUE NOT NULL,
    value TEXT NOT NULL,
    data_type VARCHAR(20) DEFAULT 'string',
    description TEXT,
    is_public BOOLEAN DEFAULT false,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les accès fréquents
CREATE INDEX idx_system_configs_key ON system_configs(key);
CREATE INDEX idx_system_configs_public ON system_configs(is_public);
```

**Explications des colonnes :**

- `id` : Identifiant unique UUID généré automatiquement
- `key` : Clé unique de configuration (ex: "sms_cost_cents")
- `value` : Valeur de la configuration stockée en texte
- `data_type` : Type de donnée pour la validation (string/integer/decimal/boolean/json)
- `description` : Description explicative de ce paramètre de configuration
- `is_public` : Indique si cette configuration peut être lue par l'API publique
- `updated_at` : Timestamp de dernière modification de cette configuration

**Configuration initiale multi-business :**

```sql
INSERT INTO system_configs (key, value, data_type, description, is_public) VALUES
('sms_cost_cents', '3', 'integer', 'Coût unitaire SMS en centimes', false),
('trial_duration_days', '14', 'integer', 'Durée essai gratuit', true),
('max_queue_size_default', '50', 'integer', 'Taille max file par défaut', true),
('client_timeout_default', '5', 'integer', 'Timeout client par défaut (minutes)', true),
('default_service_times', '{"bakery": 120, "hairdresser": 2700, "pharmacy": 180, "garage": 1800, "restaurant": 5400, "medical_office": 900, "dentist": 1800, "veterinary": 1200, "optician": 1500, "bank": 600, "insurance": 1200, "notary": 2400, "lawyer": 3600, "accountant": 1800, "real_estate": 1800, "prefecture": 900, "city_hall": 600, "family_allowance": 1200, "employment_agency": 1800, "public_service": 900, "post_office": 300, "dry_cleaning": 180, "cobbler": 600, "watchmaker": 900, "phone_repair": 1200, "beauty_salon": 3600, "massage": 3600, "tattoo": 7200, "nail_salon": 2400, "barber": 1800, "vehicle_inspection": 1800, "gas_station": 300, "auto_body": 3600, "tire_service": 1200, "other": 900}', 'json', 'Temps service par défaut par type', true);
```

## Row Level Security (RLS) pour PostgreSQL

```sql
-- Activation RLS sur toutes les tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE businesses ENABLE ROW LEVEL SECURITY;
ALTER TABLE queue_entries ENABLE ROW LEVEL SECURITY;
ALTER TABLE sms_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE analytics_daily ENABLE ROW LEVEL SECURITY;
ALTER TABLE billings ENABLE ROW LEVEL SECURITY;

-- Politiques sécurisées multi-business (adapté pour PostgreSQL pur)
CREATE POLICY "Users manage own data" ON users
    FOR ALL USING (id = current_setting('app.current_user_id')::UUID);

CREATE POLICY "Users manage own businesses" ON businesses
    FOR ALL USING (UserId = current_setting('app.current_user_id')::UUID);

CREATE POLICY "Users access queues via businesses" ON queue_entries
    FOR ALL USING (current_setting('app.current_user_id')::UUID = (SELECT UserId FROM businesses WHERE id = BusinessId));

CREATE POLICY "Users access SMS logs via businesses" ON sms_logs
    FOR SELECT USING (current_setting('app.current_user_id')::UUID = (SELECT UserId FROM businesses WHERE id = BusinessId));

CREATE POLICY "Users access analytics via businesses" ON analytics_daily
    FOR SELECT USING (current_setting('app.current_user_id')::UUID = (SELECT UserId FROM businesses WHERE id = BusinessId));

CREATE POLICY "Users access own billing" ON billings
    FOR SELECT USING (UserId = current_setting('app.current_user_id')::UUID);

-- Accès public via QR code (avec context setting)
CREATE POLICY "Public queue access via QR token" ON queue_entries
    FOR SELECT USING (
        BusinessId IN (
            SELECT id FROM businesses
            WHERE qr_code_token = current_setting('app.current_business_token', true)
        )
    );
```

## Triggers et fonctions automatiques

```sql
-- Mise à jour automatique des timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$ language 'plpgsql';

-- Application sur toutes les tables avec updated_at
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_businesses_updated_at BEFORE UPDATE ON businesses FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_queue_entries_updated_at BEFORE UPDATE ON queue_entries FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_subscription_plans_updated_at BEFORE UPDATE ON subscription_plans FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Recalcul automatique des positions par business
CREATE OR REPLACE FUNCTION recalculate_queue_positions()
RETURNS TRIGGER AS $
BEGIN
    UPDATE queue_entries
    SET position = new_position
    FROM (
        SELECT id, ROW_NUMBER() OVER (ORDER BY created_at) as new_position
        FROM queue_entries
        WHERE BusinessId = COALESCE(NEW.BusinessId, OLD.BusinessId)
        AND status = 'waiting'
    ) AS positioned
    WHERE queue_entries.id = positioned.id;

    RETURN COALESCE(NEW, OLD);
END;
$ language 'plpgsql';

CREATE TRIGGER recalculate_positions_after_change
    AFTER UPDATE OF status OR DELETE ON queue_entries
    FOR EACH ROW EXECUTE FUNCTION recalculate_queue_positions();

-- Contrainte pour limiter les business selon le plan
CREATE OR REPLACE FUNCTION validate_business_count_on_plan_change()
RETURNS TRIGGER AS $
DECLARE
    current_businesses INTEGER;
    new_max_businesses INTEGER;
BEGIN
    -- Récupérer le nombre de business actifs
    SELECT COUNT(*) INTO current_businesses
    FROM businesses
    WHERE UserId = NEW.id AND is_active = true;

    -- Récupérer la nouvelle limite
    SELECT max_businesses INTO new_max_businesses
    FROM subscription_plans
    WHERE id = NEW.SubscriptionPlanId;

    -- Vérifier si le changement de plan est valide
    IF new_max_businesses != -1 AND current_businesses > new_max_businesses THEN
        RAISE EXCEPTION 'Cannot downgrade: user has % businesses but plan allows only %',
            current_businesses, new_max_businesses;
    END IF;

    RETURN NEW;
END;
$ language 'plpgsql';

CREATE TRIGGER validate_plan_change_trigger
    BEFORE UPDATE OF SubscriptionPlanId ON users
    FOR EACH ROW EXECUTE FUNCTION validate_business_count_on_plan_change();
```

## Contrôles multi-business

### Fonction de validation des limites par plan

```sql
CREATE OR REPLACE FUNCTION check_business_limit()
RETURNS TRIGGER AS $
DECLARE
    current_count INTEGER;
    max_allowed INTEGER;
    plan_name VARCHAR(100);
BEGIN
    -- Compter les business actifs de l'utilisateur
    SELECT COUNT(*) INTO current_count
    FROM businesses
    WHERE UserId = NEW.UserId AND is_active = true;

    -- Récupérer les limites du plan
    SELECT sp.max_businesses, sp.name INTO max_allowed, plan_name
    FROM users u
    JOIN subscription_plans sp ON u.SubscriptionPlanId = sp.id
    WHERE u.id = NEW.UserId;

    -- Vérifier la limite (-1 = illimité)
    IF max_allowed != -1 AND current_count >= max_allowed THEN
        RAISE EXCEPTION 'Plan % allows maximum % businesses. Upgrade required.', plan_name, max_allowed;
    END IF;

    RETURN NEW;
END;
$ language 'plpgsql';

CREATE TRIGGER check_business_limit_trigger
    BEFORE INSERT ON businesses
    FOR EACH ROW EXECUTE FUNCTION check_business_limit();
```

### Fonction de calcul de facturation multi-business

```sql
CREATE OR REPLACE FUNCTION calculate_monthly_billing(user_id UUID, period_start DATE, period_end DATE)
RETURNS TABLE(
    base_price INTEGER,
    businesses_count INTEGER,
    total_sms INTEGER,
    sms_overage INTEGER,
    overage_cost INTEGER,
    total_amount INTEGER,
    usage_detail JSONB
) AS $
DECLARE
    plan_info RECORD;
    sms_usage JSONB := '{}';
    business_rec RECORD;
    total_sms_used INTEGER := 0;
BEGIN
    -- Récupérer info du plan
    SELECT sp.price_cents, sp.sms_quota_monthly INTO plan_info
    FROM users u
    JOIN subscription_plans sp ON u.SubscriptionPlanId = sp.id
    WHERE u.id = user_id;

    -- Calculer usage SMS par business
    FOR business_rec IN
        SELECT b.id, b.name, COALESCE(SUM(1), 0) as sms_count
        FROM businesses b
        LEFT JOIN sms_logs sl ON b.id = sl.BusinessId
            AND sl.sent_at >= period_start
            AND sl.sent_at < period_end
            AND sl.status = 'sent'
        WHERE b.UserId = user_id AND b.is_active = true
        GROUP BY b.id, b.name
    LOOP
        sms_usage := jsonb_set(sms_usage, ARRAY[business_rec.id::text],
            jsonb_build_object('name', business_rec.name, 'sms_count', business_rec.sms_count));
        total_sms_used := total_sms_used + business_rec.sms_count;
    END LOOP;

    -- Calculer dépassement
    sms_overage := GREATEST(0, total_sms_used - plan_info.sms_quota_monthly);
    overage_cost := sms_overage * 3; -- 3 centimes par SMS

    RETURN QUERY SELECT
        plan_info.price_cents,
        (SELECT COUNT(*)::INTEGER FROM businesses WHERE UserId = user_id AND is_active = true),
        total_sms_used,
        sms_overage,
        overage_cost,
        plan_info.price_cents + overage_cost,
        jsonb_set(sms_usage, '{total}', total_sms_used::text::jsonb);
END;
$ language 'plpgsql';
```
