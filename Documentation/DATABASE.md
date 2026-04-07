# Base de données

**Mise à jour :** 03-04-2026

**Par :** [Steven YAMBOS](https://www.linkedin.com/in/steven-yambos/)

[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)

## Bonnes pratiques

- Les tables sont au pluriel et en PascalCase (exemple : `Users`).
- Les champs sont en PascalCase `Id`.
- Utilisation des UUID comme clés primaires pour éviter les collisions.
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

L'architecture permet à un utilisateur de gérer plusieurs établissements via des plans tarifaires adaptés. La séparation entre `Users` (compte utilisateur) et `Businesses` (établissements) garantit une évolutivité maximale.

### Relation utilisateur-business

- **1 utilisateur → N businesses** (selon le plan d'abonnement)
- **Plan Basic** : 1 business maximum
- **Plan Pro** : 5 businesses maximum
- **Plan Enterprise** : Illimité

## Tables principales

### Table `Users`

**Description :** Représente les comptes utilisateurs de la plateforme Waitify. Cette table stocke uniquement les informations personnelles et d'authentification. Les détails des établissements sont déportés dans la table `Businesses` pour supporter le multi-établissement.

```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    GoogleId VARCHAR(255),
    Email VARCHAR(255) UNIQUE NOT NULL,
    Password VARCHAR(255),
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    PhoneNumber VARCHAR(20),
    ProfilePicture VARCHAR(255),
    IsActive BOOLEAN DEFAULT true,
    -- EmailConfirmed BOOLEAN DEFAULT false, // remplace `IsActive`. Documentation -> https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityuser-1.emailconfirmed?view=aspnetcore-10.0#microsoft-aspnetcore-identity-identityuser-1-emailconfirmed
    AuthProvider VARCHAR(50) DEFAULT 'google',
    Role VARCHAR(50) DEFAULT 'Owner',
    SubscriptionStatus VARCHAR(50) DEFAULT 'trial',
    SubscriptionPlanId UUID REFERENCES SubscriptionPlans(Id),
    TrialEndsAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    LastLogin TIMESTAMP WITH TIME ZONE
);

-- Index pour les performances
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_subscription_plan ON Users(SubscriptionPlanId);
CREATE INDEX idx_users_subscription_status ON Users(SubscriptionStatus);
CREATE INDEX idx_users_active ON Users(IsActive) WHERE IsActive = true;

-- Contraintes de validation
ALTER TABLE Users ADD CONSTRAINT check_email_format CHECK (Email ~* '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$');
ALTER TABLE Users ADD CONSTRAINT check_subscription_status CHECK (SubscriptionStatus IN ('trial', 'active', 'suspended', 'cancelled'));
ALTER TABLE Users ADD CONSTRAINT check_auth_provider CHECK (AuthProvider IN ('google', 'facebook'));
ALTER TABLE Users ADD CONSTRAINT check_role CHECK (Role IN ('client', 'admin', 'owner'));
ALTER TABLE Users ADD CONSTRAINT check_phone_number_format CHECK (PhoneNumber IS NULL OR PhoneNumber ~ '^(\+33|0)[1-9][0-9]{8}$');
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `GoogleId` : Identifiant unique partagé par Google lors de l'inscription avec Google oAuth2
- `Email` : Adresse email unique servant d'identifiant de connexion
- `Password` : Hash bcrypt du mot de passe, jamais stocké en clair. ⚠️ Le mot de passe n'est pas `NOT NULL` car avec l'inscription avec Google on ne récupère pas le mot de passe de l'utilisateur ⚠️
- `FirstName` : Prénom de l'utilisateur
- `LastName` : Nom de famille de l'utilisateur
- `PhoneNumber` : Numéro de téléphone de contact
- `ProfilePicture` : Image de profile
- `IsActive` : Permet de suspendre un compte utilisateur globalement
- `AuthProvider` : Application de connexion
- `Role` : Rôle de l'utilisateur :
  - `Client` : Clients.
  - `Owner` : Commerçants.
  - `Admin` : Développeurs.
- `SubscriptionStatus` : État global de l'abonnement utilisateur
- `SubscriptionPlanId` : Référence vers le plan d'abonnement actuel
- `TrialEndsAt` : Date limite de la période d'essai gratuite de 14 jours
- `CreatedAt` : Timestamp de création du compte
- `UpdatedAt` : Timestamp de dernière modification
- `LastLogin` : Timestamp de dernière connexion

### Table `Businesses`

**Description :** Représente chaque établissement géré par un utilisateur. Cette table contient tous les paramètres opérationnels spécifiques à chaque point de vente : configuration de la file d'attente, horaires, messages personnalisés.

```sql
CREATE TABLE Businesses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OwnerId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Name VARCHAR(255) NOT NULL,
    BusinessType VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    Logo VARCHAR(255),
    Address TEXT,
    City VARCHAR(100),
    ZipCode VARCHAR(10),
    Country VARCHAR(50) DEFAULT 'France',
    QrCodeToken VARCHAR(255) UNIQUE NOT NULL,
    AverageServiceTime INTEGER DEFAULT 300,
    IsQueueActive BOOLEAN DEFAULT false,
    IsQueuePaused BOOLEAN DEFAULT false,
    MaxQueueSize INTEGER DEFAULT 50,
    OpeningHours JSONB,
    CustomMessage TEXT,
    SmsNotificationsEnabled BOOLEAN DEFAULT true,
    AutoAdvanceEnabled BOOLEAN DEFAULT true,
    ClientTimeoutMinutes INTEGER DEFAULT 5,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les performances multi-business
CREATE INDEX idx_businesses_user ON Businesses(OwnerId);
CREATE INDEX idx_businesses_user_active ON Businesses(OwnerId, IsActive);
CREATE UNIQUE INDEX idx_businesses_qr_token ON Businesses(QrCodeToken);
CREATE INDEX idx_businesses_type ON Businesses(BusinessType);
CREATE INDEX idx_businesses_active_by_user ON Businesses(OwnerId, CreatedAt) WHERE IsActive = true;

-- Contraintes de validation
ALTER TABLE Businesses ADD CONSTRAINT check_business_type CHECK (BusinessType IN (
    'bakery', 'hairdresser', 'pharmacy', 'garage', 'restaurant',
    'medical_office', 'dentist', 'veterinary', 'optician', 'bank',
    'insurance', 'notary', 'lawyer', 'accountant', 'real_estate',
    'prefecture', 'city_hall', 'family_allowance', 'employment_agency', 'public_service',
    'post_office', 'dry_cleaning', 'cobbler', 'watchmaker', 'phone_repair',
    'beauty_salon', 'massage', 'tattoo', 'nail_salon', 'barber',
    'vehicle_inspection', 'gas_station', 'auto_body', 'tire_service',
    'other'
));
ALTER TABLE Businesses ADD CONSTRAINT check_service_time_positive CHECK (AverageServiceTime > 0);
ALTER TABLE Businesses ADD CONSTRAINT check_max_queue_reasonable CHECK (MaxQueueSize BETWEEN 1 AND 200);
ALTER TABLE Businesses ADD CONSTRAINT check_timeout_reasonable CHECK (ClientTimeoutMinutes BETWEEN 1 AND 30);
ALTER TABLE Businesses ADD CONSTRAINT check_phone_number_format_business CHECK (PhoneNumber IS NULL OR PhoneNumber ~ '^(\+33|0)[1-9][0-9]{8}$');
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `OwnerId` : Référence vers le propriétaire utilisateur de l'établissement
- `Name` : Nom commercial de l'établissement (ex: "Boulangerie Martin Centre-Ville")
- `BusinessType` : Type d'activité utilisé pour les temps de service par défaut
- `PhoneNumber` : Numéro de téléphone spécifique à cet établissement
- `Logo` : Logo du commerce
- `Address` : Adresse physique complète de l'établissement
- `City` : Ville où se situe l'établissement
- `ZipCode` : Code postal de l'établissement
- `Country` : Pays de l'établissement (par défaut France)
- `QrCodeToken` : Token unique pour identifier l'établissement via QR code
- `AverageServiceTime` : Temps moyen en secondes pour servir un client
- `IsQueueActive` : Contrôle global de la file d'attente (ouverte/fermée)
- `IsQueuePaused` : Pause temporaire sans fermer complètement
- `MaxQueueSize` : Limite du nombre de clients simultanés
- `OpeningHours` : Horaires d'ouverture au format JSON par jour
- `CustomMessage` : Message personnalisé inclus dans les SMS aux clients
- `SmsNotificationsEnabled` : Active/désactive l'envoi de SMS pour cet établissement
- `AutoAdvanceEnabled` : Active le passage automatique au client suivant après timeout
- `ClientTimeoutMinutes` : Délai avant passage automatique au suivant
- `IsActive` : Permet de désactiver temporairement un établissement
- `CreatedAt` : Timestamp de création de l'établissement
- `UpdatedAt` : Timestamp de dernière modification

**Format JSON pour OpeningHours :**

```json
{
  "monday": { "open": "08:00", "close": "18:00", "closed": false },
  "tuesday": { "open": "08:00", "close": "18:00", "closed": false },
  "wednesday": { "open": "08:00", "close": "12:00", "closed": false },
  "thursday": { "open": "08:00", "close": "18:00", "closed": false },
  "friday": { "open": "08:00", "close": "18:00", "closed": false },
  "saturday": { "open": "08:00", "close": "17:00", "closed": false },
  "sunday": { "closed": true }
}
```

### Table `QueueEntries`

**Description :** Gère les inscriptions dans les files d'attente de chaque établissement. Cette table est le cœur opérationnel du système, stockant les positions, estimations de temps et le cycle de vie complet de chaque client.

```sql
CREATE TABLE QueueEntries (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES Businesses(Id) ON DELETE CASCADE,
    Phone VARCHAR(20) NOT NULL,
    ClientName VARCHAR(100),
    Position INTEGER NOT NULL,
    EstimatedWaitTime INTEGER,
    Status VARCHAR(50) DEFAULT 'waiting',
    CalledAt TIMESTAMP WITH TIME ZONE,
    ServedAt TIMESTAMP WITH TIME ZONE,
    ActualServiceTime INTEGER,
    SmsSentCount INTEGER DEFAULT 0,
    LastSmsSentAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index optimisés pour le multi-business
CREATE INDEX idx_queue_entries_business_status ON QueueEntries(BusinessId, Status);
CREATE INDEX idx_queue_entries_active_position ON QueueEntries(BusinessId, Position) WHERE Status = 'waiting';
CREATE INDEX idx_queue_entries_business_created ON QueueEntries(BusinessId, CreatedAt);
CREATE INDEX idx_queue_entries_phone_business ON QueueEntries(Phone, BusinessId);
CREATE INDEX idx_queue_entries_waiting_by_business ON QueueEntries(BusinessId, Position, CreatedAt) WHERE Status = 'waiting';

-- Index pour requêtes cross-business (performance)
CREATE INDEX idx_queue_entries_user_status ON QueueEntries(
    (SELECT OwnerId FROM Businesses WHERE Id = BusinessId),
    Status,
    CreatedAt
);

-- Contraintes de validation
ALTER TABLE QueueEntries ADD CONSTRAINT check_position_positive CHECK (Position > 0);
ALTER TABLE QueueEntries ADD CONSTRAINT check_status_valid CHECK (Status IN ('waiting', 'called', 'served', 'missed', 'cancelled'));
ALTER TABLE QueueEntries ADD CONSTRAINT check_phone_format CHECK (Phone ~ '^(\+33|0)[1-9][0-9]{8}$');
ALTER TABLE QueueEntries ADD CONSTRAINT check_estimated_wait_positive CHECK (EstimatedWaitTime IS NULL OR EstimatedWaitTime >= 0);
ALTER TABLE QueueEntries ADD CONSTRAINT check_called_before_served CHECK (CalledAt IS NULL OR ServedAt IS NULL OR ServedAt >= CalledAt);
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement concerné
- `Phone` : Numéro de téléphone du client (format français validé)
- `ClientName` : Nom ou prénom du client (optionnel)
- `Position` : Rang dans la file d'attente, recalculé automatiquement
- `EstimatedWaitTime` : Temps d'attente estimé en minutes au moment de l'inscription
- `Status` : État du client dans le processus (waiting/called/served/missed/cancelled)
- `CalledAt` : Timestamp précis de l'appel du client par le commerçant
- `ServedAt` : Timestamp de confirmation du service effectué
- `ActualServiceTime` : Durée réelle du service en secondes pour améliorer les estimations
- `SmsSentCount` : Nombre total de SMS envoyés à ce client pour le billing
- `LastSmsSentAt` : Timestamp du dernier SMS pour éviter le spam
- `CreatedAt` : Timestamp d'inscription dans la file d'attente
- `UpdatedAt` : Timestamp de dernière modification du statut

**Cycle de vie d'une entrée :**

1. `waiting` : Client inscrit, en attente de son tour
2. `called` : Commerçant a appelé le client (SMS envoyé)
3. `served` : Client servi avec succès
4. `missed` : Client absent lors de son appel (timeout)
5. `cancelled` : Client a annulé sa place manuellement

### Table `SubscriptionPlans`

**Description :** Définit les différents plans tarifaires avec leurs limites et fonctionnalités. Cette table permet une gestion flexible des offres commerciales et une évolution tarifaire sans modification du code.

```sql
CREATE TABLE SubscriptionPlans (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) UNIQUE NOT NULL,
    PriceCents INTEGER NOT NULL,
    MaxBusinesses INTEGER NOT NULL,
    SmsQuotaMonthly INTEGER DEFAULT 1000,
    Features JSONB,
    IsActive BOOLEAN DEFAULT true,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les requêtes fréquentes
CREATE INDEX idx_subscription_plans_active ON SubscriptionPlans(IsActive);
CREATE INDEX idx_subscription_plans_name ON SubscriptionPlans(Name);

-- Contraintes de validation
ALTER TABLE SubscriptionPlans ADD CONSTRAINT check_price_positive CHECK (PriceCents >= 0);
ALTER TABLE SubscriptionPlans ADD CONSTRAINT check_max_businesses_valid CHECK (MaxBusinesses = -1 OR MaxBusinesses > 0);
ALTER TABLE SubscriptionPlans ADD CONSTRAINT check_sms_quota_positive CHECK (SmsQuotaMonthly > 0);
```

**Plans par défaut :**

```sql
INSERT INTO SubscriptionPlans (Name, PriceCents, MaxBusinesses, SmsQuotaMonthly, Features) VALUES
('basic', 1900, 1, 1000, '{"analytics": "basic", "support": "email", "api_access": false}'),
('pro', 4900, 5, 2500, '{"analytics": "advanced", "support": "priority", "api_access": true, "custom_branding": true}'),
('enterprise', 9900, -1, 5000, '{"analytics": "advanced", "support": "phone", "api_access": true, "custom_branding": true, "dedicated_manager": true}');
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `Name` : Nom unique du plan affiché à l'utilisateur
- `PriceCents` : Prix mensuel en centimes d'euro
- `MaxBusinesses` : Nombre maximum d'établissements autorisés (-1 pour illimité)
- `SmsQuotaMonthly` : Quota de SMS inclus dans l'abonnement mensuel
- `Features` : Fonctionnalités JSON incluses dans le plan
- `IsActive` : Indique si le plan est proposable aux nouveaux clients
- `CreatedAt` : Timestamp de création du plan
- `UpdatedAt` : Timestamp de dernière modification

### Table `SmsLogs`

**Description :** Journal exhaustif de tous les SMS envoyés par établissement. Essentiel pour la facturation multi-business, l'audit et le monitoring des performances par établissement.

```sql
CREATE TABLE SmsLogs (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES Businesses(Id) ON DELETE CASCADE,
    QueueEntryId UUID REFERENCES QueueEntries(Id) ON DELETE SET NULL,
    Phone VARCHAR(20) NOT NULL,
    MessageType VARCHAR(50) NOT NULL,
    MessageContent TEXT NOT NULL,
    Status VARCHAR(50) DEFAULT 'pending',
    ProviderResponse JSONB,
    CostCents INTEGER DEFAULT 3,
    SentAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    DeliveredAt TIMESTAMP WITH TIME ZONE
);

-- Index pour l'analyse multi-business
CREATE INDEX idx_sms_logs_business_date ON SmsLogs(BusinessId, SentAt);
CREATE INDEX idx_sms_logs_business_type ON SmsLogs(BusinessId, MessageType);
CREATE INDEX idx_sms_logs_user_period ON SmsLogs((SELECT OwnerId FROM Businesses WHERE Id = BusinessId), SentAt);
CREATE INDEX idx_sms_logs_status ON SmsLogs(Status);

-- Contraintes de validation
ALTER TABLE SmsLogs ADD CONSTRAINT check_message_type_valid CHECK (MessageType IN ('confirmation', 'reminder', 'your_turn', 'missed', 'cancelled'));
ALTER TABLE SmsLogs ADD CONSTRAINT check_sms_status_valid CHECK (Status IN ('pending', 'sent', 'delivered', 'failed'));
ALTER TABLE SmsLogs ADD CONSTRAINT check_cost_positive CHECK (CostCents >= 0);
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement qui a envoyé le SMS
- `QueueEntryId` : Référence vers l'entrée de queue concernée (optionnel pour SMS génériques)
- `Phone` : Numéro de téléphone destinataire du SMS
- `MessageType` : Catégorie du SMS pour classifier les communications
- `MessageContent` : Texte exact envoyé, stocké pour audit et debugging
- `Status` : État de livraison du SMS (pending/sent/delivered/failed)
- `ProviderResponse` : Réponse JSON complète de l'API SMS pour troubleshooting
- `CostCents` : Coût unitaire en centimes pour la facturation précise
- `SentAt` : Timestamp d'envoi du SMS
- `DeliveredAt` : Confirmation de livraison par l'opérateur (webhook)

**Types de messages SMS :**

- `confirmation` : "Votre place #3 chez [Business] est confirmée, temps d'attente: 12min"
- `reminder` : "Plus que 2 clients devant vous chez [Business]"
- `your_turn` : "C'est votre tour chez [Business] ! Présentez-vous au comptoir"
- `missed` : "Votre tour chez [Business] est passé. Rescannez le QR code"
- `cancelled` : "Votre place chez [Business] a été annulée"

### Table `AnalyticsDaily`

**Description :** Métriques quotidiennes par établissement pour des tableaux de bord performants. Permet des comparaisons entre établissements d'un même utilisateur et des analyses de performance globales.

```sql
CREATE TABLE AnalyticsDaily (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    BusinessId UUID NOT NULL REFERENCES Businesses(Id) ON DELETE CASCADE,
    Date DATE NOT NULL,
    TotalClientsServed INTEGER DEFAULT 0,
    TotalClientsMissed INTEGER DEFAULT 0,
    TotalClientsCancelled INTEGER DEFAULT 0,
    TotalClientsRegistered INTEGER DEFAULT 0,
    AverageWaitTime INTEGER,
    AverageServiceTime INTEGER,
    PeakHour INTEGER,
    PeakQueueSize INTEGER,
    AbandonmentRate DECIMAL(5,2),
    SmsSentCount INTEGER DEFAULT 0,
    RevenuePotentialLost INTEGER DEFAULT 0,
    BusiestTimeStart TIME,
    BusiestTimeEnd TIME,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(BusinessId, Date)
);

-- Index pour les analyses multi-business
CREATE INDEX idx_analytics_daily_business_date ON AnalyticsDaily(BusinessId, Date DESC);
CREATE INDEX idx_analytics_daily_user_date ON AnalyticsDaily((SELECT OwnerId FROM Businesses WHERE Id = BusinessId), Date);
CREATE INDEX idx_analytics_daily_date ON AnalyticsDaily(Date);

-- Contraintes de validation
ALTER TABLE AnalyticsDaily ADD CONSTRAINT check_abandonment_rate_valid CHECK (AbandonmentRate >= 0 AND AbandonmentRate <= 100);
ALTER TABLE AnalyticsDaily ADD CONSTRAINT check_peak_hour_valid CHECK (PeakHour IS NULL OR (PeakHour >= 0 AND PeakHour <= 23));
ALTER TABLE AnalyticsDaily ADD CONSTRAINT check_totals_positive CHECK (
    TotalClientsServed >= 0 AND
    TotalClientsMissed >= 0 AND
    TotalClientsCancelled >= 0 AND
    TotalClientsRegistered >= 0
);
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `BusinessId` : Référence vers l'établissement concerné par ces statistiques
- `Date` : Date des statistiques (unique par établissement)
- `TotalClientsServed` : Nombre de clients effectivement servis dans la journée
- `TotalClientsMissed` : Nombre de clients qui ont manqué leur tour (timeout)
- `TotalClientsCancelled` : Nombre de clients qui ont annulé leur place
- `TotalClientsRegistered` : Nombre total d'inscriptions dans la journée
- `AverageWaitTime` : Temps d'attente moyen en minutes pour cette journée
- `AverageServiceTime` : Temps de service moyen en secondes par client
- `PeakHour` : Heure (0-23) avec la plus longue file d'attente
- `PeakQueueSize` : Taille maximum de la file atteinte dans la journée
- `AbandonmentRate` : Pourcentage de clients ayant annulé ou manqué leur tour
- `SmsSentCount` : Nombre total de SMS envoyés dans la journée
- `RevenuePotentialLost` : Estimation du manque à gagner des abandons en centimes
- `BusiestTimeStart` : Heure de début de la période la plus chargée
- `BusiestTimeEnd` : Heure de fin de la période la plus chargée
- `CreatedAt` : Timestamp de génération de ces statistiques

### Table `Billings`

**Description :** Facturation consolidée par utilisateur incluant la consommation de tous ses établissements. Gère les abonnements multi-business avec détail de l'usage par établissement.

```sql
CREATE TABLE Billings (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    SubscriptionPlanId UUID NOT NULL REFERENCES SubscriptionPlans(Id),
    BillingPeriodStart DATE NOT NULL,
    BillingPeriodEnd DATE NOT NULL,
    BasePriceCents INTEGER NOT NULL,
    ActiveBusinessesCount INTEGER DEFAULT 1,
    SmsIncluded INTEGER DEFAULT 1000,
    SmsUsed INTEGER DEFAULT 0,
    SmsOverage INTEGER DEFAULT 0,
    SmsOverageCostCents INTEGER DEFAULT 0,
    SmsUsageByBusiness JSONB,
    TotalAmountCents INTEGER NOT NULL,
    Status VARCHAR(50) DEFAULT 'pending',
    StripeInvoiceId VARCHAR(255),
    StripePaymentIntentId VARCHAR(255),
    PaidAt TIMESTAMP WITH TIME ZONE,
    DueDate DATE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour la facturation multi-business
CREATE INDEX idx_billings_user_period ON Billings(UserId, BillingPeriodStart);
CREATE INDEX idx_billings_status ON Billings(Status);
CREATE INDEX idx_billings_due_date ON Billings(DueDate);
CREATE INDEX idx_billings_subscription_plan ON Billings(SubscriptionPlanId);
CREATE INDEX idx_billings_unpaid_by_user ON Billings(UserId, DueDate) WHERE Status IN ('pending', 'failed');

-- Contraintes de validation
ALTER TABLE Billings ADD CONSTRAINT check_amounts_positive CHECK (TotalAmountCents >= 0 AND BasePriceCents >= 0);
ALTER TABLE Billings ADD CONSTRAINT check_billing_status_valid CHECK (Status IN ('pending', 'paid', 'failed', 'refunded', 'cancelled'));
ALTER TABLE Billings ADD CONSTRAINT check_sms_usage_logical CHECK (SmsOverage >= 0 AND SmsUsed >= 0);
ALTER TABLE Billings ADD CONSTRAINT check_period_valid CHECK (BillingPeriodEnd > BillingPeriodStart);
ALTER TABLE Billings ADD CONSTRAINT check_businesses_count_positive CHECK (ActiveBusinessesCount > 0);
ALTER TABLE Billings ADD CONSTRAINT check_billing_period_sequential CHECK (BillingPeriodStart < BillingPeriodEnd);
ALTER TABLE Billings ADD CONSTRAINT check_sms_overage_calculation CHECK (
    (SmsUsed <= SmsIncluded AND SmsOverage = 0) OR
    (SmsUsed > SmsIncluded AND SmsOverage = SmsUsed - SmsIncluded)
);
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `UserId` : Référence vers l'utilisateur facturé
- `SubscriptionPlanId` : Référence vers le plan d'abonnement utilisé pour cette période
- `BillingPeriodStart` : Date de début de la période de facturation
- `BillingPeriodEnd` : Date de fin de la période de facturation
- `BasePriceCents` : Prix de base de l'abonnement en centimes
- `ActiveBusinessesCount` : Nombre d'établissements actifs pendant la période
- `SmsIncluded` : Quota SMS compris dans l'abonnement mensuel
- `SmsUsed` : Nombre total de SMS consommés pendant la période
- `SmsOverage` : SMS dépassant le quota (SmsUsed - SmsIncluded si positif)
- `SmsOverageCostCents` : Facturation supplémentaire à 3 centimes par SMS
- `SmsUsageByBusiness` : Détail JSON de la consommation par établissement
- `TotalAmountCents` : Montant total de la facture en centimes
- `Status` : État de la facture (pending/paid/failed/refunded/cancelled)
- `StripeInvoiceId` : Référence de la facture Stripe
- `StripePaymentIntentId` : Référence Stripe pour le suivi des paiements
- `PaidAt` : Timestamp de confirmation du paiement
- `DueDate` : Date limite de paiement (généralement +30 jours)
- `CreatedAt` : Timestamp de génération de la facture

### Table `SystemConfigs`

**Description :** Configuration système centralisée incluant les paramètres spécifiques au multi-business comme les temps de service par défaut et les limites par plan.

```sql
CREATE TABLE SystemConfigs (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Key VARCHAR(100) UNIQUE NOT NULL,
    Value TEXT NOT NULL,
    DataType VARCHAR(20) DEFAULT 'string',
    Description TEXT,
    IsPublic BOOLEAN DEFAULT false,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index pour les accès fréquents
CREATE INDEX idx_system_configs_key ON SystemConfigs(Key);
CREATE INDEX idx_system_configs_public ON SystemConfigs(IsPublic);
```

**Explications des colonnes :**

- `Id` : Identifiant unique UUID généré automatiquement
- `Key` : Clé unique de configuration (ex: "sms_cost_cents")
- `Value` : Valeur de la configuration stockée en texte
- `DataType` : Type de donnée pour la validation (string/integer/decimal/boolean/json)
- `Description` : Description explicative de ce paramètre de configuration
- `IsPublic` : Indique si cette configuration peut être lue par l'API publique
- `UpdatedAt` : Timestamp de dernière modification de cette configuration

**Configuration initiale multi-business :**

```sql
INSERT INTO SystemConfigs (Key, Value, DataType, Description, IsPublic) VALUES
('sms_cost_cents', '3', 'integer', 'Coût unitaire SMS en centimes', false),
('trial_duration_days', '14', 'integer', 'Durée essai gratuit', true),
('max_queue_size_default', '50', 'integer', 'Taille max file par défaut', true),
('client_timeout_default', '5', 'integer', 'Timeout client par défaut (minutes)', true),
('default_service_times', '{"bakery": 120, "hairdresser": 2700, "pharmacy": 180, "garage": 1800, "restaurant": 5400, "medical_office": 900, "dentist": 1800, "veterinary": 1200, "optician": 1500, "bank": 600, "insurance": 1200, "notary": 2400, "lawyer": 3600, "accountant": 1800, "real_estate": 1800, "prefecture": 900, "city_hall": 600, "family_allowance": 1200, "employment_agency": 1800, "public_service": 900, "post_office": 300, "dry_cleaning": 180, "cobbler": 600, "watchmaker": 900, "phone_repair": 1200, "beauty_salon": 3600, "massage": 3600, "tattoo": 7200, "nail_salon": 2400, "barber": 1800, "vehicle_inspection": 1800, "gas_station": 300, "auto_body": 3600, "tire_service": 1200, "other": 900}', 'json', 'Temps service par défaut par type', true);
```

## Row Level Security (RLS) pour PostgreSQL

```sql
-- Activation RLS sur toutes les tables
ALTER TABLE Users ENABLE ROW LEVEL SECURITY;
ALTER TABLE Businesses ENABLE ROW LEVEL SECURITY;
ALTER TABLE QueueEntries ENABLE ROW LEVEL SECURITY;
ALTER TABLE SmsLogs ENABLE ROW LEVEL SECURITY;
ALTER TABLE AnalyticsDaily ENABLE ROW LEVEL SECURITY;
ALTER TABLE Billings ENABLE ROW LEVEL SECURITY;

-- Politiques sécurisées multi-business (adapté pour PostgreSQL pur)
CREATE POLICY "Users manage own data" ON Users
    FOR ALL USING (Id = current_setting('app.current_user_id')::UUID);

CREATE POLICY "Users manage own businesses" ON Businesses
    FOR ALL USING (OwnerId = current_setting('app.current_user_id')::UUID);

CREATE POLICY "Users access queues via businesses" ON QueueEntries
    FOR ALL USING (current_setting('app.current_user_id')::UUID = (SELECT OwnerId FROM Businesses WHERE Id = BusinessId));

CREATE POLICY "Users access SMS logs via businesses" ON SmsLogs
    FOR SELECT USING (current_setting('app.current_user_id')::UUID = (SELECT OwnerId FROM Businesses WHERE Id = BusinessId));

CREATE POLICY "Users access analytics via businesses" ON AnalyticsDaily
    FOR SELECT USING (current_setting('app.current_user_id')::UUID = (SELECT OwnerId FROM Businesses WHERE Id = BusinessId));

CREATE POLICY "Users access own billing" ON Billings
    FOR SELECT USING (UserId = current_setting('app.current_user_id')::UUID);

-- Accès public via QR code (avec context setting)
CREATE POLICY "Public queue access via QR token" ON QueueEntries
    FOR SELECT USING (
        BusinessId IN (
            SELECT Id FROM Businesses
            WHERE QrCodeToken = current_setting('app.current_business_token', true)
        )
    );
```

## Triggers et fonctions automatiques

```sql
-- Mise à jour automatique des timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $
BEGIN
    NEW.UpdatedAt = NOW();
    RETURN NEW;
END;
$ language 'plpgsql';

-- Application sur toutes les tables avec UpdatedAt
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON Users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_businesses_updated_at BEFORE UPDATE ON Businesses FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_queue_entries_updated_at BEFORE UPDATE ON QueueEntries FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_subscription_plans_updated_at BEFORE UPDATE ON SubscriptionPlans FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- Recalcul automatique des positions par business
CREATE OR REPLACE FUNCTION recalculate_queue_positions()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE "QueueEntries"
    SET "Position" = new_position
    FROM (
        SELECT "Id", ROW_NUMBER() OVER (ORDER BY "CreatedAt") as new_position
        FROM "QueueEntries"
        WHERE "BusinessId" = COALESCE(NEW."BusinessId", OLD."BusinessId")
        AND "Status" = 'waiting'
    ) AS positioned
    WHERE "QueueEntries"."Id" = positioned."Id";
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER recalculate_positions_after_change
    AFTER UPDATE OF "Status" OR DELETE ON "QueueEntries"
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
    FROM Businesses
    WHERE OwnerId = NEW.Id AND IsActive = true;

    -- Récupérer la nouvelle limite
    SELECT MaxBusinesses INTO new_max_businesses
    FROM SubscriptionPlans
    WHERE Id = NEW.SubscriptionPlanId;

    -- Vérifier si le changement de plan est valide
    IF new_max_businesses != -1 AND current_businesses > new_max_businesses THEN
        RAISE EXCEPTION 'Cannot downgrade: user has % businesses but plan allows only %',
            current_businesses, new_max_businesses;
    END IF;

    RETURN NEW;
END;
$ language 'plpgsql';

CREATE TRIGGER validate_plan_change_trigger
    BEFORE UPDATE OF SubscriptionPlanId ON Users
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
    FROM Businesses
    WHERE OwnerId = NEW.OwnerId AND IsActive = true;

    -- Récupérer les limites du plan
    SELECT sp.MaxBusinesses, sp.Name INTO max_allowed, plan_name
    FROM Users u
    JOIN SubscriptionPlans sp ON u.SubscriptionPlanId = sp.Id
    WHERE u.Id = NEW.OwnerId;

    -- Vérifier la limite (-1 = illimité)
    IF max_allowed != -1 AND current_count >= max_allowed THEN
        RAISE EXCEPTION 'Plan % allows maximum % businesses. Upgrade required.', plan_name, max_allowed;
    END IF;

    RETURN NEW;
END;
$ language 'plpgsql';

CREATE TRIGGER check_business_limit_trigger
    BEFORE INSERT ON Businesses
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
    SELECT sp.PriceCents, sp.SmsQuotaMonthly INTO plan_info
    FROM Users u
    JOIN SubscriptionPlans sp ON u.SubscriptionPlanId = sp.Id
    WHERE u.Id = user_id;

    -- Calculer usage SMS par business
    FOR business_rec IN
        SELECT b.Id, b.Name, COALESCE(SUM(1), 0) as sms_count
        FROM Businesses b
        LEFT JOIN SmsLogs sl ON b.Id = sl.BusinessId
            AND sl.SentAt >= period_start
            AND sl.SentAt < period_end
            AND sl.Status = 'sent'
        WHERE b.OwnerId = user_id AND b.IsActive = true
        GROUP BY b.Id, b.Name
    LOOP
        sms_usage := jsonb_set(sms_usage, ARRAY[business_rec.Id::text],
            jsonb_build_object('name', business_rec.Name, 'sms_count', business_rec.sms_count));
        total_sms_used := total_sms_used + business_rec.sms_count;
    END LOOP;

    -- Calculer dépassement
    sms_overage := GREATEST(0, total_sms_used - plan_info.SmsQuotaMonthly);
    overage_cost := sms_overage * 3; -- 3 centimes par SMS

    RETURN QUERY SELECT
        plan_info.PriceCents,
        (SELECT COUNT(*)::INTEGER FROM Businesses WHERE OwnerId = user_id AND IsActive = true),
        total_sms_used,
        sms_overage,
        overage_cost,
        plan_info.PriceCents + overage_cost,
        jsonb_set(sms_usage, '{total}', total_sms_used::text::jsonb);
END;
$ language 'plpgsql';
```
